using System;
using System.Collections.Generic;
using System.Linq;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace Kumo
{
    class Content
    {
        private Package _parent;

        private W.Document _xml;

        public string Text => _xml.InnerText;

        public Content(Package holder, W.Document content)
        {
            _parent = holder;
            _xml = content;
        }

        public Range Range(int start, int end)
        {
            if (start >= end && start >= 0 && end > 0)
            {
                throw new ArgumentException(
                    "Start parameter must be less than end parameter. "
                    + "both must be non-negative"
                );
            }

            return new Range(_parent, start, end);
        }

        public IEnumerable<Range> Entries(
            string text,
            StringComparison comparison)
        {
            var ranges = new LinkedList<Range>();
            int offset = 0;
            while (offset != -1)
            {
                offset = _xml.InnerText.IndexOf(
                    text, offset,
                    _xml.InnerText.Length - offset,
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

        public Block Block(Range range)
        {
            int fullLength = _xml.InnerText.Length;
            if (range.Start > fullLength || range.End > fullLength)
            {
                throw new IndexOutOfRangeException(
                    "Range bounds must be within text"
                );
            }

            var ts = _xml.Descendants<W.Text>();
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
            var starts = _xml
                .Descendants<W.BookmarkStart>()
                .Where(b =>
                    {
                        return b.Name.Value.StartsWith(
                            Kumo.Bookmark.BASENAME
                        );
                    }
                );

            var ends = _xml.Descendants<W.BookmarkEnd>();
            var bookmarkTagPairs = starts.Join(
                ends,
                s => s.Id,
                e => e.Id,
                (start, end) => (start, end)
            );

            var runs = _xml.Descendants<W.Run>();
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

                    var range = Range(rangeStart, rangeEnd);
                    return new Bookmark(
                        id, this, range, b
                    );
                }
            );

            return bookmarks.ToDictionary(b => b.Range);
        }
    }
}
