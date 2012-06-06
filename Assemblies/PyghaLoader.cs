#if GH_0_9

using System;
using GhPython.Forms;
using Grasshopper.Kernel;
using Rhino.Runtime;
using System.IO;
using System.Collections.Generic;
using GhPython.Component;
using System.Reflection;
using System.Collections;
using System.Windows.Forms;
using System.Security;
using Grasshopper;
using System.Runtime.InteropServices;

namespace GhPython.Assemblies
{
  public class GhpyLoader : GH_AssemblyPriority
  {
    PythonEnvironment _gha_environment;

    public override GH_LoadingInstruction PriorityLoad()
    {
      try
      {
        _gha_environment = CreateEnvironment();
        LoadExternalPythonAssemblies();
        SetupMainDirListener();
      }
      catch (Exception ex)
      {
        Global_Proc.ASSERT(Guid.Empty, "GhPython last exception boundary", ex);
      }
      return GH_LoadingInstruction.Proceed;
    }

    private PythonEnvironment CreateEnvironment()
    {
      var externalPy = PythonScript.Create();
      return new PythonEnvironment(null, externalPy);
    }

    private void SetupMainDirListener()
    {
      if (Directory.Exists(GH_ComponentServer.GHA_AppDataDirectory))
      {
        var watcher = GH_FileWatcher.CreateDirectoryWatcher(GH_ComponentServer.GHA_AppDataDirectory, "*.ghpy", GH_FileWatcherEvents.Created,
          (sender, filePath, change) =>
          {
            try
            {
              if (change == WatcherChangeTypes.Created)
              {
                if (LoadOneAddon(_gha_environment, filePath))
                {
                  GH_ComponentServer.UpdateRibbonUI();
                }
              }
            }
            catch (Exception ex)
            {
              Global_Proc.ASSERT(Guid.Empty, "GhPython last exception boundary", ex);
            }
          });
        watcher.Active = true;
      }
    }

    private void LoadExternalPythonAssemblies()
    {
      var allGhas = GetAllPygha();

      foreach (var path in allGhas)
      {
        LoadOneAddon(_gha_environment, path);
      }
    }


