"""Constructs a recursive Koch curve.
    Inputs:
        x: The original line. (Line)
        y: The the number of subdivisions. (int)
    Outputs:
        a: The Koch curve, as a list of lines.
"""
import Rhino.Geometry as rg
import math


def Main():
    global a # as usual, you can assign to the global scope with the "global" keyword
    a = []
    SubdivideAndRotate(x, 0)

def WeightedAvaragePts(pt1, pt2, pt1_part):
    rest = 1 - pt1_part
    return pt1 * rest + pt2 * pt1_part

def SubdivideAndRotate(line, level):
    if level == y:
        a.append(line)
        return
    
    f = line.From #you can use "rg.Line." for exploring methods
    t = line.To
    b = WeightedAvaragePts(f, t, 1.0 / 3.0)
    c = WeightedAvaragePts(f, t, 2.0 / 3.0)
    
    m = WeightedAvaragePts(f, t, 0.5)
    bm = m - b
    
    e1 = bm / math.cos(angle)
    e1.Rotate(angle, rg.Vector3d(0,0,1))
    e = e1 + b
    
    level += 1
    
    SubdivideAndRotate(rg.Line(f, b), level)
    SubdivideAndRotate(rg.Line(b, e), level)
    SubdivideAndRotate(rg.Line(e, c), level)
    SubdivideAndRotate(rg.Line(c, t), level)

if x is None: x = rg.Line(rg.Point3d(0,0,0), rg.Point3d(10,10,0))
if y is None: y = 4
angle = math.pi / 3

Main()