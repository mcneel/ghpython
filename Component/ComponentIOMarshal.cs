using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Rhino.Geometry;
using GhPython.DocReplacement;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Parameters;
using Grasshopper;

namespace GhPython.Component
{
    class ComponentIOMarshal
    {
        dynamic _document; //GrasshopperDocument-like object
        dynamic _objectTable; //CustomTable-like object
        PythonComponent _component;


        public ComponentIOMarshal(object document, PythonComponent component)
        {
            _document = document;
            _objectTable = _document.Objects;
            _component = component;
        }



        #region Inputs

        public object GetInput(IGH_DataAccess DA, int i)
        {
            object o;
            switch (_component.Params.Input[i].Access)
            {
                case GH_ParamAccess.item:
                    o = GetItemFromParameter(DA, i);
                    break;

                case GH_ParamAccess.list:
                    o = GetListFromParameter(DA, i);
                    break;

                case GH_ParamAccess.tree:
                    o = GetTreeFromParameter(DA, i);
                    break;

                default:
                    throw new ApplicationException("Wrong parameter in variable access type");
            }

            return o;
        }


        private object GetItemFromParameter(IGH_DataAccess DA, int index)
        {
            IGH_Goo destination = null;
            DA.GetData<IGH_Goo>(index, ref destination);
            var toReturn = this.TypeCast(destination, index);

            if (_component.DocStorageMode == DocStorage.AutomaticMarshal)
                DocumentSingle(ref toReturn);

            return toReturn;
        }

        private object GetListFromParameter(IGH_DataAccess DA, int index)
        {
            List<IGH_Goo> list2 = new List<IGH_Goo>();
            DA.GetDataList<IGH_Goo>(index, list2);
            IGH_TypeHint typeHint = ((Param_ScriptVariable)_component.Params.Input[index]).TypeHint;
            var t = Type.GetType("IronPython.Runtime.List,IronPython");
            IList list = Activator.CreateInstance(t) as IList;

            if (_component.DocStorageMode != DocStorage.AutomaticMarshal)
            {
                for (int i = 0; i < list2.Count; i++)
                {
                    list.Add(this.TypeCast(list2[i], typeHint));
                }
            }
            else
            {
                for (int i = 0; i < list2.Count; i++)
                {
                    object thisInput = this.TypeCast(list2[i], typeHint);
                    if (DocumentSingle(ref thisInput))
                    {
                        list.Add(thisInput);
                    }
                }
            }

            return list;
        }

        private object GetTreeFromParameter(IGH_DataAccess DA, int index)
        {
            GH_Structure<IGH_Goo> structure = new GH_Structure<IGH_Goo>();
            DA.GetDataTree<IGH_Goo>(index, out structure);
            IGH_TypeHint typeHint = ((Param_ScriptVariable)_component.Params.Input[index]).TypeHint;
            var tree = new DataTree<object>();

            for (int i = 0; i < structure.PathCount; i++)
            {
                GH_Path path = structure.get_Path(i);
                List<IGH_Goo> list = structure.Branches[i];
                List<object> data = new List<object>();

                for (int j = 0; j < list.Count; j++)
                {
                    object cast = this.TypeCast(list[j], typeHint);

                    if(_component.DocStorageMode == DocStorage.AutomaticMarshal)
                        DocumentSingle(ref cast);

                    data.Add(cast);
                }
                tree.AddRange(data, path);
            }
            return tree;
        }

        private object TypeCast(IGH_Goo data, int index)
        {
            Param_ScriptVariable variable = (Param_ScriptVariable)_component.Params.Input[index];
            return this.TypeCast(data, variable.TypeHint);
        }

        private object TypeCast(IGH_Goo data, IGH_TypeHint hint)
        {
            if (data == null)
            {
                return null;
            }
            if (hint == null)
            {
                return data.ScriptVariable();
            }
            object objectValue = data.ScriptVariable();
            object target = null;
            hint.Cast(objectValue, out target);
            return target;
        }

        public bool DocumentSingle(ref object input)
        {
            if (input is GeometryBase)
            {
                input = _objectTable.__InternalAdd(input as dynamic);
                return true;
            }
            else if (input is Point3d)
            {
                input = _objectTable.__InternalAdd((Point3d)input);
                return true;
            }
            return false;
        }

        #endregion


        #region Outputs

        public void SetOutput(object o, IGH_DataAccess DA, int index)
        {
            if (o == null)
                return;

            if (o is GrasshopperDocument)
            {
                var ogh = o as GrasshopperDocument;
                DA.SetDataList(index, ogh.Objects.Geometries);
                ogh.Objects.Clear();
            }
            else if (o is string) //string is IEnumerable, so we need to check first
            {
                DA.SetData(index, o);
            }
            else if (o is IEnumerable)
            {
                if (_component.DocStorageMode == DocStorage.AutomaticMarshal)
                    o = GeometryList(o as IEnumerable);
                DA.SetDataList(index, o as IEnumerable);
            }
            else if (o is IGH_DataTree)
            {
                if (_component.DocStorageMode == DocStorage.AutomaticMarshal)
                {
                    try
                    {
                        o = (this as dynamic).GeometryTree(o as dynamic);
                    }
                    catch {}
                }
                DA.SetDataTree(index, o as IGH_DataTree);
            }
            else
            {
                if (_component.DocStorageMode == DocStorage.AutomaticMarshal)
                {
                    GeometrySingle(o, out o);
                }
                DA.SetData(index, o);
            }
        }

        bool GeometrySingle(object input, out object output)
        {
            if (input is Guid)
            {
                dynamic o = _objectTable.Find((Guid)input);
                if (o != null)
                {
                    output = o.Geometry;
                    return output != null;
                }
            }
            output = null;
            return false;
        }

        List<object> GeometryList(IEnumerable output)
        {
            List<object> newOutput = new List<object>();
            foreach(var o in output)
            {
                object toAdd;
                GeometrySingle(o, out toAdd);
                newOutput.Add(toAdd);
            }
            return newOutput;
        }

        IGH_DataTree GeometryTree<T>(DataTree<T> output)
        {
            DataTree<object> newOutput = new DataTree<object>();
            for (int b = 0; b < output.BranchCount; b++)
            {
                var p = output.Path(b);
                var currentBranch = output.Branch(b);
                var newBranch = GeometryList(currentBranch);
                newOutput.AddRange(newBranch, p);
            }
            return newOutput;
        }

        #endregion
    }
}