using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
      {
        return null;
      }
      ZuiPythonComponent component_new = new ZuiPythonComponent();

      component_new.HideCodeInput = component_OBSOLETE.HideCodeInput;
      if (component_new.HideCodeInput)
        component_new.Params.Input.RemoveAt(0);

      component_new.HideCodeOutput = component_OBSOLETE.HideCodeOutput;

      if (component_new.HideCodeInput)
        component_new.Params.Output.RemoveAt(0);

      if (component_new.HideCodeInput)
        component_new.CodeInput = component_OBSOLETE.CodeInput;

      if (GH_UpgradeUtil.SwapComponents(component_OBSOLETE, component_new))
      {
        bool toRhinoScript = (component_OBSOLETE.DocStorageMode == DocReplacement.DocStorage.AutomaticMarshal);
        {
          foreach (var c in component_new.Params.Input)
          {
            var sc = c as Param_ScriptVariable;
            if (sc != null)
            {
              IGH_TypeHint newHint;

              if (toRhinoScript)
              {
                if (PythonHints.ToNewRhinoscriptHint(sc.TypeHint, out newHint))
                  sc.TypeHint = newHint;
              }
              else
              {
                if (PythonHints.ToNewRhinoCommonHint(sc.TypeHint, out newHint))
                  sc.TypeHint = newHint;
              }
            }
          }
        }

        return component_new;
      }
      return null;
    }

    public Guid UpgradeFrom
    {
      get { return new Guid(PythonComponent_OBSOLETE.Id); }
    }

    public Guid UpgradeTo
    {
      get { return new Guid(ZuiPythonComponent.Id); }
    }

    public DateTime Version
    {
      get { return new DateTime(2012, 3, 19, 21, 0, 0); }
    }
  }
}
