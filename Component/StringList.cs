using System.Collections.Generic;
using System.Text;

namespace GhPython.Component
{
  /// <summary>
  /// Used to capture the output stream from an executing python script
  /// </summary>
  class StringList
  {
    private readonly List<string> _txts = new List<string>();

    public void Write(string s)
    {
      if (s == null) s = string.Empty;

      // print() seems to always adds a \n char at the end of the string
      // we want to counteract that
      if (s.EndsWith("\n")) s = s.Remove(s.Length - 1);

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

    public override string ToString()
    {
      if (_txts.Count == 0) return string.Empty;

      var sb = new StringBuilder(_txts[0]);
      for (int i = 1; i < _txts.Count; i++)
      {
        sb.AppendLine().Append(_txts[i]);
      }

      return sb.ToString();
    }
  }
}
