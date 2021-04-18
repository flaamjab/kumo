#nullable enable

using System;
using System.Linq;
using System.Collections.Generic;

namespace Kumo
{
    class Range : IRange, IEquatable<Range>
    {
        private Body _holder;

        public int Start { get; }

        public int End { get; }

        public Range(Body body, int start, int end)
        {
            _holder = body;
            Start = start;
            End = end;
        }

        public string Text()
        {
            var block = _holder.Block(this);
            var textValues = block.Nodes.Select(n => n.Text);
            var text = String.Join("", textValues);

            var (leftOffset, rightOffset) = this.Offsets(block);

            return text.Substring(
                leftOffset,
                text.Length - leftOffset - rightOffset
            );
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
            return _holder.Annotate(
                this,
                new Property[] { property },
                new Range[0]
            );
        }

        public IAnnotation Annotate(
            Property property,
            IEnumerable<IRange> relations)
        {
            return _holder.Annotate(
                this,
                new Property[] { property },
                relations.ToArray()
            );
        }

        public IAnnotation Annotate(IEnumerable<Property> properties)
        {
            return _holder.Annotate(
                this,
                properties.ToArray(),
                new Range[0]
            );
        }

        public IAnnotation Annotate(
            IEnumerable<Property> properties,
            IEnumerable<IRange> relations)
        {
            return _holder.Annotate(
                this,
                properties.ToArray(),
                relations.ToArray()
            );
        }

        public void Reannotate(Property property)
        {
            throw new NotImplementedException();
        }

        public void Reannotate(IEnumerable<Property> properties)
        {
            throw new NotImplementedException();
        }

        public IAnnotation Annotation()
        {
            return _holder.Annotation(this);
        }

        public bool Annotated()
        {
            return _holder.Annotated(this);
        }

        public bool Valid()
        {
            throw new NotImplementedException();
        }

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
    }

    static class RangeExtensions
    {
        public static (int, int) Offsets(this IRange range, Block block)
        {
            if (range.Start < block.Start || block.End < range.End
                || block.End < range.Start)
            {
                throw new ArgumentOutOfRangeException(
                    "the Range must be contained within the block"
                );
            }

            int leftOffset = range.Start - block.Start;
            int rightOffset = block.End - range.End;

            return (leftOffset, rightOffset);
        }
    }
}
