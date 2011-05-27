using System.Collections.Generic;
using System.Text;

namespace GhPython.Component
{
    class StringList
    {
        List<string> _txts = new List<string>();

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
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyCollection<string>(_txts);
            }
        }

        public string GetResultAsOne()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < _txts.Count; i++)
                sb.AppendLine(_txts[i]);
            return sb.ToString();
        }
    }
}
