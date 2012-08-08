using System;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Parameters.Hints;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GhPython.Component
{
  [Guid("39FBC626-7A01-46AB-A18E-EC1C0C41685B")]
  class NewFloatHint : GH_DoubleHint_CS, IGH_TypeHint
  {
    Guid IGH_TypeHint.HintID { get { return this.GetType().GUID; } }

    string IGH_TypeHint.TypeName { get { return "float"; } }
  }

  [Guid("37261734-EEC7-4F50-B6A8-B8D1F3C4396B")]
  class NewStrHint : GH_StringHint_CS, IGH_TypeHint
  {
    Guid IGH_TypeHint.HintID { get { return this.GetType().GUID; } }

    string IGH_TypeHint.TypeName { get { return "str"; } }
  }

  [Guid("87F87F55-5B71-41F4-8AEA-21D494016F81")]
  class GhDocGuidHint : GH_NullHint, IGH_TypeHint
  {
    Guid IGH_TypeHint.HintID { get { return this.GetType().GUID; } }
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

    string IGH_TypeHint.TypeName { get { return "ghdoc Object when geometry (rhinoscriptsyntax)"; } }
  }

  [Guid("35915213-5534-4277-81B8-1BDC9E7383D2")]
  class NoChangeHint : GH_NullHint, IGH_TypeHint
  {
    Guid IGH_TypeHint.HintID { get { return this.GetType().GUID; } }

    string IGH_TypeHint.TypeName { get { return "No Type Hint"; } }
  }


  static class PythonHints
  {
    static readonly Dictionary<Guid, IGH_TypeHint> _new_marshallings = new Dictionary<Guid, IGH_TypeHint> {
      { typeof(NewFloatHint).GUID, new NewFloatHint() },
      { typeof(NewStrHint).GUID, new NewStrHint() },
      { typeof(NoChangeHint).GUID, new NoChangeHint() },
      { typeof(GhDocGuidHint).GUID, new GhDocGuidHint() },
    };


    static readonly Dictionary<Type, Guid> _old_to_rs = new Dictionary<Type, Guid> {
      { typeof(GH_DoubleHint_CS), typeof(NewFloatHint).GUID },

      { typeof(GH_NullHint), typeof(GhDocGuidHint).GUID },
      { typeof(DynamicHint), typeof(GhDocGuidHint).GUID },

      { typeof(GH_Point3dHint), typeof(GhDocGuidHint).GUID },

      { typeof(GH_ArcHint), typeof(GhDocGuidHint).GUID },
      { typeof(SpecialArcHint), typeof(GhDocGuidHint).GUID },

      { typeof(GH_BoxHint), typeof(GhDocGuidHint).GUID },
      { typeof(SpecialBoxHint), typeof(GhDocGuidHint).GUID },

      { typeof(GH_CircleHint), typeof(GhDocGuidHint).GUID },
      { typeof(SpecialCircleHint), typeof(GhDocGuidHint).GUID },

      { typeof(GH_LineHint), typeof(GhDocGuidHint).GUID },
      { typeof(SpecialLineHint), typeof(GhDocGuidHint).GUID },

      { typeof(GH_PolylineHint), typeof(GhDocGuidHint).GUID },
      { typeof(SpecialPolylineHint), typeof(GhDocGuidHint).GUID },

      { typeof(GH_CurveHint), typeof(GhDocGuidHint).GUID },
      { typeof(GH_SurfaceHint), typeof(GhDocGuidHint).GUID },
      { typeof(GH_MeshHint), typeof(GhDocGuidHint).GUID },
      { typeof(GH_BrepHint), typeof(GhDocGuidHint).GUID },
      { typeof(GH_GeometryBaseHint), typeof(GhDocGuidHint).GUID },
    };

    static readonly Dictionary<Type, Type> _old_to_common = new Dictionary<Type, Type> {
      { typeof(GH_DoubleHint_CS), typeof(NewFloatHint) },

      { typeof(GH_NullHint), typeof(NoChangeHint) },
      { typeof(DynamicHint), typeof(NoChangeHint) },

      { typeof(SpecialArcHint), typeof(GH_ArcHint) },
      { typeof(SpecialBoxHint), typeof(GH_BoxHint) },
      { typeof(SpecialCircleHint), typeof(GH_CircleHint) },
      { typeof(SpecialLineHint), typeof(GH_LineHint) },
      { typeof(SpecialPolylineHint), typeof(GH_PolylineHint) },
    };


    public static bool ToNewRhinoscriptHint(IGH_TypeHint probe, out IGH_TypeHint newHint)
    {
      newHint = null;
      if (probe == null) return false;

      var probeType = probe.GetType();

      if (_old_to_rs.ContainsKey(probeType))
      {
        newHint = _new_marshallings[_old_to_rs[probeType]];
        return true;
      }
      return false;
    }

    public static void ToNewRhinoCommonHint(Param_ScriptVariable sc)
    {
      if (sc == null || sc.TypeHint == null) return;

      Type probeType = sc.TypeHint.GetType();

      if (_old_to_common.ContainsKey(probeType))
      {
        var type = _old_to_common[probeType];
        if (type != null)
          sc.TypeHint = System.Activator.CreateInstance(type) as IGH_TypeHint;
      }
    }
  }
}