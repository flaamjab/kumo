#nullable enable

using System;
using System.IO;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Packaging;

namespace Kumo.OOXML
{
    // Represents an OOXML package.
    class Package : IPackage
    {
        private WordprocessingDocument _doc;
        private BookmarkStore _bookmarkStore;
        private Lazy<RdfStore> _rdf;
        private UriStore _uri;
        private bool _autoSave;
        private bool _editable;

        private Content _content;

        private RdfStore Rdf => _rdf.Value;

        public IContent Content => _content;

        public Uri Uri => _uri.Value;

        public Package(
            WordprocessingDocument document,
            bool editable,
            bool autoSave)
        {
            _content = new Content(this, document.MainDocumentPart.Document);
            _doc = document;

            _uri = new UriStore(document.MainDocumentPart);

            _rdf = new Lazy<RdfStore>(() =>
                {
                    var rdf = new RdfStore(Uri);
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

            _bookmarkStore = new BookmarkStore(_content);

            _autoSave = autoSave;
            _editable = editable;
        }

        public void Dispose()
        {
            if (_editable)
            {
                Save();
            }
            Rdf.Dispose();
            _doc.Dispose();
        }

        public void Save()
        {
            var part = CustomXmlPart(RdfStore.ID);
            if (part is null)
            {
                part = NewCustomXmlPart(RdfStore.ID, "text/plain");
            }

            // Make sure RDf is loaded before attempting to save.
            // Otherwise the stream will be opened a second time
            // while being opened in update mode, which is an error.
            var rdf = Rdf;
            using (var s = part.GetStream(FileMode.Open, FileAccess.Write))
            {
                s.SetLength(0);
                rdf.Save(s);
            }

            _doc.Save();
        }

        public IPackage Clone()
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
            var stars = _bookmarkStore.MarkedRanges();

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

            if (!_bookmarkStore.Marked(range))
            {
                _bookmarkStore.Mark(range);
            }

            var link = new Star(range.Uri, properties);

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
                var link = new Star(range.Uri, properties);
                g.Retract(link);

                if (!g.Exists(range.Uri) && _bookmarkStore.Marked(range))
                {
                    _bookmarkStore.Unmark(range);   
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
            if (_bookmarkStore.Marked(range))
            {
                if (graph.Exists(range.Uri))
                {
                    return true;
                }
            }

            return false;
        }

        private Star Link(Range range)
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
