using System;
using System.Collections.Generic;

namespace Kumo
{
    public class Range : IRange
    {
        public Range()
        {

        }

        public string Text()
        {
            throw new NotImplementedException();
        }

        public void Annotate(Annotation annotation)
        {
            throw new NotImplementedException();
        }

        public void Annotate(IEnumerable<Annotation> annotations)
        {
            throw new NotImplementedException();
        }

        public void Annotate(IAnnotator annotator)
        {
            throw new NotImplementedException();
        }
    }
}
