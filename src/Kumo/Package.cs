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
        private BookmarkStore _bookmarkTable;
        private Lazy<RdfStore> _rdf;
        private UriStore _uri;
        private bool _autoSave;
        private bool _editable;

        private RdfStore Rdf => _rdf.Value;

        public Content Content { get; }

        public Uri Uri => _uri.Value;

        public Package(
            WordprocessingDocument document,
            bool editable,
            bool autoSave)
        {
            Content = new Content(this, document.MainDocumentPart.Document);
            _doc = document;

            _rdf = new Lazy<RdfStore>(() =>
                {
                    var rdf = new RdfStore();
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
            _bookmarkTable = new BookmarkStore(this);

            _autoSave = autoSave;
            _editable = editable;
        }

        public void Dispose()
        {
            if (_editable)
            {
                Save();
            }
            _doc.Dispose();
            Rdf.Dispose();
        }

        public void Save()
        {
            var part = CustomXmlPart(RdfStore.ID);
            if (part is null)
            {
                part = NewCustomXmlPart(RdfStore.ID, "text/plain");
            }

            using (var s = part.GetStream(FileMode.Open, FileAccess.Write))
            {
                s.SetLength(0);
                Rdf.Save(s);
            }

            _doc.Save();
        }

        public Package Clone()
        {
            var clone = (WordprocessingDocument)_doc.Clone();
            return new Package(clone, _editable, _autoSave);
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
            if (!_editable)
            {
                throw new InvalidOperationException(
                    "Cannot edit a document opened in read-only mode"
                );
            }

            if (!_bookmarkTable.Marked(range))
            {
                _bookmarkTable.Mark(range);
            }

            var link = new Link(range.Uri, properties);

            var g = Rdf.RangeGraph;
            g.Assert(link);

            if (_autoSave)
            {
                Save();   
            }
        }

        public void Unlink(Range range, IEnumerable<Property> properties)
        {
            var g = Rdf.RangeGraph;
            // Ask the RdfStore if the range is annotated.
            if (g.Exists(range.Uri))
            {
                // Proceed to remove properties if it is.
                var link = new Link(range.Uri, properties);
                g.Retract(link);

                if (!g.Exists(range.Uri) && _bookmarkTable.Marked(range))
                {
                    _bookmarkTable.Unmark(range);   
                }

                if (_autoSave)
                {
                    Save();
                }
            }
            else 
            {
                // otherwise throw.
                throw new InvalidOperationException(
                    $"The range {range.Uri} has no properties"
                );
            }
        }

        public Stream RdfStream()
        {
            var stream = new MemoryStream();
            Rdf.Save(stream);

            return new MemoryStream(stream.ToArray(), false);
        }

        private bool Known(Range range)
        {
            var graph = Rdf.RangeGraph;
            if (_bookmarkTable.Marked(range))
            {
                if (graph.Exists(range.Uri))
                {
                    return true;
                }
            }

            return false;
        }

        private Link Link(Range range)
        {
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
