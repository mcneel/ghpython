using System;
using Grasshopper.Kernel.Types;
using Rhino.DocObjects;

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

        public IGH_GeometricGoo Geometry
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

        public ObjectAttributes Attributes
        {
            get
            {
                return _attributes;
            }
            set
            {
                _attributes = value;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", Geometry, (object)Attributes??"[null]");
        }

        public override int GetHashCode()
        {
            int val;
            if (Geometry == null || Attributes == null)
                val = 0;
            else
                val = Geometry.GetHashCode() ^ Attributes.GetHashCode();
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
