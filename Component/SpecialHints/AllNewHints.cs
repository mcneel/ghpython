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

        else if (target is Polyline)
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
        return "dynamic (rhinoscript Guid when geometry)";
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

  static class PythonHints
  {


    static Dictionary<Guid, IGH_TypeHint> _new_marshallings = new Dictionary<Guid, IGH_TypeHint>() {
      { NewFloatHint.ID, new NewFloatHint() },
      { NewStrHint.ID, new NewStrHint() },


      { NewDynamicHint.ID, new NewDynamicHint() },
      { NewDynamicAsGuidHint.ID, new NewDynamicAsGuidHint() },
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

      { typeof(GH_Point3dHint), NewDynamicAsGuidHint.ID },

      { typeof(GH_ArcHint), NewDynamicAsGuidHint.ID },
      { typeof(SpecialArcHint), NewDynamicAsGuidHint.ID },

      { typeof(GH_BoxHint), NewDynamicAsGuidHint.ID },
      { typeof(SpecialBoxHint), NewDynamicAsGuidHint.ID },

      { typeof(GH_CircleHint), NewDynamicAsGuidHint.ID },
      { typeof(SpecialCircleHint), NewDynamicAsGuidHint.ID },

      { typeof(GH_LineHint), NewDynamicAsGuidHint.ID },
      { typeof(SpecialLineHint), NewDynamicAsGuidHint.ID },

      { typeof(GH_PolylineHint), NewDynamicAsGuidHint.ID },
      { typeof(SpecialPolylineHint), NewDynamicAsGuidHint.ID },

      { typeof(GH_CurveHint), NewDynamicAsGuidHint.ID },
      { typeof(GH_SurfaceHint), NewDynamicAsGuidHint.ID },
      { typeof(GH_MeshHint), NewDynamicAsGuidHint.ID },
      { typeof(GH_BrepHint), NewDynamicAsGuidHint.ID },
      { typeof(GH_GeometryBaseHint), NewDynamicAsGuidHint.ID },
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