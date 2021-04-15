#nullable enable

using System;
using System.Collections.Generic;

namespace Kumo
{
    class Range : IRange, IEquatable<Range>
    {
        private Body _parent;

        public IAnnotation? Annotation { get; private set; } = null;

        public int Start { get; }

        public int End { get; }

        public Range(Body body, int start, int end)
        {
            _parent = body;
            Start = start;
            End = end;
        }

        public string Text()
        {
            return _parent.Text(this);
        }

        public IEnumerable<IRange> Paragraphs()
        {
            throw new NotImplementedException();
        }

        public void Annotate(IAnnotator annotator)
        {
            throw new NotImplementedException();
        }

        public IAnnotation Annotate(Property property)
        {
            throw new NotImplementedException();
        }

        public IAnnotation Annotate(IEnumerable<Property> properties)
        {
            throw new NotImplementedException();
        }

        public void Reannotate(Property property)
        {
            throw new NotImplementedException();
        }

        public void Reannotate(Property[] properties)
        {
            throw new NotImplementedException();
        }

        public bool IsValid()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Range);
        }

        public bool Equals(Range? other)
        {
            if (other is null)
            {
                return false;
            }

            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            if (GetType() != other.GetType())
            {
                return false;
            }

            return (Start, End) == (other.Start, other.End);
        }

        public override int GetHashCode() => (Start, End).GetHashCode();

        public static bool operator ==(Range lhs, Range rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                return false;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(
            Range lhs, Range rhs
        ) => !(lhs == rhs);
    }
}
