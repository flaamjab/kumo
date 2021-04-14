using System;

namespace Kumo
{
    class Annotation : IAnnotation
    {
        public IRange Range { get; private set; }

        public Property[] Properties { get; }

        public Annotation(Range range, Property[] properties)
        {
            Range = range;
        }        
    }
}
