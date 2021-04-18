#nullable enable

using System.Collections.Generic;

namespace Kumo
{
    /// <summary>
    ///   Exposes a range, which references a text excerpt
    ///   within a document.
    /// </summary>
    public interface IRange
    {
        /// <summary>The start character position of the range.</summary>
        public int Start { get; }

        /// <summary>The character position directly after the end of the range.</summary>
        public int End { get; }

        /// <summary>
        ///   <para>Retrieves raw text within this <c>Range</c>.</para>
        ///   <para>
        ///     Note that paragraphs are generally not separated
        ///     in any way in raw text.
        ///     Use the <c>Paragraphs</c> method to split this <c>IRange</c>
        ///     across paraagraphs.
        ///   </para>
        /// </summary>        
        public string Text();

        /// <summary>
        ///   Retrieves a separate <c>IRange</c> for
        ///   each paragraph this <c>IRange</c> spans.
        /// </summary>
        public IEnumerable<IRange> Paragraphs();

        /// <summary>Annotates this <c>IRange</c>.</summary>
        /// <param name="annotator">The annotator to use.</param>
        public void Annotate(IAnnotator annotator);

        /// <summary>Annotate this <c>IRange</c>.</summary>
        /// <param name="property">The property to annotate with.</param>
        public IAnnotation Annotate(Property property);

        /// <summary>Annotate this <c>IRange</c>.</summary>
        /// <param name="property">The property to annotate with.</param>
        /// <param name="relations">The ranges this range relates to.</param>
        public IAnnotation Annotate(
            Property property,
            IEnumerable<IRange> relations
        );

        /// <summary>Annotate this <c>IRange</c>.</summary>
        /// <param name="properties">The properties to annotate with.</param>
        public IAnnotation Annotate(IEnumerable<Property> properties);

        /// <summary>Annotate this <c>IRange</c>.</summary>
        /// <param name="properties">The properties to annotate with.</param>
        /// <param name="relations">The ranges this range relates to.</param>
        public IAnnotation Annotate(
            IEnumerable<Property> properties,
            IEnumerable<IRange> relations
        );

        /// <summary>
        ///   Annotate this <c>IRange</c> replacing its
        ///   current annotation if it exists.
        /// </summary>
        /// <param name="property>">The property to annotate with.</param>
        public void Reannotate(Property property);

        /// <summary>
        ///   Annotate this <c>IRange</c> replacing its
        ///   current annotation if it exists.
        /// </summary>
        /// <param name="properties">The properties to annotate this range with</param>
        public void Reannotate(IEnumerable<Property> properties);

        /// <summary>Fetches the annotation for this <c>IRange</c>.</summary>
        public IAnnotation Annotation();

        /// <summary>
        ///   Returns the value indicating whether this <c>IRange</c>
        ///   has an annotation associated with it
        /// </summary>
        public bool Annotated();

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
        ///   <para>Only valid ranges can be annotated.</para>
        /// </summary>
        public bool Valid();
    }
}
