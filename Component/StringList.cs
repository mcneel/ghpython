using System;
using System.Collections.Generic;
using System.Text;

namespace GhPython.Component
{
    class StringList
    {
        List<string> _txts;

        public StringList()
        {
            _txts = new List<string>();
        }

        public void Write(string s)
        {
            _txts.Add(s);
        }

        public void Reset()
        {
            _txts.Clear();
        }

        public List<string> Result
        {
            get
            {
                return _txts;
            }
        }

        public string GetResultAsOne()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < _txts.Count; i++)
                sb.Append(_txts[i]);
            return sb.ToString();
        }
    }
}
