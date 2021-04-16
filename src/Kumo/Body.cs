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
        private Dictionary<Range, Bookmark>? _bookmarks = null;
        private RdfStore _rdfStore;

        public Body(W.Document content, RdfStore rdfStore)
        {
            _content = content;
            _rdfStore = rdfStore;
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

        public Annotation? Annotation(Range range)
        {
            // Fetch the bookmark for this range
            // If the bookmark exists, use its ID to retrieve
            // the annotation from RdfStore.
            // Otherwise return null.

            throw new NotImplementedException();
        }

        public IEnumerable<IAnnotation> Annotations()
        {
            // Get all the bookmarks
            var bs = Bookmarks();
            // Fetch triples from the store
            return bs.Select(b => Annotation(b.ID));
        }

        public Annotation Annotate(
            Range range,
            Property[] properties,
            Range[] references)
        {
            // Fetch the bookmark for this range
            // If the bookmark exists, assert that there is not
            // annotation for it in the RdfStore.
            // If not, create a new bookmark for this range,
            // apply it, associate it with this range.
            // Using the bookmark's ID, ask the RdfStore
            // to store this annotation.

            throw new NotImplementedException();
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

        private Annotation Annotation(int id)
        {
            var triples = _rdfStore.Get(id.ToString());
            throw new NotImplementedException();
        }

        private IEnumerable<Bookmark> Bookmarks()
        {
            var starts = _content
                .Descendants<W.BookmarkStart>()
                .Where(b =>
                    {
                        Console.WriteLine(b.Name.Value);
                        return b.Name.Value.StartsWith(Bookmark.BASENAME);
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

                    return new Bookmark(
                        id, this,
                        new Range(this, rangeStart, rangeEnd)
                    );
                }
            );

            return bookmarks;
        }
    }
}
