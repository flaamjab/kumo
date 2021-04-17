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
            _bookmarkTable = new BookmarkTable(this, content);
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
            var bs = _bookmarkTable.Bookmarks();
            // Fetch triples from the store
            return (IEnumerable<IAnnotation>)bs.Values
                .Select(b => Annotation(b.Id))
                .Where(b => b != null);
        }

        public Annotation Annotate(
            Range range,
            Property[] properties,
            Range[] crossrefs)
        {
            // Fetch the bookmark for this range
            var subjectBookmark = _bookmarkTable[range];
            // If the bookmark exists,
            if (subjectBookmark != null)
            {
                // assert that there is no annotation for it in the RdfStore.
                _rdfStore.Get(subjectBookmark.Id);
            }
            // If the bookmark does not exists,
            else
            {
                // create a new bookmark for this range,
                var newB = new Bookmark(0, this, range);

                // apply it and associate it with this range.
                newB.Apply();
                _bookmarkTable[range] = newB;
            }

            var crossrefBs = crossrefs.Select(cr =>
                {
                    var b = _bookmarkTable[cr];
                    if (b != null)
                    {
                        return b;
                    }
                    else
                    {
                        var newB = new Bookmark(1, this, range);
                        newB.Apply();
                        _bookmarkTable[range] = newB;

                        return newB;
                    }
                }
            );

            // Create the annotation object.
            var a = new Annotation(
                subjectBookmark,
                properties,
                crossrefBs.ToArray()
            );

            // Using the bookmark's ID, ask the RdfStore
            // to store this annotation.
            _rdfStore.Assert(a.ToStar());

            return a;
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

        private Annotation? Annotation(int id)
        {
            var star = _rdfStore.Get(id);
            if (star == null)
            {
                return null;
            }

            var refersTo = Schema.Uri(Schema.RefersTo);
            var crossrefs = star.Properties
                .Where(p => p.Name == refersTo)
                .Select(p =>
                    {
                       int id = int.Parse(p.Value.ToString());
                       return _bookmarkTable[id];
                    }
                )
                .ToArray();

            var properties = star.Properties
                .Where(p => p.Name != refersTo)
                .ToArray();

            var subject = _bookmarkTable[id];
            return new Annotation(subject, properties, crossrefs);
        }
    }
}
