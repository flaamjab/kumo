using System;
using System.Linq;

namespace Kumo
{
    class Annotation : IAnnotation
    {

        private Bookmark _subject;
        private Bookmark[] _crossrefs;

        public IRange Subject { get => _subject.Range; }

        public Property[] Properties { get; }

        public IRange[] Crossrefs {
            get => _crossrefs.Select(b => b.Range).ToArray();
        }

        public Annotation(
            Bookmark subject,
            Property[] properties,
            Bookmark[] crossrefs)
        {
            _subject = subject;
            Properties = properties;
            _crossrefs = crossrefs;
        }

        public Star ToStar()
        {
            throw new NotImplementedException();
        }
    }
}
