using System.Collections.Generic;
using System.Text;

namespace GhPython.Component
{
  /// <summary>
  /// Used to capture the output stream from an executing python script
  /// </summary>
  class StringList
  {
    private readonly List<string> m_txts = new List<string>();

    public void Write(string s)
    {
      if (s == null) s = string.Empty;

      // print() seems to always adds a \n char at the end of the string
      // we want to counteract that
      if (s.EndsWith("\n")) s = s.Remove(s.Length - 1);

      m_txts.Add(s);
    }

    public void Reset()
    {
      m_txts.Clear();
    }

    public IList<string> Result
    {
      get { return new System.Collections.ObjectModel.ReadOnlyCollection<string>(m_txts); }
    }

    public override string ToString()
    {
      if (m_txts.Count == 0) return string.Empty;

      var sb = new StringBuilder(m_txts[0]);
      for (int i = 1; i < m_txts.Count; i++)
      {
        sb.AppendLine().Append(m_txts[i]);
      }

      return sb.ToString();
    }
  }
}
