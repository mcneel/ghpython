using System;
using System.Collections;
using System.Collections.Generic;
using GhPython.DocReplacement;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace GhPython.Component
{
  abstract class ComponentIOMarshal
  {
    public abstract object GetInput(IGH_DataAccess DA, int i);
    public abstract void SetOutput(object o, IGH_DataAccess DA, int index);
  }

  sealed class NewComponentIOMarshal : ComponentIOMarshal
  {
    private readonly ZuiPythonComponent m_component;
    private readonly CustomTable m_objectTable;
    private readonly GrasshopperDocument m_document;

    public NewComponentIOMarshal(GrasshopperDocument document, ZuiPythonComponent component)
    {
      m_document = document;
      m_objectTable = m_document.Objects;
      m_component = component;
    }

    #region Inputs

    public override object GetInput(IGH_DataAccess DA, int i)
    {
      var input = (Param_ScriptVariable) m_component.Params.Input[i];
      bool addIntoGhDoc = input.TypeHint is GhDocGuidHint;

      object o;
      switch (input.Access)
      {
        case GH_ParamAccess.item:
          o = GetItemFromParameter(DA, i, addIntoGhDoc);
          break;

        case GH_ParamAccess.list:
          o = GetListFromParameter(DA, i, addIntoGhDoc);
          break;

        case GH_ParamAccess.tree:
          o = GetTreeFromParameter(DA, i, addIntoGhDoc);
          break;

        default:
          throw new ApplicationException("Wrong parameter in variable access type");
      }

      return o;
    }


    private object GetItemFromParameter(IGH_DataAccess DA, int index, bool addIntoGhDoc)
    {
      IGH_Goo destination = null;
      DA.GetData(index, ref destination);
      var toReturn = this.TypeCast(destination, index);

      DocumentSingle(ref toReturn, addIntoGhDoc);

      return toReturn;
    }

    private object GetListFromParameter(IGH_DataAccess DA, int index, bool addIntoGhDoc)
    {
      List<IGH_Goo> list2 = new List<IGH_Goo>();
      DA.GetDataList(index, list2);
      IGH_TypeHint typeHint = ((Param_ScriptVariable) m_component.Params.Input[index]).TypeHint;
      var t = Type.GetType("IronPython.Runtime.List,IronPython");
      IList list = Activator.CreateInstance(t) as IList;

      if (list != null)
      {
        for (int i = 0; i < list2.Count; i++)
        {
          object cast = this.TypeCast(list2[i], typeHint);
          DocumentSingle(ref cast, addIntoGhDoc);
          list.Add(cast);
        }
      }

      return list;
    }

    private object GetTreeFromParameter(IGH_DataAccess DA, int index, bool addIntoGhDoc)
    {
      GH_Structure<IGH_Goo> structure;
      DA.GetDataTree(index, out structure);
      IGH_TypeHint typeHint = ((Param_ScriptVariable) m_component.Params.Input[index]).TypeHint;
      var tree = new DataTree<object>();

      for (int i = 0; i < structure.PathCount; i++)
      {
        GH_Path path = structure.get_Path(i);
        List<IGH_Goo> list = structure.Branches[i];
        List<object> data = new List<object>();

        for (int j = 0; j < list.Count; j++)
        {
          object cast = this.TypeCast(list[j], typeHint);
          DocumentSingle(ref cast, addIntoGhDoc);

          data.Add(cast);
        }
        tree.AddRange(data, path);
      }
      return tree;
    }

    private object TypeCast(IGH_Goo data, int index)
    {
      Param_ScriptVariable variable = (Param_ScriptVariable) m_component.Params.Input[index];
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
      object target;
      hint.Cast(objectValue, out target);
      return target;
    }

    public void DocumentSingle(ref object input, bool addIntoGhDoc)
    {
      if (addIntoGhDoc)
      {
        if (input is GeometryBase)
        {
          input = m_objectTable.__InternalAdd(input as dynamic);
        }
        else if (input is Point3d)
        {
          input = m_objectTable.AddPoint((Point3d) input);
        }
      }
    }

    #endregion


    #region Outputs

    public override void SetOutput(object o, IGH_DataAccess DA, int index)
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
        o = GeometryList(o as IEnumerable);
        DA.SetDataList(index, o as IEnumerable);
      }
      else if (o is IGH_DataTree)
      {
        try
        {
          o = (this as dynamic).GeometryTree(o as dynamic);
        }
        catch (Exception ex)
        {
          Rhino.Runtime.HostUtils.ExceptionReport(ex);
        }
        DA.SetDataTree(index, o as IGH_DataTree);
      }
      else if (o is System.Numerics.Complex)
      {
        // 8 August 2012 (S. Baer) - https://github.com/mcneel/ghpython/issues/17
        // Grasshopper doesn't internally support System.Numerics.Complex right now
        // and uses a built-in complex data structure.  Convert to GH complex when
        // we run into System.Numeric.Complex
        System.Numerics.Complex cplx = (System.Numerics.Complex)o;
        DA.SetData(index, new Complex(cplx.Real, cplx.Imaginary));
      }
      else
      {
        GeometrySingle(ref o);
        DA.SetData(index, o);
      }
    }

    private void GeometrySingle(ref object input)
    {
      if (input is Guid)
      {
        AttributedGeometry a = m_objectTable.Find((Guid) input);
        input = a.Geometry;
      }
    }

    private List<object> GeometryList(IEnumerable output)
    {
      List<object> newOutput = new List<object>();
      foreach (var o in output)
      {
        object toAdd = o;
        GeometrySingle(ref toAdd);
        newOutput.Add(toAdd);
      }
      return newOutput;
    }

    private IGH_DataTree GeometryTree<T>(DataTree<T> output)
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


  sealed class OldComponentIOMarshal : ComponentIOMarshal
  {
    private readonly dynamic _document; //GrasshopperDocument-like object
    private readonly dynamic _objectTable; //CustomTable-like object
    private readonly PythonComponent_OBSOLETE _component;


    public OldComponentIOMarshal(object document, PythonComponent_OBSOLETE component)
    {
      _document = document;
      _objectTable = _document.Objects;
      _component = component;
    }



    #region Inputs

    public override object GetInput(IGH_DataAccess DA, int i)
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
      DA.GetData(index, ref destination);
      var toReturn = this.TypeCast(destination, index);

      if (_component.DocStorageMode == DocStorage.AutomaticMarshal)
        DocumentSingle(ref toReturn);

      return toReturn;
    }

    private object GetListFromParameter(IGH_DataAccess DA, int index)
    {
      List<IGH_Goo> list2 = new List<IGH_Goo>();
      DA.GetDataList(index, list2);
      IGH_TypeHint typeHint = ((Param_ScriptVariable) _component.Params.Input[index]).TypeHint;
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
          DocumentSingle(ref thisInput);
          list.Add(thisInput);
        }
      }

      return list;
    }

    private object GetTreeFromParameter(IGH_DataAccess DA, int index)
    {
      GH_Structure<IGH_Goo> structure;
      DA.GetDataTree(index, out structure);
      IGH_TypeHint typeHint = ((Param_ScriptVariable) _component.Params.Input[index]).TypeHint;
      var tree = new DataTree<object>();

      for (int i = 0; i < structure.PathCount; i++)
      {
        GH_Path path = structure.get_Path(i);
        List<IGH_Goo> list = structure.Branches[i];
        List<object> data = new List<object>();

        for (int j = 0; j < list.Count; j++)
        {
          object cast = this.TypeCast(list[j], typeHint);

          if (_component.DocStorageMode == DocStorage.AutomaticMarshal)
            DocumentSingle(ref cast);

          data.Add(cast);
        }
        tree.AddRange(data, path);
      }
      return tree;
    }

    private object TypeCast(IGH_Goo data, int index)
    {
      Param_ScriptVariable variable = (Param_ScriptVariable) _component.Params.Input[index];
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
      object target;
      hint.Cast(objectValue, out target);
      return target;
    }

    public void DocumentSingle(ref object input)
    {
      if (input is GeometryBase)
      {
        input = _objectTable.__InternalAdd(input as dynamic);
      }
      else if (input is Point3d)
      {
        input = _objectTable.__InternalAdd((Point3d) input);
      }
    }

    #endregion


    #region Outputs

    public override void SetOutput(object o, IGH_DataAccess DA, int index)
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
          catch (Exception ex)
          {
            Rhino.Runtime.HostUtils.ExceptionReport(ex);
          }
        }
        DA.SetDataTree(index, o as IGH_DataTree);
      }
      else
      {
        if (_component.DocStorageMode == DocStorage.AutomaticMarshal)
        {
          GeometrySingle(ref o);
        }
        DA.SetData(index, o);
      }
    }

    private void GeometrySingle(ref object input)
    {
      if (input is Guid)
      {
        dynamic o = _objectTable.Find((Guid) input);
        if (o != null)
        {
          input = o.Geometry;
        }
      }
    }

    private List<object> GeometryList(IEnumerable output)
    {
      List<object> newOutput = new List<object>();
      foreach (var o in output)
      {
        object toAdd = o;
        GeometrySingle(ref toAdd);
        newOutput.Add(toAdd);
      }
      return newOutput;
    }

    private IGH_DataTree GeometryTree<T>(DataTree<T> output)
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