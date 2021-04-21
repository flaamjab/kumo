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

        public IEnumerable<Property> Properties => _holder.Properties(this);

        public IEnumerable<IRange> Relations => _holder.Relations(this);

        internal Range(Body body, int start, int end)
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

        public void Attach(Property property)
        {
            _holder.Link(this, new Property[] { property });
        }

        public void Attach(IRange range)
        {
            throw new NotImplementedException();
        }

        public void Attach(IEnumerable<Property> properties)
        {
            foreach (var p in properties)
            {
                Attach(p);
            }
        }

        public void Attach(IEnumerable<IRange> ranges)
        {
            throw new NotImplementedException();
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
