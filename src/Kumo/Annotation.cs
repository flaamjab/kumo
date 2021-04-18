using System.Linq;

namespace Kumo
{
    class Annotation : IAnnotation
    {
        public IRange Subject { get; }

        public Property[] Properties { get; }

        public IRange[] Crossrefs { get; }

        public Annotation(
            Range subject,
            Property[] properties,
            Range[] crossrefs)
        {
            Subject = subject;
            Properties = properties;
            Crossrefs = crossrefs;
        }

        public Description ToDescription(BookmarkTable lookup)
        {
            var subject = lookup.Get(Subject);
            var crossrefs = Crossrefs
                .Select(cr => lookup.Get(cr).Id)
                .ToArray();

            return new Description(subject.Id, Properties, crossrefs);
        }
    }
}
