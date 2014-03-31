using System;
using System.Collections.Generic;
using Grasshopper.Kernel;

namespace GhPython.ScriptHelpers
{
  // See GH_StructureIterator in grasshopper for real implementation
  class GhPyDataAccess : IGH_DataAccess
  {
    readonly GH_Component m_component;
    readonly object[] m_output;
    readonly IList<object> m_data;

    public GhPyDataAccess(GH_Component parent, IList<object> inputData)
    {
      m_component = parent;
      m_data = inputData;
      int output_count = parent.Params.Output.Count;
      m_output = new object[output_count];
    }

    public object[] Output
    {
      get { return m_output; }
    }


    public void AbortComponentSolution()
    {
      throw new NotImplementedException();
    }

    public bool BlitData<TQ>(int paramIndex, Grasshopper.Kernel.Data.GH_Structure<TQ> tree, bool overwrite) where TQ : Grasshopper.Kernel.Types.IGH_Goo
    {
      throw new NotImplementedException();
    }

    public void DisableGapLogic(int paramIndex)
    {
      throw new NotImplementedException();
    }

    public void DisableGapLogic()
    {
      throw new NotImplementedException();
    }

    public bool GetData<T>(string name, ref T destination)
    {
      throw new NotImplementedException();
    }

    public bool GetData<T>(int index, ref T destination)
    {
      // If the parameter is empty, there is nothing to return.
      var data = m_data[index];
      if (data == null)
        return false;

      // Cast/Convert the data
      if (CastData<T>(data, out destination))
        return true;

      //Bummer, someone screwed up, sure hope it's not me.
      //  Dim t0 As String = MungeTypeNameForGUI(d_data.GetType.Name)
      //  Dim t1 As String = MungeTypeNameForGUI(GetType(T).Name)
      //  m_component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, String.Format("Invalid cast: {0} » {1}", t0, t1))
      return false;
    }


    public bool GetDataList<T>(string name, List<T> list)
    {
      throw new NotImplementedException();
    }

    public bool GetDataList<T>(int index, List<T> list)
    {
      throw new NotImplementedException();
    }

    public bool GetDataTree<T>(string name, out Grasshopper.Kernel.Data.GH_Structure<T> tree) where T : Grasshopper.Kernel.Types.IGH_Goo
    {
      throw new NotImplementedException();
    }

    public bool GetDataTree<T>(int index, out Grasshopper.Kernel.Data.GH_Structure<T> tree) where T : Grasshopper.Kernel.Types.IGH_Goo
    {
      throw new NotImplementedException();
    }

    public void IncrementIteration(IList<object> data)
    {
      throw new NotImplementedException();
    }

    public void IncrementIteration()
    {
      throw new NotImplementedException();
    }

    public int Iteration
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public int ParameterTargetIndex(int paramIndex)
    {
      throw new NotImplementedException();
    }

    public Grasshopper.Kernel.Data.GH_Path ParameterTargetPath(int paramIndex)
    {
      throw new NotImplementedException();
    }

    public bool SetData(string paramName, object data)
    {
      int index = m_component.Params.IndexOfOutputParam(paramName);
      return SetData(index, data);
    }

    public bool SetData(int paramIndex, object data, int itemIndexOverride)
    {
      throw new NotImplementedException();
    }

    public bool SetData(int paramIndex, object data)
    {
      if (paramIndex < 0 || paramIndex >= m_output.Length)
        return false;
      m_output[paramIndex] = data;
      return true;
    }

    public bool SetDataList(string paramName, System.Collections.IEnumerable data)
    {
      throw new NotImplementedException();
    }

    public bool SetDataList(int paramIndex, System.Collections.IEnumerable data, int listIndexOverride)
    {
      throw new NotImplementedException();
    }

    public bool SetDataList(int paramIndex, System.Collections.IEnumerable data)
    {
      throw new NotImplementedException();
    }

    public bool SetDataTree(int paramIndex, Grasshopper.Kernel.Data.IGH_Structure tree)
    {
      throw new NotImplementedException();
    }

    public bool SetDataTree(int paramIndex, Grasshopper.Kernel.Data.IGH_DataTree tree)
    {
      throw new NotImplementedException();
    }

    public int Util_CountNonNullRefs<T>(List<T> L)
    {
      throw new NotImplementedException();
    }

    public int Util_CountNullRefs<T>(List<T> L)
    {
      throw new NotImplementedException();
    }

    public bool Util_EnsureNonNullCount<T>(List<T> L, int N)
    {
      throw new NotImplementedException();
    }

    public int Util_FirstNonNullItem<T>(List<T> L)
    {
      throw new NotImplementedException();
    }

    public List<T> Util_RemoveNullRefs<T>(List<T> L)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Convert data from the unknown type to a target type. 
    /// Conversion is optimized and will only duplicate data if the types are not the same.
    /// </summary>
    /// <typeparam name="T">Target type of data conversion.</typeparam>
    /// <param name="inputData">Input, cannot be null.</param>
    /// <param name="outputData">Output, is expected to be null.</param>
    /// <returns>True on success, false on failure.</returns>
    private static bool CastData<T>(object inputData, out T outputData)
    {
      if (inputData is T)
      {
        //Return a straight cast
        outputData = (T)inputData;
        return true;
      }
      else
      {
        //Call the CastTo method on the data, maybe it knows how to convert itself into T
        //(Although nothing indicates that [in] implements IGH_Goo, it is logically impossible 
        //at the time of writing (september 02 2008) for it to be anything else.)
        var goo_data = (Grasshopper.Kernel.Types.IGH_Goo)inputData;
        if ((goo_data.CastTo<T>(out outputData)))
          return true;

        //Looks like it didn't. If Destination implements IGH_Goo, perhaps it defines a conversion.
        if ((Grasshopper.Kernel.GH_TypeLib.t_gh_goo.IsAssignableFrom(typeof(T))))
        {
          if ((outputData == null))
          {
            //Destination is nothing, so we need to create a new instance of type T
            var temp_instance = (Grasshopper.Kernel.Types.IGH_Goo)System.Activator.CreateInstance(typeof(T));
            if ((temp_instance.CastFrom(goo_data)))
            {
              outputData = (T)temp_instance;
              return true;
            }
          }
          else
          {
            //Destination is already filled in, so we can call the caster directly
            if (((Grasshopper.Kernel.Types.IGH_Goo)outputData).CastFrom(goo_data))
              return true;
          }
        }
      }

      return false;
    }

  }
}