    static class ExternalUnsafe
    {
      [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      private static extern bool DeleteFile(string name);

      const int FILE_ATTRIBUTE_DIRECTORY = 0x10;

      [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
      static extern int GetFileAttributes(string lpFileName);

      public static bool HasZoneIdetifier(string fileName)
      {
        if (!File.Exists(fileName)) return false;
        var val = GetFileAttributes(fileName + ":Zone.Identifier");
        return (FILE_ATTRIBUTE_DIRECTORY & val) != FILE_ATTRIBUTE_DIRECTORY;
      }

      public static bool Unblock(string fileName)
      {
        if (!File.Exists(fileName)) return false;
        return DeleteFile(fileName + ":Zone.Identifier");
      }
    }


    private static bool LoadOneAddon(PythonEnvironment p, string path)
    {
      var engine = p.Engine as dynamic;
      var runtime = engine.Runtime;
      var ops = engine.Operations;

      if(ExternalUnsafe.HasZoneIdetifier(path))
      {
        if (MessageBox.Show("A FILE IS BLOCKED: \n\n" +
          path +
          "\n\nBefore being able to use it, this file should be unblocked.\n" +
          "Do you want attempt to unblock it now?", "GhPython Assembly is blocked",
          MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2,
          MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
        {
          if (!ExternalUnsafe.Unblock(path))
          {
            Global_Proc.ASSERT(Guid.Empty, "You need to unblock \"" + path + "\" manually."); return false;
          }
        }
      }

      AssemblyName assName;
      try
      {
        assName = AssemblyName.GetAssemblyName(path);
      }
      catch (SecurityException ex)
      {
        Global_Proc.ASSERT(Guid.Empty, "You have not enough rights to load \"" + path + "\".", ex); return false;
      }
      catch (BadImageFormatException ex)
      {
        Global_Proc.ASSERT(Guid.Empty, "The assembly \"" + path + "\" has a bad format.", ex); return false;
      }
      catch (FileLoadException ex)
      {
        Global_Proc.ASSERT(Guid.Empty, "The assembly \"" + path + "\" is found but cannot be loaded.", ex); return false;
      }

      var appDomain = AppDomain.CreateDomain("Temp");
      try
      {
        var farAssembly = appDomain.CreateInstanceFrom(path, "DLRCachedCode");
      }
      catch (FileLoadException ex)
      {
        int error = Marshal.GetHRForException(ex);
        if (error == -0x40131515)
        {
          Global_Proc.ASSERT(Guid.Empty, "The file \"" + path + "\" is blocked.", ex); return false;
        }
        Global_Proc.ASSERT(Guid.Empty, "The assembly at \"" + path + "\" cannot be loaded.", ex); return false;
      }
      catch (BadImageFormatException ex)
      {
        Global_Proc.ASSERT(Guid.Empty, "This assembly \"" + path + "\" has a bad inner format.", ex); return false;
      }
      catch (TypeLoadException ex)
      {
        Global_Proc.ASSERT(Guid.Empty, "\"" + path + "\" is not a valid Python assembly. Please remove it.", ex); return false;
      }
      catch (MissingMethodException ex)
      {
        Global_Proc.ASSERT(Guid.Empty, "This assembly \"" + path + "\" is ruined.", ex); return false;
      }
      finally
      {
        if (appDomain != null)
        {
          AppDomain.Unload(appDomain);
        }
      }

      Assembly assembly = Assembly.LoadFile(path);
      var cachedCode = assembly.GetType("DLRCachedCode", false, false);

      if (cachedCode == null) return false; //should be already ruled out

      dynamic info = cachedCode.InvokeMember("GetScriptCodeInfo",
        BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static,
        null, null, new object[0]);
      string[] modules = info.GetValue(2)[0];

      runtime.LoadAssembly(assembly);

      bool toReturn = false;

      foreach (var module in modules)
      {
        var statement = "import " + module;
        p.Script.ExecuteScript(statement);

        dynamic ns = p.Script.GetVariable(module);

        var dict = ns.Get__dict__();
        var vars = dict.Keys;

        foreach (var v in vars)
        {
          var text = v as string;

          if (text == null) continue;
          object o = dict[text];

          if (o == null) continue;
          Type type = o.GetType();

          if (type.FullName != "IronPython.Runtime.Types.PythonType") continue;

          var basesEnum = (IEnumerable)type.InvokeMember(
            "get_BaseTypes",
            BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance, null, o, null);
          if (basesEnum == null) continue;

          foreach (var baseObj in basesEnum)
          {
            Type finalSystemType = (Type)baseObj.GetType().InvokeMember(
              "get_FinalSystemType",
              BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance, null, baseObj, null);
            if (finalSystemType == null) continue;

            if (typeof(IGH_Component).IsAssignableFrom(finalSystemType))
            {
              var instance = Instantiate(ops as object, o);
              var proxy = new PythonInstantiatorProxy(instance, o, ops as object, path);

              toReturn |= Grasshopper.GH_InstanceServer.ComponentServer.AddProxy(proxy);
            }
          }
        }

        p.Script.ExecuteScript("del " + module);
      }
      return toReturn;
    }

    private static IGH_Component Instantiate(object engineOperations, object pythonType)
    {
      return (engineOperations as dynamic).Invoke(pythonType);
    }

    private static IEnumerable<string> GetAllPygha()
    {
      foreach (var path in GetPathsToBeSearched())
      {
        string[] files = Directory.GetFiles(path, "*.ghpy", SearchOption.AllDirectories);
        if (files != null)
        {
          for (int i = 0; i < files.Length; i++)
          {
            if (files[i] != null)
              yield return files[i];
          }
        }
      }
    }

    private static IEnumerable<string> GetPathsToBeSearched()
    {
      var dirs = new Dictionary<string, string>();
      foreach (var path in GH_ComponentServer.GHA_Directories)
      {
        if (string.IsNullOrEmpty(path)) continue;
        var newPath = path;
        if (path.EndsWith("\\")) newPath = path.Substring(0, path.Length - 1);
        if (!Directory.Exists(newPath)) continue;
        var pathUp = newPath.ToUpperInvariant();
        if (!dirs.ContainsKey(pathUp))
        {
          dirs.Add(pathUp, null);
          yield return newPath;
        }
      }
    }
  }
}

#endif