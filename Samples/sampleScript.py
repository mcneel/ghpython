import math
import rhinoscriptsyntax as rs

result = []
for i in range(x):
    cId = rs.AddCircle([i, math.cos(i), 0], 0.3)
    result.append(cId)

a = ghdoc[result]