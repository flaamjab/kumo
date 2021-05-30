#nullable enable

using System;
using System.Linq;
using System.Collections.Generic;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace Kumo
{
    class Body
    {
        private W.Document _content;
        private BookmarkTable _bookmarkTable;
        private RdfStore _rdfStore;

        public Body(W.Document content, RdfStore rdfStore)
        {
            _content = content;
            _rdfStore = rdfStore;
            _bookmarkTable = new BookmarkTable(this);
        }

        public string Text => _content.InnerText;

        public Range Range(int start, int end)
        {
            if (start >= end && start >= 0 && end > 0)
            {
                throw new ArgumentException(
                    "start parameter must be less than end parameter. "
                    + "both must be non-negative"
                );
            }

            return new Range(this, start, end);
        }

        public IEnumerable<Range> Ranges(
            string text,
            StringComparison comparison)
        {
            var ranges = new LinkedList<Range>();
            int offset = 0;
            while (offset != -1)
            {
                offset = _content.InnerText.IndexOf(
                    text, offset,
                    _content.InnerText.Length - offset,
                    comparison
                );

                if (offset != -1)
                {
                    int start = offset;
                    int end = offset + text.Length;
                    ranges.AddLast(Range(start, end));
                    offset = end;
                }   
            }

            return ranges;
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
                .Where(b => _rdfStore.Exists(b.Id))
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

            _rdfStore.Assert(link);
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

        public Block Block(Range range)
        {
            int fullLength = _content.InnerText.Length;
            if (range.Start > fullLength || range.End > fullLength)
            {
                throw new IndexOutOfRangeException(
                    "Range bounds must be within text"
                );
            }

            var ts = _content.Descendants<W.Text>();
            int offset = 0;
            int blockStart = 0;
            int blockEnd = 0;
            var nodes = new LinkedList<W.Text>();

            foreach (var t in ts)
            {
                int tStart = offset;
                int tEnd = offset + t.Text.Length;

                bool rStartInT = range.Start >= tStart && range.Start < tEnd;
                bool rStartBeforeT = range.Start <= tStart;

                if (rStartInT || rStartBeforeT)
                {
                    if (rStartInT)
                    {
                        blockStart = tStart;
                    }

                    nodes.AddLast(t);
                }

                if (tStart <= range.End && range.End <= tEnd)
                {
                    blockEnd = tEnd;
                    break;
                }

                offset = tEnd;
            }

            return new Block(nodes.ToArray(), blockStart, blockEnd);
        }

        public Dictionary<Range, Bookmark> Bookmarks()
        {
            var starts = _content
                .Descendants<W.BookmarkStart>()
                .Where(b =>
                    {
                        return b.Name.Value.StartsWith(
                            Kumo.Bookmark.BASENAME
                        );
                    }
                );

            var ends = _content.Descendants<W.BookmarkEnd>();
            var bookmarkTagPairs = starts.Join(
                ends,
                s => s.Id,
                e => e.Id,
                (start, end) => (start, end)
            );

            var runs = _content.Descendants<W.Run>();
            var offsets = new Dictionary<W.Run, int>();
            int offset = 0;
            foreach (var r in runs)
            {
                offsets[r] = offset;
                offset += r.InnerText.Length;
            }

            var bookmarks = bookmarkTagPairs.Select(b =>
                {
                    int id = int.Parse(b.start.Id.Value);

                    var startRun = (W.Run)b.start.NextSibling();
                    int rangeStart = offsets[startRun];

                    var endRun = (W.Run)b.end.PreviousSibling();
                    int rangeEnd = offsets[endRun] + endRun.InnerText.Length;

                    var range = new Range(this, rangeStart, rangeEnd);
                    return new Bookmark(id, this, range);
                }
            );

            return bookmarks.ToDictionary(b => b.Range as Range);
        }

        private bool Known(Range range)
        {
            if (_bookmarkTable.Marked(range))
            {
                var b = _bookmarkTable.Get(range);
                if (_rdfStore.Exists(b.Id))
                {
                    return true;
                }
            }

            return false;
        }

        private Link Link(Range range)
        {
            var b = _bookmarkTable.Get(range);
            if (!_rdfStore.Exists(b.Id))
            {
                throw new InvalidOperationException(
                    "the range is bookmarked but not present in the RDF store"
                );
            }

            return _rdfStore.Get(b.Id);
        }
    }
}
