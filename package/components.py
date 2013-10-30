import clr
clr.AddReference("Grasshopper")
import Grasshopper as GH


def __make_function__(helper):
    def component_function(*args, **kwargs):
        obj = helper.proxy
        comp = obj.CreateInstance()
        comp.ClearData()
        if args:
            for i, arg in enumerate(args):
                if arg is None: continue
                param = comp.Params.Input[i]
                param.PersistentData.Clear()
                if hasattr(arg, '__iter__'): #TODO deal with polyline
                    [param.AddPersistentData(a) for a in arg]
                else:
                    param.AddPersistentData(arg)
        if kwargs:
            for param in comp.Params.Input:
                name = param.Name.lower()
                if name in kwargs:
                    param.PersistentData.Clear()
                    arg = kwargs[name]
                    if hasattr(arg, '__iter__'): #TODO deal with polyline
                        [param.AddPersistentData(a) for a in arg]
                    else:
                        param.AddPersistentData(arg)
        doc = GH.Kernel.GH_Document()
        doc.AddObject(comp, False, 0)
        comp.CollectData()
        comp.ComputeData()
        return helper.create_output(comp.Params)
    return component_function


class namespace_object(object):
    def __init__(self):
        pass


class function_helper(object):
    def __init__(self, proxy):
        self.proxy = proxy
        self.return_type = None

    def create_output(self, params):
        import collections
        output_values = []
        for output in params.Output:
            data = output.VolatileData.AllData(True)
            v = [x.Value for x in data]
            if len(v)<1:
                output_values.append(None)
            elif len(v)==1:
                output_values.append(v[0])
            else:
                output_values.append(v)
        if len(output_values)==1: return output_values[0]
        if self.return_type is None:
            names = [output.Name.lower() for output in params.Output]
            try:
                t = collections.namedtuple('Output', names, rename=True)
                self.return_type = t
            except:
                self.return_type = False
        if not self.return_type: return output_values
        return self.return_type(*output_values)


def __build_module():
    def function_description(description, params):
        rc = ['',description, "Input:"]
        for param in params.Input:
            s = "\t{0} [{1}] - {2}"
            if param.Optional:
                s = "\t{0} (in, optional) [{1}] - {2}"
            rc.append(s.format(param.Name.lower(), param.TypeName, param.Description))
        if params.Output.Count == 1:
            param = params.Output[0]
            rc.append("Returns: [{0}] - {1}".format(param.TypeName, param.Description))
        elif params.Output.Count > 1:
            rc.append("Returns:")
            for out in params.Output:
                s = "\t{0} [{1}] - {2}"
                rc.append(s.format(out.Name.lower(), out.TypeName, out.Description))
        return '\n'.join(rc)

    import sys, types, string
    core_module = sys.modules['ghpython.components']
    translation = string.maketrans("'()*|+&", "_______")
    for obj in GH.Instances.ComponentServer.ObjectProxies:
        if obj.Exposure == GH.Kernel.GH_Exposure.hidden or obj.Obsolete:
            continue
        t = clr.GetClrType(GH.Kernel.IGH_Component)
        if not (t.IsAssignableFrom(obj.Type)):
            continue
        m = core_module
        library_id = obj.LibraryGuid
        assembly = GH.Instances.ComponentServer.FindAssembly(library_id)
        if not assembly.IsCoreLibrary:
            module_name = assembly.Assembly.GetName().Name
            if module_name in core_module.__dict__:
                m = core_module.__dict__[module_name]
            else:
                m = namespace_object()
                setattr(core_module, module_name, m)
        name = obj.Desc.Name.Replace(" ", "")
        if "LEGACY" in name or "#" in name:
            continue
        name = name.translate(translation)
        if not name[0].isalpha(): name = '_' + name
        function = __make_function__(function_helper(obj))
        if m == core_module:
            setattr(m, name, function)
            comp = obj.CreateInstance()
            a = m.__dict__[name]
            a.__name__ = name
            a.__doc__ = function_description(obj.Desc.Description, comp.Params)
        else:
            setattr(m, name, types.MethodType(function, m, type(m)))


__build_module()