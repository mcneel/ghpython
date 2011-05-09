using System;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Parameters.Hints;

namespace GhPython.Component
{
    class DynamicHint : GH_NullHint, IGH_TypeHint
    {

        bool IGH_TypeHint.Cast(object data, out object target)
        {
            return base.Cast(data, out target);
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

