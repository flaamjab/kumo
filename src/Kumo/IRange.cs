using System.Collections.Generic;

namespace Kumo
{
    public interface IRange
    {
        /// <summary>Retrieves the text within this <c>Range</c>.</summary>
        public string Text();

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
