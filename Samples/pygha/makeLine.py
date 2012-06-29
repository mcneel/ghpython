import clr

###############################################################
# 1. This is a sample of the code to create a Line component in Python.
# David can make this slightly simpler by never using protected nested types.

clr.AddReference("Grasshopper")
import Grasshopper

import System, Rhino
clr.AddReference("RhinoCommon")
import Rhino.Geometry as rh

class MyLineComponent(Grasshopper.Kernel.GH_Component):
    
    def __new__(cls):
        
        # Information is:                                        Name,            Nickname,      Description,        Category, Subcategory
        instance = Grasshopper.Kernel.GH_Component.__new__(cls, "Line from ends", "Line", "Makes a line from two points.", "PYTHON", "Py")
        return instance
    
    def get_ComponentGuid(self):
        return System.Guid("8549a015-74c5-479f-8633-c01a9d06a7fa")
        
    def SetUpParam(self, p, name, nickname, description):
        p.Name = name
        p.NickName = nickname
        p.Description = description
        
    
    def RegisterInputParams(self, pManager):
        p0 = Grasshopper.Kernel.Parameters.Param_Point()
        self.SetUpParam(p0, "Point A", "A", "The first point")
        p0.Access = Grasshopper.Kernel.GH_ParamAccess.item
        self.Params.Input.Add(p0)
        
        p1 = Grasshopper.Kernel.Parameters.Param_Point()
        self.SetUpParam(p1, "Point B", "B", "The second point")
        p1.Access = Grasshopper.Kernel.GH_ParamAccess.item
        self.Params.Input.Add(p1)
    
    
    def RegisterOutputParams(self, pManager):
        l0 = Grasshopper.Kernel.Parameters.Param_Line()
        self.SetUpParam(l0, "Line", "L", "The resulting line")
        self.Params.Output.Add(l0)
    
    def SolveInstance(self, DA):
        pointA = Rhino.Geometry.Point3d(0,0,0)
        pointB = Rhino.Geometry.Point3d(0,0,0)
        
        v0 = DA.GetData(0, pointA)
        v1 = DA.GetData(1, pointB)
        if v0[0] and v1[1]:
            line = rh.Line(v0[1], v1[1])
            DA.SetData(0, line)
    
    def get_Internal_Icon_24x24(self):
        import base64
        o = """iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACx
jwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAadEVYdFNvZnR3YXJlAFBhaW50Lk5FVCB2My41
LjEwMPRyoQAAAalJREFUSEvVlD1Lw1AYhZsPWhpiCzFYEGsoCKVGsB2ySUZJwWy1hLahoVTUXVBx
sk466eLg6CD+gY7+AhcdnHQQdLSToGDx47ylgbSb3rt44XATwvuccz/yxmL/cMwgs8M7twpgAF1B
n+l0+rDdbk+wmsgArECX0Dv0HapcLp/CQGQxoOJuFBo+J5PJHuANFvig1nEcM5VKPY2bGIZxDYMi
swEBTNNcxfQVNbFt+wIGSS4GgGiqqt5omvZAJrIsv7VarQ0ecGV4uHNIazWbTT+TydxCd3hf4mFQ
FAShl8/n90KY7/uG67onMJhkNfAAUKrVagOwTUgKgTzg8/Qj4fYcERTABGvaaP3g6lmW1QmCYJ8V
ro0lI3hfUZQdAkPUb/48qAU8SpJkDwk6zblc7rxWq3UAp+9MY5nuNG4J9Zdd6DUej7sAT0ELTORh
8RkZhEokEs+VSmULcIEHnJb/EjUQRfGjUCis84ATY7A946LtipwJk9fI9gDc13X9vlQqdT3PW2Pt
8YPtiULr9foxoNuQA00zRUfxbDabPeANHQmFlIs8krKu9Ff1P/WNhQUWh+7RAAAAAElFTkSuQmCC"""
        return System.Drawing.Bitmap(System.IO.MemoryStream(System.Convert.FromBase64String(o)))