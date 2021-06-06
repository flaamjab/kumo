#nullable enable

using System;
using System.Linq;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Packaging;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace Kumo
{
    // Represents an OOXML package.
    class Package : IDisposable
    {
        private WordprocessingDocument _doc;
        private BookmarkTable _bookmarkTable;
        private RdfStore _rdf;
        private UriStore _uri;
        private bool _autoSave;

        public Content Content { get; }

        public Uri Uri => _uri.Value;

        public Package(WordprocessingDocument document, bool autoSave)
        {
            Content = new Content(this, document.MainDocumentPart.Document);
            _doc = document;
            _rdf = new RdfStore(document.MainDocumentPart, autoSave);
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
            _rdf.Save();
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

        public IEnumerable<Range> Relations(Range range)
        {
            if (Known(range))
            {
                var link = Link(range);
                return link.Relations
                    .Select(id => _bookmarkTable.Get(id).Range);
            }
            else
            {
                return new Range[0];
            }
        }

        public IEnumerable<Range> Stars()
        {
            var stars = _bookmarkTable
                .Bookmarks()
                .Where(b => _rdf.Exists(b.Id))
                .Select(b => b.Range);

            return stars;
        }

        public void Link(Range range, IEnumerable<Property> properties)
        {
            int subject;
            if (!_bookmarkTable.Marked(range))
            {
                var b = _bookmarkTable.Mark(range);
                subject = b.Id;
            }
            else
            {
                var b = _bookmarkTable.Get(range);
                subject = b.Id;
            }

            var link = new Link(subject, properties, new int[0]);

            _rdf.Assert(link);
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
            if (_bookmarkTable.Marked(range))
            {
                var b = _bookmarkTable.Get(range);
                if (_rdf.Exists(b.Id))
                {
                    return true;
                }
            }

            return false;
        }

        private Link Link(Range range)
        {
            var b = _bookmarkTable.Get(range);
            if (!_rdf.Exists(b.Id))
            {
                throw new InvalidOperationException(
                    "the range is bookmarked but not present in the RDF store"
                );
            }

            return _rdf.Get(b.Id);
        }
    }
}
