using System;
using GhPython.Forms;
using Grasshopper.Kernel;
using Rhino.Runtime;
using System.IO;
using System.Collections.Generic;
using GhPython.Component;
using System.Reflection;
using System.Collections;

namespace GhPython.Assembies
{
  public class PyghaLoader : GH_AssemblyPriority
  {
    public override GH_LoadingInstruction PriorityLoad()
    {
      try
      {
        LoadExternalPythosAssemblies();
      }
      catch(Exception ex)
      {
        //GH_RuntimeMessage.(ex);
      }
      return GH_LoadingInstruction.Proceed;
    }

    private static void LoadExternalPythosAssemblies()
    {
      var externalPy = PythonScript.Create();

      PythonEnvironment p = new PythonEnvironment(null, externalPy);
      var engine = p.Engine as dynamic;
      
      var localscope = p.LocalScope;
      var scriptscope = p.ScriptScope;
      var runtime = engine.Runtime;
      var ops = engine.Operations;
      
      var allGhas = GetAllPygha();

      foreach (var path in allGhas)
      {
          var assembly = Assembly.LoadFile(path);
          var cachedCode = assembly.GetType("DLRCachedCode", true, false);
          dynamic info = cachedCode.InvokeMember("GetScriptCodeInfo",
            BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static,
            null, null, new object[0]);
          string[] modules = info.GetValue(2)[0];
          
          runtime.LoadAssembly(assembly);

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

                  var bases = new List<object>((IEnumerable<object>)type.InvokeMember("get_BaseTypes", BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance, null, o, null));
                  if (bases.Count != 1) continue;

                  object baseObj = bases[0];
                  Type finalSystemType = (Type)baseObj.GetType().InvokeMember("get_FinalSystemType", BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance, null, baseObj, null);
                  if (finalSystemType == null) continue;

                  if (typeof(IGH_Component).IsAssignableFrom(finalSystemType))
                  {
                      var instance = Instantiate(ops as object, o);
                      var proxy = new PythonInstantiatorProxy(instance, o, ops as object, path);

                      Grasshopper.GH_InstanceServer.ComponentServer.AddProxy(proxy);
                  }
              }

              p.Script.ExecuteScript("del " + module);
          }
          break;
      }
    }

    private static IGH_Component Instantiate(object engineOperations, object pythonType)
    {
        return (engineOperations as dynamic).Invoke(pythonType);
    }

    private static IList<string> GetAllPygha()
    {
      List<string> addons = new List<string>();
      foreach (var path in GetPathsToBeSearched())
      {
        string[] files = Directory.GetFiles(path, "*.pygha", SearchOption.AllDirectories);
        if (files != null)
          addons.AddRange(files);
      }
      return addons;
    }

    private static IEnumerable<string> GetPathsToBeSearched()
    {
      var dirs = new Dictionary<string, string>();
      foreach (var path in GH_ComponentServer.GHA_Directories)
      {
        if (string.IsNullOrEmpty(path)) continue;
        if (path.EndsWith("\\")) path.Substring(0, path.Length - 1);
        if (!Directory.Exists(path)) continue;
        if (!dirs.ContainsKey(path)) dirs.Add(path, null);
      }

      return dirs.Keys;
    }
  }
}