using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;

namespace GhPython.Component
{
  public class PyUpgrader : IGH_UpgradeObject
  {
    public IGH_DocumentObject Upgrade(IGH_DocumentObject target, GH_Document document)
    {
      PythonComponent_OBSOLETE component_OBSOLETE = target as PythonComponent_OBSOLETE;
      if (component_OBSOLETE == null)
        return null;

      ZuiPythonComponent component_new = new ZuiPythonComponent();

      bool show_code_input = false;
      if (component_OBSOLETE.CodeInputVisible)
      {
        // see if the "code" input on the old component really has anything
        // hooked up to it.  If not, don't show the input
        show_code_input = component_OBSOLETE.Params.Input[0].SourceCount > 0;
      }
      component_new.CodeInputVisible = show_code_input;

      component_new.HideCodeOutput = component_OBSOLETE.HideCodeOutput;

      if (component_new.HideCodeOutput)
        component_new.Params.Output.RemoveAt(0);

      if (!component_new.CodeInputVisible)
        component_new.CodeInput = component_OBSOLETE.CodeInput;

      component_OBSOLETE.Dispose();

      if (GH_UpgradeUtil.SwapComponents(component_OBSOLETE, component_new))
      {
        bool toRhinoScript = (component_OBSOLETE.DocStorageMode == DocReplacement.DocStorage.AutomaticMarshal);
        {
          foreach (var c in component_new.Params.Input)
          {
            var sc = c as Param_ScriptVariable;
            if (sc == null) continue;

            if (toRhinoScript)
            {
              IGH_TypeHint newHint;
              if (PythonHints.ToNewRhinoscriptHint(sc.TypeHint, out newHint))
                sc.TypeHint = newHint;
            }
            else
            {
              PythonHints.ToNewRhinoCommonHint(sc);
            }
          }
        }

        component_new.CodeInputVisible = show_code_input;
        return component_new;
      }
      return null;
    }

    public Guid UpgradeFrom
    {
      get { return typeof(PythonComponent_OBSOLETE).GUID; }
    }

    public Guid UpgradeTo
    {
      get { return typeof(ZuiPythonComponent).GUID; }
    }

    public DateTime Version
    {
      get { return new DateTime(2012, 3, 19, 21, 0, 0); }
    }
  }
}
