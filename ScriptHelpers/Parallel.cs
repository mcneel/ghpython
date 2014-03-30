using System;
using System.Collections.Generic;
using System.Linq;

namespace GhPython.ScriptHelpers
{
  public static class Parallel
  {
    public static IEnumerable<object> Run(Func<object, object> function, IList<object> data, bool flatten)
    {
      if (data == null || data.Count < 1)
        return null;
      object[] rc = new object[data.Count];
      // Run the first operation serial to account for classes attempting to lazily create data
      rc[0] = function(data[0]);
      if( rc.Length>1 )
        System.Threading.Tasks.Parallel.For(1, rc.Length, (i) => { rc[i] = function(data[i]); });
      if (!flatten)
        return rc;
      List<object> flat = null;
      // see if the results are lists
      foreach (object obj in rc)
      {
        if (obj == null)
          continue;
        System.Collections.IEnumerable e = obj as System.Collections.IEnumerable;
        if (e == null)
          break;
        int sub_length = e.Cast<object>().Count();
        flat = new List<object>(rc.Length * sub_length); //good guess
        break;
      }

      if (flat == null)
        return rc;
      foreach (object obj in rc)
      {
        if (obj == null)
          continue;
        System.Collections.IEnumerable e = obj as System.Collections.IEnumerable;
        if (e == null)
        {
          flat.Add(obj);
        }
        else
        {
          foreach (object subitem in e)
            flat.Add(subitem);
        }
      }
      return flat;
    }

  }
}
