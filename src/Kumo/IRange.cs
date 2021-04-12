using System.Collections.Generic;

namespace Kumo
{
    public interface IRange
    {
        /** <summary>
                <para>Retrieves raw text within this <c>Range</c>.</para>
                <para>
                    Note that raw text contains no newlines, therefore
                    paragraphs are not separated in any way.
                    Use the <c>Paragraphs</c> method to get <c>Range</c>s
                    for each paragraph within this <c>Range</c>.
                </para>
            </summary>
        */
        public string Text();

        /** <summary>
                Retrieves a separate <c>Range</c> for
                each paragraph this <c>Range</c> spans.
            </summary>
        */
        public IEnumerable<Range> Paragraphs();

        /// <summary>Annotate this <c>Range</c> with <c>annotation</c></summary>
        public void Annotate(Annotation annotation);

        /// <summary>Annotate this <c>Range</c> with <c>annotations</c></summary>
        public void Annotate(IEnumerable<Annotation> annotations);

        /** <summary>
                Annotate this <c>Range</c> using
                the provided <c>IAnnotator</c>.
           </summary>
        */
        public void Annotate(IAnnotator annotator);
    }
}
