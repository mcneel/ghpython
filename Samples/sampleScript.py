# sample script to show how to use this component and the rhinoscriptsyntax
"""Constructs a sinusoidal series of circles.
  Inputs:
    x: The number of circles. (integer)
    y: The radius of each circle. (float)
  Outputs:
    a: The list of circles. (list of circle)
    b: The list of radii. (list of float)
"""
import math
import rhinoscriptsyntax as rs

if x is None:
    x = 24    # if nothing is connected to x, set x to something (24).
if y is None:
    y = 0.3    # if nothing is connected to y, set y to 0.3.

circles = []            # create a list. We will add IDs to it later on.
radii = []              # ...and create another one.

for i in range(int(x)):
    pt = (i, math.cos(i), 0)             # a tuple (here for a point).
    id1 = rs.AddCircle(pt, y)
    circles.append(id1)
    endPt = rs.PointAdd(pt, (0, 0.3, 0)) # move the point by the vector.
    id2 = rs.AddLine(pt, endPt)
    radii.append(id2)

a = circles
b = radii