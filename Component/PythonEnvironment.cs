using System;
using Grasshopper.Kernel;
using Rhino.Runtime;

namespace GhPython.Component
{
  public class PythonEnvironment
  {
    internal PythonEnvironment(Grasshopper.Kernel.GH_Component component, PythonScript script)
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

        var baseType = scriptType.BaseType;
        if (baseType != null && baseType != typeof (object))
        {
          var hostType = baseType.Assembly.GetType("RhinoPython.Host");
          if (hostType != null)
          {
            var engineInfo = hostType.GetProperty("Engine");
            if (engineInfo != null)
              Engine = engineInfo.GetValue(null, null);

            var scopeInfo = hostType.GetProperty("Scope", System.Reflection.BindingFlags.NonPublic |
                                                          System.Reflection.BindingFlags.GetProperty |
                                                          System.Reflection.BindingFlags.Static);
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

    public object Engine { get; internal set; }
  }
}
