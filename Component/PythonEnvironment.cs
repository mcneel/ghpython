using System;
using Rhino.Runtime;

namespace GhPython.Component
{
    public class PythonEnvironment
    {
        PythonComponent _component;
        PythonScript _script;
        object _localscope;
        object _engine;
        object _scriptscope;

        internal PythonEnvironment(PythonComponent component, PythonScript script)
        {
            _component = component;
            _script = script;

            if (script != null)
            {
                Type scriptType = script.GetType();

                var scopeField = scriptType.GetField("m_scope");
                if (scopeField != null)
                {
                    _localscope = scopeField.GetValue(script);
                }

                var baseType = scriptType.BaseType;
                if(baseType != null && baseType != typeof(object))
                {
                    var hostType = baseType.Assembly.GetType("RhinoPython.Host");
                    if (hostType != null)
                    {
                        var engineInfo = hostType.GetProperty("Engine");
                        if (engineInfo != null)
                            _engine = engineInfo.GetValue(null, null);

                        var scopeInfo = hostType.GetProperty("Scope", System.Reflection.BindingFlags.NonPublic |
                            System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Static);
                        if (scopeInfo != null)
                            _scriptscope = scopeInfo.GetValue(null, null);
                    }
                }
            }

        }

        public PythonComponent Component
        {
            get { return _component; }
            internal set { _component = value; }
        }

        public PythonScript Script
        {
            get { return _script; }
            internal set { _script = value; }
        }

        public object LocalScope
        {
            get { return _localscope; }
            internal set { _localscope = value; }
        }

        public object ScriptScope
        {
            get { return _scriptscope; }
            internal set { _scriptscope = value; }
        }
    }
}
