using System.Collections.Generic;
using System.Text;

namespace GhPython.Component
{
  internal class StringList
  {
    private readonly List<string> _txts = new List<string>();

    public void Write(string s)
    {
      _txts.Add(s);
    }

    public void Reset()
    {
      _txts.Clear();
    }

    public IList<string> Result
    {
      get { return new System.Collections.ObjectModel.ReadOnlyCollection<string>(_txts); }
    }

    public string GetResultAsOne()
    {
      var sb = new StringBuilder();
      foreach (string s in _txts)
        sb.AppendLine(s);
      return sb.ToString();
    }
  }
}
