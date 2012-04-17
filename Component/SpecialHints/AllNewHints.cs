using System;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Parameters.Hints;
using Rhino.Geometry;
using System.Collections.Generic;

namespace GhPython.Component
{
  class NewDynamicAsGuidHint : GH_NullHint, IGH_TypeHint
  {
    bool IGH_TypeHint.Cast(object data, out object target)
    {
      bool toReturn = base.Cast(data, out target);

      if (toReturn && target != null)
      {
        Type t = target.GetType();

        if (t == typeof(Line))
          target = new LineCurve((Line)target);

        else if (t == typeof(Arc))
          target = new ArcCurve((Arc)target);

        else if (t == typeof(Circle))
          target = new ArcCurve((Circle)target);

        else if (t == typeof(Ellipse))
          target = ((Ellipse)target).ToNurbsCurve();

        else if (t == typeof(Box))
          target = Brep.CreateFromBox((Box)target);

        else if (t == typeof(BoundingBox))
          target = Brep.CreateFromBox((BoundingBox)target);

        else if (t == typeof(Rectangle3d))
          target = ((Rectangle3d)target).ToNurbsCurve();

        else if (t == typeof(Polyline))
          target = new PolylineCurve((Polyline)target);
      }

      return toReturn;
    }

    internal readonly static Guid ID = new Guid("{87F87F55-5B71-41F4-8AEA-21D494016F81}");

    Guid IGH_TypeHint.HintID
    {
      get
      {
        return ID;
      }
    }

    string IGH_TypeHint.TypeName
    {
      get
      {
        return "rhinoscript dynamic (Guid)";
      }
    }
  }

  class NewDynamicHint : GH_NullHint, IGH_TypeHint
  {
    internal readonly static Guid ID = new Guid("{35915213-5534-4277-81B8-1BDC9E7383D2}");

    Guid IGH_TypeHint.HintID
    {
      get
      {
        return ID;
      }
    }

    string IGH_TypeHint.TypeName
    {
      get
      {
        return "dynamic";
      }
    }
  }

  class NewSpecialPointAsGuidHint : GH_Point3dHint, IGH_TypeHint
  {
    internal readonly static Guid ID = new Guid("{F3DECF87-5DDB-4BA0-B958-08654E703CE8}");

    bool IGH_TypeHint.Cast(object data, out object target)
    {
      bool toReturn = base.Cast(data, out target);

      if (toReturn && target != null)
      {
        if (target.GetType() == typeof(Point3d))
          target = new Point((Point3d)target);
      }

      return toReturn;
    }

    Guid IGH_TypeHint.HintID
    {
      get
      {
        return ID;
      }
    }

    string IGH_TypeHint.TypeName
    {
      get
      {
        return "rhinoscript Point (Guid)";
      }
    }
  }

  class NewSpecialArcAsGuidHint : GH_ArcHint, IGH_TypeHint
  {
    internal readonly static Guid ID = new Guid("{E82FF11D-D7BD-436B-A1E0-2B1228EFA6AD}");

    bool IGH_TypeHint.Cast(object data, out object target)
    {
      bool toReturn = base.Cast(data, out target);

      if (toReturn && target != null)
      {
        Type t = target.GetType();

        if (t == typeof(Arc))
          target = new ArcCurve((Arc)target);

        else if (t == typeof(Circle))
          target = new ArcCurve((Circle)target);
      }

      return toReturn;
    }

    Guid IGH_TypeHint.HintID
    {
      get
      {
        return ID;
      }
    }

    string IGH_TypeHint.TypeName
    {
      get
      {
        return "rhinoscript Arc (Guid)";
      }
    }
  }

  class NewSpecialBoxAsGuidHint : GH_BoxHint, IGH_TypeHint
  {
    bool IGH_TypeHint.Cast(object data, out object target)
    {
      bool toReturn = base.Cast(data, out target);

      if (toReturn && target != null)
      {
        Type t = target.GetType();

        if (t == typeof(Box))
          target = Brep.CreateFromBox((Box)target);

      }

      return toReturn;
    }

    internal readonly static Guid ID = new Guid("{8AF34D6D-55F0-4E94-BF99-F56679F868F3}");

    Guid IGH_TypeHint.HintID
    {
      get
      {
        return ID;
      }
    }

    string IGH_TypeHint.TypeName
    {
      get
      {
        return "rhinoscript Box (Guid)";
      }
    }
  }

  class NewSpecialCircleHint : GH_CircleHint, IGH_TypeHint
  {
    bool IGH_TypeHint.Cast(object data, out object target)
    {
      bool toReturn = base.Cast(data, out target);

      if (toReturn && target != null)
      {
        Type t = target.GetType();

        if (t == typeof(Circle))
          target = new ArcCurve((Circle)target);
      }

      return toReturn;
    }

    internal readonly static Guid ID = new Guid("{4D1C5515-6737-4D37-8130-991014E3421B}");

    Guid IGH_TypeHint.HintID
    {
      get
      {
        return ID;
      }
    }

    string IGH_TypeHint.TypeName
    {
      get
      {
        return "rhinoscript Circle (Guid)";
      }
    }
  }

  class NewSpecialLineHint : GH_LineHint, IGH_TypeHint
  {
    bool IGH_TypeHint.Cast(object data, out object target)
    {
      bool toReturn = base.Cast(data, out target);

      if (toReturn && target != null)
      {
        Type t = target.GetType();

        if (t == typeof(Line))
          target = new LineCurve((Line)target);
      }

      return toReturn;
    }

    internal readonly static Guid ID = new Guid("{50E93FEE-8580-491D-9B65-7E408EF47464}");

    Guid IGH_TypeHint.HintID
    {
      get
      {
        return ID;
      }
    }

    string IGH_TypeHint.TypeName
    {
      get
      {
        return "rhinoscript Line (Guid)";
      }
    }
  }

  class NewSpecialPolylineHint : GH_PolylineHint, IGH_TypeHint
  {
    bool IGH_TypeHint.Cast(object data, out object target)
    {
      bool toReturn = base.Cast(data, out target);

      if (toReturn)
      {
        var pT = target as Polyline;
        if (pT != null)
          target = new PolylineCurve(pT);
      }

      return toReturn;
    }

    internal readonly static Guid ID = new Guid("{49B7102D-7818-4128-9A87-D7BB4327CE8B}");

    Guid IGH_TypeHint.HintID
    {
      get
      {
        return ID;
      }
    }

    string IGH_TypeHint.TypeName
    {
      get
      {
        return "rhinoscript Polyline (Guid)";
      }
    }
  }

  static class PythonHints
  {
    static Dictionary<Guid, IGH_TypeHint> _values = new Dictionary<Guid, IGH_TypeHint>() {
      { NewDynamicAsGuidHint.ID, new NewDynamicAsGuidHint() },
      { NewSpecialPointAsGuidHint.ID, new NewSpecialPointAsGuidHint() },
      { NewSpecialArcAsGuidHint.ID, new NewSpecialArcAsGuidHint() },
      { NewSpecialBoxAsGuidHint.ID, new NewSpecialBoxAsGuidHint() },
      { NewSpecialCircleHint.ID, new NewSpecialCircleHint() },
      { NewSpecialLineHint.ID, new NewSpecialLineHint() },
      { NewSpecialPolylineHint.ID, new NewSpecialPolylineHint() },
    };

    public static IDictionary<Guid, IGH_TypeHint> NewMarshalling { get { return _values; } }

    internal static bool AddToGhDoc(IGH_TypeHint probe)
    {
      return _values.ContainsKey(probe.HintID);
    }
  }
}