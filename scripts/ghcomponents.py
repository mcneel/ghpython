import clr
clr.AddReference("Grasshopper")
import Grasshopper as GH

def __makefunc__(obj):
    def component_function(*args, **kwargs):
        """"returns dictionary of output values"""
        comp = obj.CreateInstance()
        comp.ClearData()
        
        if args:
            for i, arg in enumerate(args):
                param = comp.Params.Input[i]
                param.PersistentData.Clear()
                if hasattr(arg, '__iter__'): #TODO deal with polyline
                    for a in arg:
                        param.AddPersistentData(a)
                else:
                    param.AddPersistentData(arg)
        if kwargs:
            for param in comp.Params.Input:
                name = param.NickName
                if name in kwargs:
                    param.PersistentData.Clear()
                    arg = kwargs[name]
                    if hasattr(arg, '__iter__'): #TODO deal with polyline
                        for a in arg:
                            param.AddPersistentData(a)
                    else:
                        param.AddPersistentData(arg)

        doc = GH.Kernel.GH_Document()
        doc.AddObject(comp, False, 0)
        comp.CollectData()
        comp.ComputeData()
        rc = {}
        for output in comp.Params.Output:
            data = output.VolatileData.AllData(True)
            name = output.NickName
            rc[name] = [x.Value for x in data]
        return rc
    return component_function

class namespace_object(object):
    def __init__(self):
        pass

class __builder(object):
    def __init__(self):
        import sys, types, string
        core_module = sys.modules['ghcomponents']
        translation = string.maketrans("'()*|","_____")
        for obj in GH.Instances.ComponentServer.ObjectProxies:
            if obj.Exposure==GH.Kernel.GH_Exposure.hidden or obj.Obsolete:
                continue
            t = clr.GetClrType(GH.Kernel.IGH_Component)
            if not (t.IsAssignableFrom(obj.Type)):
                continue
            m = core_module
            library_id = obj.LibraryGuid
            assembly = GH.Instances.ComponentServer.FindAssembly(library_id)
            if not assembly.IsCoreLibrary:
                module_name = assembly.Assembly.GetName().Name
                if core_module.__dict__.has_key(module_name):
                    m = core_module.__dict__[module_name]
                else:
                    m = namespace_object()
                    setattr(core_module, module_name, m)
            name = obj.Desc.Name.Replace(" ","")
            name = name.translate(translation)
            if not name[0].isalpha(): name = '_' + name
            
            if m==core_module:
                setattr(m, name, __makefunc__(obj))
                comp = obj.CreateInstance()
                a = m.__dict__[name]
                a.__name__ = name
                params = self.param_descriptions(comp.Params)
                help = "\n" + obj.Desc.Description
                for param in params:
                    help = help + "\n" + param
                a.__doc__ = help
            else:
                setattr(m, name, types.MethodType(__makefunc__(obj), m, type(m)))

    def param_descriptions(self, params):
        rc = []
        for param in params:
            if param.Kind == GH.Kernel.GH_ParamKind.input:
                s = "{0} (in) [{1}] - {2}"
                if param.Optional:
                    s = "{0} (in|optional) [{1}] - {2}"
                rc.append(s.format(param.NickName, param.TypeName, param.Description))
            elif param.Kind == GH.Kernel.GH_ParamKind.output:
                s = "{0} (out) [{1}] - {2}"
                rc.append(s.format(param.NickName, param.TypeName, param.Description))
        return rc


__b = __builder()