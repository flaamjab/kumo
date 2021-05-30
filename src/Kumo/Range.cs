#nullable enable

using System;
using System.Linq;
using System.Collections.Generic;

namespace Kumo
{
    /// <summary>Exposes a range which is a reference
    /// to a text fragment within the document.</summary>
    public class Range : IEquatable<Range>
    {
        private Body _holder;

        /// <summary>The start character position of the range.</summary>
        public int Start { get; }

        /// <summary>The character position directly after the end of the range.</summary>
        public int End { get; }

        /// <summary>Gets the properties attached to this <c>IRange</c></summary>
        public IEnumerable<Property> Properties => _holder.Properties(this);

        /// <summary>Gets the relation to other 
        /// <c>IRange</c>s set for this <c>IRange</c></summary>
        public IEnumerable<Range> Relations => _holder.Relations(this);

        internal Range(Body body, int start, int end)
        {
            _holder = body;
            Start = start;
            End = end;
        }

        /// <summary><para>Retrieves raw text within this <c>Range</c>.</para>
        /// <para>Note that paragraphs are generally not separated
        /// in any way in raw text. Use the <c>Paragraphs</c> method
        /// to split this <c>IRange</c> across paraagraphs.</para></summary>     
        /// <returns>Raw text that this <c>IRange</c> spans.</returns>  
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

        /// <summary>
        ///   Retrieves a separate <c>Range</c> for
        ///   each paragraph this <c>Range</c> spans.
        /// </summary>
        /// <returns>Paragraphs that this <c>Range</c> spans</returns>
        public IEnumerable<Range> Paragraphs()
        {
            throw new NotImplementedException();
        }

        /// <summary>Annotates this <c>Range</c>.</summary>
        /// <param name="annotator">The annotator to use.</param>
        public void Annotate(IAnnotator annotator)
        {
            throw new NotImplementedException();
        }

        /// <summary>Attaches a property to this <c>IRange</c>.</summary>
        /// <param name="property">The property to attach.</param>
        public void Attach(Property property)
        {
            _holder.Link(this, new Property[] { property });
        }

        /// <summary>Attaches an <c>IRange</c> to this <c>IRange</c>.</summary>
        /// <param name="range">The range to attach.</param>
        public void Attach(Range range)
        {
            throw new NotImplementedException();
        }

        /// <summary>Attaches a collection of properties to this <c>IRange</c>.</summary>
        /// <param name="properties">The properties to annotate with.</param>
        public void Attach(IEnumerable<Property> properties)
        {
            foreach (var p in properties)
            {
                Attach(p);
            }
        }

        /// <summary>Attaches a collection <c>IRange</c>s to this <c>IRange</c>.</summary>
        /// <param name="ranges">The ranges to attach.</param>
        public void Attach(IEnumerable<Range> ranges)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///   <para>Gets a value indicating whether this <c>IRange</c> is valid.</para>
        ///   <para>
        ///     A range is considered valid if it spans a sequence
        ///     of neighboring runs. Runs are considered neighbors if they
        ///     are within the same paragraph or directly neighboring
        ///     paragraphs and there are no other runs between them.
        ///   </para>
        ///   <para>For example, the following ranges are not valid:</para>
        ///   <list>
        ///     <item>
        ///       - A range that starts within a main body paragraph
        ///       but ends inside a table paragraph.
        ///     </item>
        ///     <item>
        ///       - A range that spans multiple table cells.
        ///     </item>
        ///   </list>
        ///   <para>Only valid ranges can be annotated with properties and other ranges.</para>
        /// </summary>
        /// <returns>The value indicating whether this range is valid or not.</returns>
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
        public static (int, int) Offsets(this Range range, Block block)
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
