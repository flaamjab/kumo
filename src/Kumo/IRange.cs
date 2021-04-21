#nullable enable

using System.Collections.Generic;

namespace Kumo
{
    /// <summary>Exposes a range which is a reference
    /// to a text fragment within the document.</summary>
    public interface IRange
    {
        /// <summary>Gets the properties attached to this <c>IRange</c></summary>
        public IEnumerable<Property> Properties { get; }

        /// <summary>Gets the relation to other 
        /// <c>IRange</c>s set for this <c>IRange</c></summary>
        public IEnumerable<IRange> Relations { get; }

        /// <summary>The start character position of the range.</summary>
        public int Start { get; }

        /// <summary>The character position directly after the end of the range.</summary>
        public int End { get; }

        /// <summary><para>Retrieves raw text within this <c>Range</c>.</para>
        /// <para>Note that paragraphs are generally not separated
        /// in any way in raw text. Use the <c>Paragraphs</c> method
        /// to split this <c>IRange</c> across paraagraphs.</para></summary>     
        /// <returns>Raw text that this <c>IRange</c> spans.</returns>   
        public string Text();

        /// <summary>
        ///   Retrieves a separate <c>IRange</c> for
        ///   each paragraph this <c>IRange</c> spans.
        /// </summary>
        /// <returns>Paragraphs that this <c>IRange</c> spans</returns>
        public IEnumerable<IRange> Paragraphs();

        /// <summary>Annotates this <c>IRange</c>.</summary>
        /// <param name="annotator">The annotator to use.</param>
        public void Annotate(IAnnotator annotator);

        /// <summary>Attaches a property to this <c>IRange</c>.</summary>
        /// <param name="property">The property to attach.</param>
        public void Attach(Property property);

        /// <summary>Attaches an <c>IRange</c> to this <c>IRange</c>.</summary>
        /// <param name="range">The range to attach.</param>
        public void Attach(IRange range);

        /// <summary>Attaches a collection of properties to this <c>IRange</c>.</summary>
        /// <param name="properties">The properties to annotate with.</param>
        public void Attach(IEnumerable<Property> properties);

        /// <summary>Attaches a collection <c>IRange</c>s to this <c>IRange</c>.</summary>
        /// <param name="ranges">The ranges to attach.</param>
        public void Attach(IEnumerable<IRange> ranges);

        /// <summary>
        ///   <para>Checks whether this <c>IRange</c> is valid.</para>
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
        public bool Valid();
    }
}
