using System;
using System.Collections.Generic;
using System.IO;

namespace Kumo
{
    interface IPackage : IDisposable
    {
        public IContent Content { get; }
        public Uri Uri { get; }
        public void Save();
        public IPackage Clone();
        public IEnumerable<Property> Properties(Range range);
        public IEnumerable<Range> Stars();
        public void Link(Range range, IEnumerable<Property> properties);
        public void Unlink(Range range, IEnumerable<Property> properties);
        public Stream RdfStream();
    }
}
