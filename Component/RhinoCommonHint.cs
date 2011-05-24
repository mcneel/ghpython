using System;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Parameters.Hints;
using Rhino.Geometry;

namespace GhPython.Component
{
    class RhinoCommonHint : GH_NullHint, IGH_TypeHint
    {

        bool IGH_TypeHint.Cast(object data, out object target)
        {
            bool toReturn = base.Cast(data, out target);

            return toReturn;
        }

        Guid IGH_TypeHint.HintID
        {
            get
            {
                return new Guid("{F2401793-1551-2A01-09C1-47DC6021AA47}");
            }
        }

        string IGH_TypeHint.TypeName
        {
            get
            {
                return "dynamic (RhinoCommon)";
            }
        }
    }
}

