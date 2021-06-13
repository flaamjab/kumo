#nullable enable

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Packaging;

namespace Kumo
{
    // Represents an OOXML package.
    class Package : IDisposable
    {
        private WordprocessingDocument _doc;
        private BookmarkTable _bookmarkTable;
        private Lazy<RdfStore> _rdf;
        private UriStore _uri;
        private bool _autoSave;

        private RdfStore Rdf => _rdf.Value;

        public Content Content { get; }

        public Uri Uri => _uri.Value;

        public Package(WordprocessingDocument document, bool autoSave)
        {
            Content = new Content(this, document.MainDocumentPart.Document);
            _doc = document;

            _rdf = new Lazy<RdfStore>(() =>
                {
                    var rdf = new RdfStore(autoSave);
                    var part = CustomXmlPart(RdfStore.ID);

                    if (part is not null)
                    {
                        using (var s = part.GetStream(
                            FileMode.Open,
                            FileAccess.Read))
                        {
                            rdf.Load(s);
                        }
                    }

                    return rdf;
                }
            );

            _uri = new UriStore(document.MainDocumentPart);
            _bookmarkTable = new BookmarkTable(this);
        }

        public void Dispose()
        {
            _doc.Dispose();
        }

        public void Save()
        {
            _doc.Save();

            var part = CustomXmlPart(RdfStore.ID);
            if (part is null)
            {
                part = NewCustomXmlPart(RdfStore.ID, "text/plain");
            }

            using (var s = part.GetStream(FileMode.Open, FileAccess.Write))
            {
                Rdf.Save(s);
            }
        }

        public Package Clone()
        {
            var clone = (WordprocessingDocument)_doc.Clone();
            return new Package(clone, _autoSave);
        }

        public IEnumerable<Property> Properties(Range range)
        {
            if (Known(range))
            {
                var link = Link(range);
                return link.Properties;
            }
            else
            {
                return new Property[0];
            }
        }

        public IEnumerable<Range> Stars()
        {
            var g = Rdf.RangeGraph;
            var stars = _bookmarkTable
                .Bookmarks()
                .Where(b => g.Exists(b.Range.Uri))
                .Select(b => b.Range);

            return stars;
        }

        public void Link(Range range, IEnumerable<Property> properties)
        {
            if (!_bookmarkTable.Marked(range))
            {
                _bookmarkTable.Mark(range);
            }

            var link = new Link(range.Uri, properties);

            var g = Rdf.RangeGraph;
            g.Assert(link);
        }

        public void Unlink(Range range, IEnumerable<Property> properties)
        {
            // Fetch the bookmark for this range.
            // If the bookmark does not exist throw an exception.
            // Otherwise, drop the bookmark, deassociate it with the range.
            // Using the bookmark's ID, ask the rdfStore to remove
            // the annotation.

            throw new NotImplementedException();
        }

        private bool Known(Range range)
        {
            var graph = Rdf.RangeGraph;
            if (_bookmarkTable.Marked(range))
            {
                var b = _bookmarkTable.Lookup(range);
                if (graph.Exists(range.Uri))
                {
                    return true;
                }
            }

            return false;
        }

        private Link Link(Range range)
        {
            var b = _bookmarkTable.Lookup(range);
            var g = Rdf.RangeGraph;
            if (!g.Exists(range.Uri))
            {
                throw new InvalidOperationException(
                    "The range is bookmarked but not present in the RDF store"
                );
            }

            return g.Link(range.Uri);
        }

        private CustomXmlPart? CustomXmlPart(string id)
        {
            _doc.MainDocumentPart.TryGetPartById(id, out var part);
            if (part is not null)
            {
                return _doc.MainDocumentPart.GetPartById(id) switch
                {
                    CustomXmlPart p => p,
                    _ => throw new ArgumentException(
                        $"The part with id {id}"
                        + " is not a CustomXmlPart part"
                    )
                };
            }
            else
            {
                return null;
            }
        }

        private CustomXmlPart NewCustomXmlPart(string id, string contentType)
        {
            return _doc.MainDocumentPart.AddCustomXmlPart(contentType, id);
        }
    }
}
