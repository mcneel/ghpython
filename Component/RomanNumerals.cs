using System;
using System.Collections.Generic;
using System.Text;
using Grasshopper.Kernel;

namespace GhPython.Component
{
    class ExtendedRomanNumeralsConstructor : IGH_ParamNameConstructor
    {
        const int DefaultStartNumber = 1;

        const string _minusSign = "-";
        const string _zeroSign = "n";
        static readonly int[] _values = new int[] { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
        static readonly string[] _numerals = new string[] { "m", "cm", "d", "cd", "c", "xc", "l", "xl", "x", "ix", "v", "iv", "i" };
        
        int _currentNumber;
        string _currentString;

        public ExtendedRomanNumeralsConstructor()
            : this(DefaultStartNumber)
        {
        }

        public ExtendedRomanNumeralsConstructor(int start)
        {
            _currentNumber = start;
            _currentString = MakeNumber(_currentNumber);
        }

        public string Current()
        {
            return _currentString;
        }

        public string Next()
        {
            return _currentString = MakeNumber(_currentNumber++);
        }

        private static string MakeNumber(int integer)
        {
            StringBuilder sb = new StringBuilder();

            if (integer == 0)
            {
                sb.Append(_zeroSign);
            }
            else
            {
                if (integer < 0)
                {
                    sb.Append(_minusSign);
                    integer = -integer;
                }

                while (integer >= 4000)
                {
                    sb.Append(_numerals[0]);
                    integer -= 1000;
                }

                for (int i = 0; i < _numerals.Length; i++)
                {
                    while (integer >= _values[i])
                    {
                        sb.Append(_numerals[i]);
                        integer -= _values[i];
                    }
                }
            }
            return sb.ToString();
        }

        public void Reset()
        {
            _currentString = MakeNumber(_currentNumber = DefaultStartNumber);
        }
    }
}
