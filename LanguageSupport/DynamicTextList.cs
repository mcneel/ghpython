using System;
using System.Collections.Generic;
using System.Dynamic;

namespace GhPython.Infrastructure
{
    //Do not make this class public or DynamicObject will be looked up too early and
    //the plug-in will fail to load
    class DynamicTextList : DynamicObject,
        IEnumerable<object>, IList<object>, IDictionary<string, object>
    {
        KeyValuePair<string, object>[] _array;
        int _position;
        bool _closed;

        public DynamicTextList(int length)
        {
            _array = new KeyValuePair<string, object>[length];
        }

        public void Add(string input, object value)
        {
            if (_closed)
                throw new NotSupportedException("This dictionary is read-only");

            if(_position == _array.Length)
                throw new NotSupportedException("Cannot add a value. This collection is full");

            //if (string.IsNullOrEmpty(input))
            //    throw new ArgumentNullException("The input in the list lookup is null");

            _array[_position++] = new KeyValuePair<string,object>(input, value);
        }

        public object this[int index]
        {
            get
            {
                if (index < 0)
                    throw new IndexOutOfRangeException(
                        string.Format("There are no negative spaces in this collection. [{0}] is invalid.",
                        index.ToString()));

                if (index >= _array.Length)
                    throw new IndexOutOfRangeException(
                        string.Format("The index [{0}] is bigger than the last allowed index [{1}].",
                        index.ToString(), (_array.Length - 1).ToString()));

                return _array[index].Value;
            }
            set
            {
                if (_closed)
                    throw new NotSupportedException("This dictionary is read-only");

                if (index < 0)
                    throw new IndexOutOfRangeException(
                        string.Format("There are no negative spaces in this collection. [{0}] is invalid.",
                        index.ToString()));

                if (index >= _array.Length)
                    throw new IndexOutOfRangeException(
                        string.Format("The index [{0}] is bigger than the last allowed index [{1}].",
                        index.ToString(), (_array.Length - 1).ToString()));

                string s = _array[index].Key;
                _array[index] = new KeyValuePair<string, object>(s, value);
            }
        }

        public object this[string key]
        {
            get
            {
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException("Cannot setup an empty or null key in the list lookup");

                for (int i = 0; i < _array.Length; i++)
                {
                    if (_array[i].Key == key)
                        return _array[i].Value;
                }
                throw new KeyNotFoundException(
                    string.Format("The key \"{0}\" is not present in the dictionary", key));
            }
            set
            {
                //if (string.IsNullOrEmpty(key))
                //    throw new ArgumentNullException("Cannot setup an empty or null key in the list lookup");

                if (_closed)
                    throw new NotSupportedException("This dictionary is read-only");

                int p = Array.FindIndex(_array, a => a.Key == key);
                if(p == -1)
                    throw new KeyNotFoundException(
                    string.Format("The key \"{0}\" is not present in the dictionary", key));

                _array[p] = new KeyValuePair<string, object>(key, value);
            }
        }


        #region Overrides of DynamicObject

        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
        {
            var baseRet = base.TryGetMember(binder, out result);

            if (!baseRet)
            {
                var n = binder.Name;

                for (int i = 0; i < _array.Length; i++)
                {
                    if (_array[i].Key == n)
                    {
                        result = _array[i].Value;
                        return true;
                    }
                }
            }
            return baseRet;
        }

        public override bool TrySetMember(System.Dynamic.SetMemberBinder binder, object value)
        {
            var baseRet = base.TrySetMember(binder, value);

            if (!baseRet && !_closed)
            {
                var n = binder.Name;

                for (int i = 0; i < _array.Length; i++)
                {
                    if (_array[i].Key == n)
                    {
                        _array[i] = new KeyValuePair<string,object>(n, value);
                        return true;
                    }
                }
            }
            return baseRet;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            var baseNames = BaseMethods();

            foreach (var i in baseNames)
                yield return i;

            for (int i = 0; i < _array.Length; i++)
            {
                if (!string.IsNullOrEmpty(_array[i].Key) && _array[i].Key.Trim().Length != 0)
                {
                    yield return _array[i].Key.Replace(" ", "");
                }
            }
        }

        // This is here to solve "Compiler Warning (level 1) CS1911"
        private IEnumerable<string> BaseMethods()
        {
            return base.GetDynamicMemberNames();
        }
        #endregion

        #region Members of IEnumerable<object>

        public IEnumerator<object> GetEnumerator()
        {
            foreach (var o in _array)
            {
                yield return o.Value;
            }
        }

        #endregion

        #region Members of IEnumerable

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Members of IDictionary<string,object>

        public bool ContainsKey(string key)
        {
            return Array.FindIndex(_array, (a) => a.Key == key) != -1;
        }

        public bool ContainsValue(object value)
        {
            return Array.FindIndex(_array, (a) => a.Value == value) != -1;
        }

        public ICollection<string> Keys
        {
            get
            {
                return new StringKeys(this);
            }
        }

        private class StringKeys : ICollection<string>
        {
            DynamicTextList _mother;

            public StringKeys(DynamicTextList mother)
            {
                _mother = mother;
            }

            #region ICollection<string> Membri di

            public void Add(string item)
            {
                throw new NotSupportedException("Cannot add to the indexable key collection. It is read-only.");
            }

            public void Clear()
            {
                throw new NotSupportedException("Cannot clear the indexable key collection. It is read-only.");
            }

            public bool Contains(string item)
            {
                return _mother.ContainsKey(item);
            }

            public void CopyTo(string[] array, int arrayIndex)
            {
                for (int i = 0; i < _mother._array.Length; i++)
                {
                    array[arrayIndex++] = _mother._array[i].Key;
                }
            }

            public int Count
            {
                get
                {
                    return _mother._array.Length;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public bool Remove(string item)
            {
                throw new NotSupportedException("Cannot add to the indexable key collection. It is read-only.");
            }

            #endregion

            #region IEnumerable<string> Membri di

            public IEnumerator<string> GetEnumerator()
            {
                for (int i = 0; i < _mother._array.Length; i++)
                {
                    yield return _mother._array[i].Key;
                }
            }

            #endregion

            #region IEnumerable Membri di

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

        public bool Remove(string key)
        {
            throw new NotSupportedException("Cannot remove to the key collection. It is add-only.");
        }

        public bool TryGetValue(string key, out object value)
        {
            var res = Array.FindIndex(_array, a => a.Key == key);
            if (res == -1)
            {
                value = null;
                return false;
            }
            else
            {
                value = _array[res].Value;
                return true;
            }
        }

        public ICollection<object> Values
        {
            get
            {
                return new ObjectValues(this);
            }
        }

        private class ObjectValues : ICollection<object>
        {
            DynamicTextList _mother;

            public ObjectValues(DynamicTextList mother)
            {
                _mother = mother;
            }

            #region ICollection<object> Membri di

            public void Add(object item)
            {
                throw new NotSupportedException("Cannot add to the value collection. It is read-only.");
            }

            public void Clear()
            {
                throw new NotSupportedException("Cannot clear the value collection. It is read-only.");
            }

            public bool Contains(object item)
            {
                return _mother.ContainsValue(item);
            }

            public void CopyTo(object[] array, int arrayIndex)
            {
                for (int i = 0; i < _mother._array.Length; i++)
                {
                    array[arrayIndex++] = _mother._array[i].Value;
                }
            }

            public int Count
            {
                get
                {
                    return _mother._array.Length;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public bool Remove(object item)
            {
                throw new NotSupportedException("Cannot add to the indexable key collection. It is read-only.");
            }

            #endregion

            #region IEnumerable<object> Membri di

            public IEnumerator<object> GetEnumerator()
            {
                for (int i = 0; i < _mother._array.Length; i++)
                {
                    yield return _mother._array[i].Key;
                }
            }

            #endregion

            #region IEnumerable Membri di

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

        #endregion

        #region Members of ICollection<KeyValuePair<string,object>>

        void ICollection<KeyValuePair<string,object>>.Add(KeyValuePair<string, object> item)
        {
            if (_position == _array.Length)
                throw new NotSupportedException("Cannot add a value. This collection is real-only");

            _array[_position++] = item;
        }

        public void Clear()
        {
            throw new NotSupportedException("Cannot clear values. This collection is real-only");
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return Array.FindIndex(_array, (a) => a.Value == item.Value && a.Key == item.Key) != -1;
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            Array.Copy(_array, 0, array, arrayIndex, _array.Length);
        }

        public int Count
        {
            get
            {
                return _array.Length;
            }
        }

        public void Close()
        {
            _closed = true;
        }

        public bool IsReadOnly
        {
            get
            {
                return !_closed;
            }
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new NotSupportedException("Cannot remove values. This collection is add-only");
        }

        #endregion

        #region Members of IEnumerable<KeyValuePair<string,object>>

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            for (int i = 0; i < _array.Length; i++)
                yield return _array[i];
        }

        #endregion



        #region IList<object> Membri di

        int IList<object>.IndexOf(object item)
        {
            return Array.FindIndex(_array, a => a.Value == item);
        }

        void IList<object>.Insert(int index, object item)
        {
            throw new NotSupportedException();
        }

        void IList<object>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region ICollection<object> Membri di

        void ICollection<object>.Add(object item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<object>.Contains(object item)
        {
            return Array.FindIndex(_array, a => a.Value == item) != -1;
        }

        void ICollection<object>.CopyTo(object[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        bool ICollection<object>.Remove(object item)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
