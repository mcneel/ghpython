
# Welcome to the GhPython editor
# This is a sample script to show how to 
# use this component and the rhinoscriptsyntax


import math
import rhinoscriptsyntax as rs

if x is None: x = 24    # if nothing is connected to x, set x to something

circles = []            # create a list. We will add IDs to it later on
radii = []              # ...and another one

for i in range(x):
    pt = (i, math.cos(i), 0)             # a tuple (here for a point)
    
    id1 = rs.AddCircle(pt, 0.3)
    circles.append(id1)
    endPt = rs.PointAdd(pt, (0, 0.3, 0)) # move the point by the vector
    id2 = rs.AddLine(pt, endPt)
    radii.append(id2)

# check if we are in Grasshopper with the ghdoc variable
if "ghdoc" in globals():
    a = ghdoc
    
    # otherwise, to separate results
    # a = ghdoc.SubSet(circles)
    # b = ghdoc.SubSet(radii)