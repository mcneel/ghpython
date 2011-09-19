using System.Security.Cryptography;
using System.Text;

namespace GhPython.Component
{
    class TextHashMaintainer
    {
        byte[] _hash;

        public bool IsSameHashAsBefore(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                text = string.Empty;
            }

            if (_hash == null)
            {
                _hash = ComputeHash(text);
                return false;
            }

            var newHash = ComputeHash(text);
            return CompareBytes(newHash, _hash);
        }

        public void HashText(string text)
        {
            _hash = ComputeHash(text);
        }

        private static bool CompareBytes(byte[] hash, byte[] other)
        {
            if (hash == null || other == null)
                return hash == other;

            if (hash.Length == other.Length)
            {
                bool equal = true;

                for (int i = 0; i < hash.Length; i++)
                {
                    if (hash[i] != other[i])
                    {
                        equal = false;
                        break;
                    }
                }
                return equal;
            }
            return false;
        }

        private byte[] ComputeHash(string text)
        {
            using(var p = new SHA512CryptoServiceProvider())
            {
                return p.ComputeHash(Encoding.UTF8.GetBytes(text));
            }
        }
    }
}