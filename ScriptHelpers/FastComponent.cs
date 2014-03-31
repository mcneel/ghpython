using System;
using System.Collections.Generic;
using Grasshopper.Kernel;

namespace GhPython.ScriptHelpers
{
  public static class FastComponent
  {
    public static List<object[]> Run(GH_Component component, IList<object> data, System.Collections.IDictionary kwargs)
    {
      Type t = component.GetType();
      var method = t.GetMethod("SolveInstance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

      List<object[]> input = BuildInputList(data);

      int output_count = component.Params.Output.Count;
      int iterations = input.Count;
      List<object[]> output = new List<object[]>(output_count);
      for (int i = 0; i < output_count; i++)
        output.Add(new object[iterations]);

      bool run_parallel = false;
      if (kwargs != null && kwargs.Contains("multithreaded"))
        run_parallel = (bool)kwargs["multithreaded"];

      if (run_parallel)
      {
        System.Threading.Tasks.Parallel.For(0, input.Count, (iteration) => SolveIteration(iteration, component, input, output, method));
      }
      else
      {
        for( int iteration=0; iteration<input.Count; iteration++)
        {
          SolveIteration(iteration, component, input, output, method);
        };
      }
      return output;
    }

    static void SolveIteration(int iteration, GH_Component component, List<object[]> input, List<object[]> output, System.Reflection.MethodInfo method)
    {
      var da = new GhPyDataAccess(component, input[iteration]);
      method.Invoke(component, new object[] { da });
      object[] solve_results = da.Output;
      if (solve_results != null)
      {
        for (int j = 0; j < solve_results.Length; j++)
        {
          output[j][iteration] = solve_results[j];
        }
      }
    }

    static List<object[]> BuildInputList(IList<object> data)
    {
      List<object> data_list = new List<object>(data);
      List<object[]> rc = new List<object[]>();
      BuildInputHelper(0, data_list, ref rc);
      return rc;
    }
    static void BuildInputHelper(int column, IList<object> data, ref List<object[]> input)
    {
      if (column == data.Count)
      {
        object[] items= new object[data.Count];
        data.CopyTo(items,0);
        input.Add(items);
        return;
      }
      var list = data[column] as System.Collections.IList;
      if (list != null)
      {
        foreach (var item in list)
        {
          data[column] = item;
          BuildInputHelper(column + 1, data, ref input);
        }
        data[column] = list;
      }
      else
        BuildInputHelper(column + 1, data, ref input);
    }
  }
}
