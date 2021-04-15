using System;

namespace Kumo
{
    class Annotation : IAnnotation
    {
        public string ID { get; }

        public IRange Range { get; private set; }

        public Property[] Properties { get; }

        public IRange[] References { get; }

        public Annotation(
            string id,
            Range range,
            Property[] properties,
            IRange[] references)
        {
            ID = id;
            Range = range;
            Properties = properties;
            References = references;
        }        
    }
}
