using System;
using System.Collections.Generic;
using System.Text;

namespace GhPython.DocReplacement
{
    enum DocStorage : int
    {
        None = -10, // Never used
        InGrasshopperMemory = 0,
        InRhinoDoc = 10,
    }
}
