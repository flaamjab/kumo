using System.Collections.Generic;

namespace Kumo
{
    public interface IRange
    {
        public string Text();

        public void Annotate(Annotation annotation);

        public void Annotate(IEnumerable<Annotation> annotations);

        public void Annotate(IAnnotator annotator);
    }
}
