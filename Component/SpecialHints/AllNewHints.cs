using System;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Parameters.Hints;
using Rhino.Geometry;
using System.Collections.Generic;

namespace GhPython.Component
{
  class NewFloatHint : GH_DoubleHint_CS, IGH_TypeHint
  {
    internal readonly static Guid ID = new Guid("{39FBC626-7A01-46AB-A18E-EC1C0C41685B}");

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
        return "float";
      }
    }
  }

  class NewStrHint : GH_StringHint_CS, IGH_TypeHint
  {
    internal readonly static Guid ID = new Guid("{37261734-EEC7-4F50-B6A8-B8D1F3C4396B}");

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
        return "str";
      }
    }
  }

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
        return "dynamic (as rhinoscript Guid)";
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
        return "Point (as rhinoscript Guid)";
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
        return "Arc (as rhinoscript Guid)";
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
        return "Box (as rhinoscript Guid)";
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
        return "Circle (as rhinoscript Guid)";
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
        return "Line (as rhinoscript Guid)";
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
        return "Polyline (as rhinoscript Guid)";
      }
    }
  }

  class NewSpecialCurveHint : GH_CurveHint, IGH_TypeHint
  {
    internal readonly static Guid ID = new Guid("{F299C795-53EC-4043-9968-0F0430975F8E}");

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
        return "Curve (as rhinoscript Guid)";
      }
    }
  }

  class NewSpecialSurfaceHint : GH_SurfaceHint, IGH_TypeHint
  {
    internal readonly static Guid ID = new Guid("{8DAA73DA-D575-4D5F-9009-1454FF977E29}");

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
        return "Surface (as rhinoscript Guid)";
      }
    }
  }

  class NewSpecialMeshHint : GH_MeshHint, IGH_TypeHint
  {
    internal readonly static Guid ID = new Guid("{AC7B57DD-5A7C-44BB-8DF4-858D13A058C2}");

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
        return "Mesh (as rhinoscript Guid)";
      }
    }
  }

  class NewSpecialBrepHint : GH_BrepHint, IGH_TypeHint
  {
    internal readonly static Guid ID = new Guid("{8A768290-D9BF-4156-8419-9232DE9D6895}");

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
        return "Brep (as rhinoscript Guid)";
      }
    }
  }

  class NewSpecialGeometryBaseHint : GH_GeometryBaseHint, IGH_TypeHint
  {
    internal readonly static Guid ID = new Guid("{E643B799-27A9-4251-89EC-5332B36366E4}");

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
        return "GeometryBase (as rhinoscript Guid)";
      }
    }
  }


  static class PythonHints
  {


    static Dictionary<Guid, IGH_TypeHint> _new_marshallings = new Dictionary<Guid, IGH_TypeHint>() {
      { NewFloatHint.ID, new NewFloatHint() },
      { NewStrHint.ID, new NewStrHint() },

      { NewDynamicAsGuidHint.ID, new NewDynamicAsGuidHint() },
      { NewSpecialPointAsGuidHint.ID, new NewSpecialPointAsGuidHint() },
      { NewSpecialArcAsGuidHint.ID, new NewSpecialArcAsGuidHint() },
      { NewSpecialBoxAsGuidHint.ID, new NewSpecialBoxAsGuidHint() },
      { NewSpecialCircleHint.ID, new NewSpecialCircleHint() },
      { NewSpecialLineHint.ID, new NewSpecialLineHint() },
      { NewSpecialPolylineHint.ID, new NewSpecialPolylineHint() },

      { NewSpecialCurveHint.ID, new NewSpecialCurveHint() },
      { NewSpecialSurfaceHint.ID, new NewSpecialSurfaceHint() },
      { NewSpecialMeshHint.ID, new NewSpecialMeshHint() },
      { NewSpecialBrepHint.ID, new NewSpecialBrepHint() },
      { NewSpecialGeometryBaseHint.ID, new NewSpecialGeometryBaseHint() },
    };

    static Dictionary<Type, IGH_TypeHint> _gh_marshallings = new Dictionary<Type, IGH_TypeHint>() {
      { typeof(GH_BoxHint), new GH_BoxHint() },
      { typeof(GH_LineHint), new GH_LineHint() },
      { typeof(GH_CircleHint), new GH_CircleHint() },
      { typeof(GH_PolylineHint), new GH_PolylineHint() },
      { typeof(GH_ArcHint), new GH_ArcHint() },
      { typeof(GH_CurveHint), new GH_CurveHint() },
      { typeof(GH_SurfaceHint), new GH_SurfaceHint() },
      { typeof(GH_BrepHint), new GH_BrepHint() },
      { typeof(GH_MeshHint), new GH_MeshHint() },
      { typeof(GH_GeometryBaseHint), new GH_GeometryBaseHint() },
    };

    static Dictionary<Type, Guid> _old_to_rs = new Dictionary<Type, Guid>() {
      { typeof(GH_DoubleHint_CS), NewFloatHint.ID },

      { typeof(GH_NullHint), NewDynamicAsGuidHint.ID },
      { typeof(DynamicHint), NewDynamicAsGuidHint.ID },

      { typeof(GH_Point3dHint), NewSpecialPointAsGuidHint.ID },

      { typeof(GH_ArcHint), NewSpecialArcAsGuidHint.ID },
      { typeof(SpecialArcHint), NewSpecialArcAsGuidHint.ID },

      { typeof(GH_BoxHint), NewSpecialBoxAsGuidHint.ID },
      { typeof(SpecialBoxHint), NewSpecialBoxAsGuidHint.ID },

      { typeof(GH_CircleHint), NewSpecialCircleHint.ID },
      { typeof(SpecialCircleHint), NewSpecialCircleHint.ID },

      { typeof(GH_LineHint), NewSpecialLineHint.ID },
      { typeof(SpecialLineHint), NewSpecialLineHint.ID },

      { typeof(GH_PolylineHint), NewSpecialPolylineHint.ID },
      { typeof(SpecialPolylineHint), NewSpecialPolylineHint.ID },

      { typeof(GH_CurveHint), NewSpecialCurveHint.ID },
      { typeof(GH_SurfaceHint), NewSpecialSurfaceHint.ID },
      { typeof(GH_MeshHint), NewSpecialMeshHint.ID },
      { typeof(GH_BrepHint), NewSpecialBrepHint.ID },
      { typeof(GH_GeometryBaseHint), NewSpecialGeometryBaseHint.ID },
    };

    static Dictionary<Type, Type> _old_to_common = new Dictionary<Type, Type>() {
      { typeof(GH_DoubleHint_CS), typeof(NewFloatHint) },

      { typeof(GH_NullHint), typeof(NewDynamicHint) },
      { typeof(DynamicHint), typeof(NewDynamicHint) },

      { typeof(SpecialArcHint), typeof(GH_ArcHint) },
      { typeof(SpecialBoxHint), typeof(GH_BoxHint) },
      { typeof(SpecialCircleHint), typeof(GH_CircleHint) },
      { typeof(SpecialLineHint), typeof(GH_LineHint) },
      { typeof(SpecialPolylineHint), typeof(GH_PolylineHint) },
    };

    public static IDictionary<Guid, IGH_TypeHint> NewMarshalling { get { return _new_marshallings; } }

    public static IDictionary<Type, IGH_TypeHint> GhMarshalling { get { return _gh_marshallings; } }

    internal static bool ShouldAddToGhDoc(IGH_TypeHint probe)
    {
      return _new_marshallings.ContainsKey(probe.HintID);
    }

    internal static bool ToNewRhinoscriptHint(IGH_TypeHint probe, out IGH_TypeHint newHint)
    {
      newHint = null;
      if (probe == null) return false;

      var probeType = probe.GetType();

      if (_old_to_rs.ContainsKey(probeType))
      {
        newHint = _new_marshallings[_old_to_rs[probeType]];
        return true;
      }
      else
      {
        return false;
      }
    }

    internal static bool ToNewRhinoCommonHint(IGH_TypeHint probe, out IGH_TypeHint newHint)
    {
      newHint = null;
      if (probe == null) return false;

      var probeType = probe.GetType();

      if (_old_to_common.ContainsKey(probeType))
      {
        var type = _old_to_common[probeType];
        if (_gh_marshallings.ContainsKey(type))
        {
          newHint = _gh_marshallings[type];
          return true;
        }
      }
      return false;
    }
  }
}