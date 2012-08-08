using System;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Parameters.Hints;
using Rhino.Geometry;

namespace GhPython.Component
{
  class SpecialPolylineHint : GH_PolylineHint, IGH_TypeHint
  {
    private readonly PythonComponent_OBSOLETE _component;

    public SpecialPolylineHint(PythonComponent_OBSOLETE component)
    {
      if (component == null)
        throw new ArgumentNullException("component");

      _component = component;
    }

    bool IGH_TypeHint.Cast(object data, out object target)
    {
      bool toReturn = base.Cast(data, out target);

      if (toReturn &&
          _component.DocStorageMode == DocReplacement.DocStorage.AutomaticMarshal &&
          target != null)
      {
        Type t = target.GetType();

        if (t == typeof (Polyline))
          target = new PolylineCurve((Polyline) target);
      }

      return toReturn;
    }
  }
}