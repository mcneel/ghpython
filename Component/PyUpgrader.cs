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

      component_new.HiddenCodeInput = component_OBSOLETE.HiddenCodeInput;
      component_new.HiddenOutOutput = component_OBSOLETE.HiddenOutOutput;

      if (!component_new.HiddenCodeInput)
        component_new.Code = component_OBSOLETE.Code;

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

        component_OBSOLETE.Dispose();

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
      get { return new DateTime(2013, 10, 14, 0, 0, 0); }
    }
  }
}
