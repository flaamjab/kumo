﻿#nullable enable

using System;
using System.Collections.Generic;
using Kumo.OOXML;

namespace Kumo
{
    /// <summary>Represents a range which is a reference
    /// to a text fragment within the document.</summary>
    public class Range : IEquatable<Range>
    {
        private readonly IPackage _holder;

        private readonly Lazy<Uri> _uri;

        /// <summary>
        ///   Gets the URI that uniquely identifies a range within
        ///   the current document.
        /// </summary>
        public Uri Uri => _uri.Value;

        /// <summary>The start character position of the range.</summary>
        public int Start { get; }

        /// <summary>The character position directly after the end of the range.</summary>
        public int End { get; }

        /// <summary>Gets the properties attached to this <c>Range</c></summary>
        public IEnumerable<Property> Properties => _holder.Properties(this);

        internal Range(Package holder, int start, int end)
        {
            _holder = holder;
            Start = start;
            End = end;

            _uri = new Lazy<Uri>(() => new Uri(
                $"{_holder.Uri.OriginalString}#range{Start}-{End}"
            ));
        }

        /// <summary>Retrieves raw text within this <c>Range</c>.</summary>
        /// <remarks>Note that paragraphs are generally not separated
        /// in any way in raw text. Use the <c>Paragraphs</c> method
        /// to split this <c>Range</c> across paraagraphs.</remarks>
        /// <returns>Raw text that this <c>Range</c> spans.</returns>  
        public string Text()
        {
            return _holder.Content.Text(Start, End);
        }

        /// <summary>
        ///   Retrieves a separate <c>Range</c> for
        ///   each paragraph this <c>Range</c> spans.
        /// </summary>
        /// <returns>Paragraphs that this <c>Range</c> spans</returns>
        public IEnumerable<Range> Paragraphs()
        {
            return _holder.Content.Paragraphs();
        }

        /// <summary>Annotates this <c>Range</c>.</summary>
        /// <param name="annotator">The annotator to use.</param>
        public void Annotate(IAnnotator annotator)
        {
            throw new NotImplementedException();
        }

        /// <summary>Attaches a property to this <c>Range</c>.</summary>
        /// <param name="property">The property to attach.</param>
        public void Attach(Property property)
        {
            _holder.Link(this, new Property[] { property });
        }

        /// <summary>Attaches multiple properties to this <c>Range</c>.</summary>
        /// <param name="properties">The properties to annotate with.</param>
        public void Attach(IEnumerable<Property> properties)
        {
            _holder.Link(this, properties);
        }

        /// <summary>Removes a property from this <c>Range</c>.</summary>
        /// <param name="property">The property to remove.</param>
        /// <remarks>No error will be thrown if the property being detached
        /// is not associated with this <c>Range</c>.</remarks>
        public void Detach(Property property)
        {
            _holder.Unlink(this, new Property[] { property });
        }

        /// <summary>Removes multiple properties from this <c>Range</c>.</summary>
        /// <param name="properties">The properties to remove.</param>
        /// <remarks>No error will be thrown in the case a property being detached
        /// is not associated with this <c>Range</c>.</remarks>
        public void Detach(IEnumerable<Property> properties)
        {
            _holder.Unlink(this, properties);
        }

        /// <summary>Decides whether two Ranges are same.</summary>
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

        /// <summary>Decides whether two Ranges are different.</summary>
        public static bool operator !=(
            Range lhs, Range rhs
        ) => !(lhs == rhs);

        /// <summary>Decides whether this Range equals to some object.</summary>
        public override bool Equals(object? obj)
        {
            return Equals(obj as Range);
        }

        /// <summary>Decides whether this Range equals to some other Range.</summary>
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

        /// <summary>Calculates the hash code for the current <c>Range</c> instance.</summary>
        /// <returns>The hash code for the current <c>Range</c> instance.</returns>
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
