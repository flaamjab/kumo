using System.Linq;

namespace Kumo
{
    class Annotation : IAnnotation
    {
        public IRange Subject { get; }

        public Property[] Properties { get; }

        public IRange[] Relations { get; }

        public Annotation(
            IRange subject,
            Property[] properties,
            IRange[] relations)
        {
            Subject = subject;
            Properties = properties;
            Relations = relations;
        }

        public Description ToDescription(BookmarkTable lookup)
        {
            var subject = lookup.Get(Subject);
            var relations = Relations
                .Select(cr => lookup.Get(cr).Id)
                .ToArray();

            return new Description(subject.Id, Properties, relations);
        }
    }
}
