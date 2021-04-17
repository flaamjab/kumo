#nullable enable

using System.Collections.Generic;
using System.Linq;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace Kumo
{
    class BookmarkTable
    {
        private Dictionary<Range, Bookmark> _bookmarks;
        private W.Document _content;
        private Body _holder;

        public Bookmark this[Range index]
        {
            get
            {
                var bs = Bookmarks();
                return bs[index];
            }
            set
            {
                var bs = Bookmarks();
                bs[index] = value;
                value.Apply();
            }
        }

        public Bookmark? this[int index]
        {
            get
            {
                var bs = Bookmarks();
                return bs.Values.FirstOrDefault(b => b.Id == index);
            }
        }

        public BookmarkTable(Body holder, W.Document content)
        {
            _holder = holder;
            _content = content;
            _bookmarks = new Dictionary<Range, Bookmark>();
        }

        public bool Remove(Range range)
        {
            var b = _bookmarks[range];
            if (b != null)
            {
                b.Remove();
                _bookmarks.Remove(range);
                return true;
            }

            return false;
        }

        public Dictionary<Range, Bookmark> Bookmarks()
        {
            if (_bookmarks != null)
            {
                return _bookmarks;
            }

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

                    return new Bookmark(
                        id, _holder,
                        new Range(_holder, rangeStart, rangeEnd)
                    );
                }
            );

            _bookmarks = bookmarks.ToDictionary(b => b.Range);
            return _bookmarks;
        }
    }
}
