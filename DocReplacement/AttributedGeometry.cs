using System;
using Grasshopper.Kernel.Types;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace GhPython.DocReplacement
{
    public struct AttributedGeometry : IEquatable<AttributedGeometry>
    {
        IGH_GeometricGoo _geometry;
        ObjectAttributes _attributes;

        public AttributedGeometry(IGH_GeometricGoo item, ObjectAttributes attr)
        {
            _geometry = item;
            _attributes = attr;
        }

        internal IGH_GeometricGoo GhGeometry
        {
            get
            {
                return _geometry;
            }
            set
            {
                _geometry = value;
            }
        }

        public object Geometry
        {
            get
            {
                if (object.ReferenceEquals(_geometry, null))
                    return null;
                    
                var toReturn = _geometry.ScriptVariable();

                if (toReturn is Point3d)
                    toReturn = new Point((Point3d)toReturn);

                return toReturn;
            }
        }

        public ObjectAttributes Attributes
        {
            get
            {
                return _attributes;
            }
            set
            {
                if (_attributes == null)
                    throw new ArgumentNullException();

                _attributes = value;
            }
        }

        public string Name
        {
            get
            {
                return object.ReferenceEquals(_attributes, null) ? null : _attributes.Name;
            }
            set
            {
                _attributes.Name = value;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", Geometry, (object)Attributes??"(default)");
        }

        public override int GetHashCode()
        {
            int val;
            if (Geometry == null)
                val = 0;
            else if (Attributes == null)
                val = Geometry.GetHashCode();
            else
                val = Geometry.GetHashCode() ^ (Attributes.GetHashCode() << 5);
            return val;
        }

        public bool Equals(AttributedGeometry other)
        {
            return Geometry == other.Geometry &&
                Attributes == other.Attributes;
        }

        public override bool Equals(object obj)
        {
            return (obj is AttributedGeometry) && Equals((AttributedGeometry)obj);
        }

        public static bool operator ==(AttributedGeometry one, AttributedGeometry other)
        {
            return one.Equals(other);
        }

        public static bool operator !=(AttributedGeometry one, AttributedGeometry other)
        {
            return !one.Equals(other);
        }
    }
}
