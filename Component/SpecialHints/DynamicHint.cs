using System;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Parameters.Hints;
using Rhino.Geometry;

namespace GhPython.Component
{
    class DynamicHint : GH_NullHint, IGH_TypeHint
    {
        PythonComponent _component;

        public DynamicHint(PythonComponent component)
        {
            if (component == null)
                throw new ArgumentNullException("component");

            _component = component;
        }

        bool IGH_TypeHint.Cast(object data, out object target)
        {
            bool toReturn = base.Cast(data, out target);

            if (_component.DocStorageMode == DocReplacement.DocStorage.AutomaticMarshal && target != null)
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

        Guid IGH_TypeHint.HintID
        {
            get
            {
                return new Guid("{C1C11093-4F61-4E99-90C7-113C6421CC73}");
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
}

