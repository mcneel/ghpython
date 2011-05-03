import math
import rhinoscriptsyntax as rs

if x is None:
    x = 100

circles = []
radii = []
for i in range(x):
    pt = (i, math.cos(i), 0)
    
    cId = rs.AddCircle(pt, 0.3)
    circles.append(cId)
    
    rId = rs.AddLine(pt, rs.PointAdd(pt, (0, 0.3, 0)))
    radii.append(rId)

# Check if we are in Grasshopper
# with Grasshopper document override
if "ghdoc" in globals():
    #a = ghdoc
    
    # If we need to separate different results,
    # this will do:
    a = ghdoc[circles]
    #r = ghdoc[radii]