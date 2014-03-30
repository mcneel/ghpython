using System;
using System.Collections.Generic;
using Grasshopper.Kernel;

namespace GhPython.ScriptHelpers
{
  public static class FastComponent
  {
    public static List<object>[] Run(GH_Component component, IList<object> data)
    {
      var da = new GhPyDataAccess(component);
      Type t = component.GetType();
      var method = t.GetMethod("SolveInstance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      List<object> data_list = new List<object>(data);
      RunOnData(0, data_list, method, component, da);
      return da.Output;
    }

    static void RunOnData(int column, IList<object> data, System.Reflection.MethodInfo method, GH_Component component, GhPyDataAccess da)
    {
      if (column == data.Count)
      {
        da.IncrementIteration(data);
        method.Invoke(component, new object[] { da });
        return;
      }
      var list = data[column] as System.Collections.IList;
      if (list != null)
      {
        foreach (var item in list)
        {
          data[column] = item;
          RunOnData(column + 1, data, method, component, da);
        }
        data[column] = list;
      }
      else
        RunOnData(column + 1, data, method, component, da);
    }
  }
}
