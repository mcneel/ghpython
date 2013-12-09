using System;
using Grasshopper.Kernel;
using Rhino.Runtime;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace GhPython.Component
{
  public class PythonEnvironment
  {
    internal PythonEnvironment(GH_Component component, PythonScript script)
    {
      Component = component;
      Script = script;

      if (script != null)
      {
        Type scriptType = script.GetType();

        var scopeField = scriptType.GetField("m_scope");
        if (scopeField != null)
        {
          LocalScope = scopeField.GetValue(script);
        }

        var intellisenseField = scriptType.GetField("m_intellisense",
          BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
        if (intellisenseField != null)
        {
          Intellisense = intellisenseField.GetValue(script);
          if (Intellisense != null)
          {
            var intellisenseType = Intellisense.GetType();
            var scopeProperty = intellisenseType.GetProperty("Scope");
            IntellisenseScope = scopeProperty.GetValue(Intellisense, null);
          }
        }

        var baseType = scriptType.BaseType;
        if (baseType != null && baseType != typeof(object))
        {
          var hostType = baseType.Assembly.GetType("RhinoPython.Host");
          if (hostType != null)
          {
            var engineInfo = hostType.GetProperty("Engine");
            if (engineInfo != null)
            {
              Engine = engineInfo.GetValue(null, null);

              if (Engine != null)
              {
                var runtimeInfo = Engine.GetType().GetProperty("Runtime");
                Runtime = runtimeInfo.GetValue(Engine, null);
              }
            }

            var scopeInfo = hostType.GetProperty("Scope", BindingFlags.NonPublic |
                                                          BindingFlags.GetProperty |
                                                          BindingFlags.Static);
            if (scopeInfo != null)
              ScriptScope = scopeInfo.GetValue(null, null);
          }
        }
      }
    }

    public GH_Component Component { get; internal set; }

    public PythonScript Script { get; internal set; }

    public object LocalScope { get; internal set; }

    public object ScriptScope { get; internal set; }

    public object Intellisense { get; internal set; }

    public object IntellisenseScope { get; internal set; }

    public object Engine { get; internal set; }

    public object Runtime { get; internal set; }

    public Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

    public IGH_DataAccess DataAccessManager { get; internal set; }

    public void LoadAssembly(Assembly assembly)
    {
      FunctionalityLoad(assembly);

      IList list = GetIntellisenseList();
      if (list == null) return;

      foreach (var namesp in GetToplevelNamespacesForAssembly(assembly))
      {
        if (!list.Contains(namesp))
          list.Add(namesp);
      }
    }

    public void AddGhPythonPackage()
    {
      IList list = GetIntellisenseList();
      if (list == null) return;

      // add gh_python package
      if (!list.Contains("gh_python"))
        list.Add("gh_python");
    }

    private IList GetIntellisenseList()
    {
      IList list = null;

      // now intellisense
      if (IntellisenseScope == null) return list;

      // we really want to get intellisense right away. No matter what
      // so, we first make it cache, then add to it

      var intellisense_type = Intellisense.GetType();
      var m = intellisense_type.GetMethod("GetModuleList", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod);
      m.Invoke(Intellisense, null);

      var ex_m_autocomplete_modules = intellisense_type.GetField("m_autocomplete_modules",
        BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);

      if (ex_m_autocomplete_modules == null) return list;
      return ex_m_autocomplete_modules.GetValue(Intellisense) as IList;
    }

    private void FunctionalityLoad(Assembly assembly)
    {
      var runtime = Runtime as dynamic;
      runtime.LoadAssembly(assembly);
    }

    private static IEnumerable<string> GetToplevelNamespacesForAssembly(Assembly assembly)
    {
      return assembly.GetTypes().Select(GetTopLevelNamespace)
        .Where(s => !string.IsNullOrEmpty(s)).Distinct();
    }

    // question by David here:
    // http://stackoverflow.com/questions/1549198/finding-all-namespaces-in-an-assembly-using-reflection-dotnet
    static string GetTopLevelNamespace(Type t)
    {
      string ns = t.Namespace ?? "";
      int firstDot = ns.IndexOf('.');
      return firstDot == -1 ? ns : ns.Substring(0, firstDot);
    }
  }
}
