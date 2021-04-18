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

        public Annotation Annotation(Range range)
        {
            // If the bookmark exists,
            if (_bookmarkTable.Marked(range))
            {
                // use its ID to retrieve the annotation from RdfStore.
                var b = _bookmarkTable.Get(range);
                if (_rdfStore.Exists(b.Id))
                {
                    var d = _rdfStore.Get(b.Id);
                    return d.ToAnnotation(_bookmarkTable);
                }
            }

            throw new InvalidOperationException(
                "the range is not annotated"
            );
        }

        public bool Annotated(Range range)
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

        public IEnumerable<IAnnotation> Annotations()
        {
            // Get all the bookmarks
            var bs = _bookmarkTable.Bookmarks();
            // Fetch triples from the store
            return (IEnumerable<IAnnotation>)bs
                .Select(b => Annotation(b.Id))
                .Where(b => b != null);
        }

        public Annotation Annotate(
            IRange range,
            Property[] properties,
            IRange[] relations)
        {
            Console.WriteLine($"Annotating range({range.Start}, {range.End})");
            // If the bookmark exists,
            if (_bookmarkTable.Marked(range))
            {
                // assert that there is no annotation for it in the RdfStore.
                var subjectBookmark = _bookmarkTable.Get(range);
                if (_rdfStore.Exists(subjectBookmark.Id))
                {
                    throw new InvalidOperationException(
                        "this range is already annotated"
                    );
                }
            }
            // If the bookmark does not exists,
            else
            {
                _bookmarkTable.Mark(range);
            }

            foreach (var c in relations)
            {
                if (!_bookmarkTable.Marked(c))
                {
                    _bookmarkTable.Mark(c);
                }
            }

            // Create the annotation object.
            var annotation = new Annotation(range, properties, relations);

            // Using the bookmark's ID, ask the RdfStore
            // to store this annotation.
            _rdfStore.Assert(annotation.ToDescription(_bookmarkTable));

            return annotation;
        }

        public void Deannotate(Range range)
        {
            // Fetch the bookmark for this range.
            // If the bookmark does not exist throw an exception.
            // Otherwise, drop the bookmark, deassociate it with the range.
            // Using the bookmark's ID, ask the rdfStore to remove
            // the annotation.

            throw new NotImplementedException();
        }

        public Block Block(IRange range)
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

                bool rStartInT = range.Start >= tStart && range.Start <= tEnd;
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

        public Dictionary<IRange, Bookmark> Bookmarks()
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

            return bookmarks.ToDictionary(b => b.Range as IRange);
        }

        private Annotation? Annotation(int id)
        {
            var description = _rdfStore.Get(id);
            if (description == null)
            {
                return null;
            }

            return description.ToAnnotation(_bookmarkTable);
        }
    }

    static partial class ConversionExtensions
    {
        public static Annotation ToAnnotation(
            this Description d,
            BookmarkTable bookmarkTable)
        {
            var refersTo = Schema.Uri(Schema.RefersTo);
            var relations = d.Properties
                .Where(p => p.Name == refersTo)
                .Select(p =>
                    {
                        int id = int.Parse(p.Value.ToString());
                        return bookmarkTable.Get(id).Range;
                    }
                )
                .ToArray();

            var properties = d.Properties
                .Where(p => p.Name != refersTo)
                .ToArray();

            var subject = bookmarkTable.Get(d.Subject).Range;

            return new Annotation(subject, properties, relations);
        }
    }
}
