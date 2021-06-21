#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace Kumo.OOXML
{
    class BookmarkStore
    {
        private Lazy<Dictionary<Range, Bookmark>> _table;
        private Lazy<SortedSet<int>> _availableIds;
        private Content _content;

        private Dictionary<Range, Bookmark> Table => _table.Value;

        private SortedSet<int> AvailableIds => _availableIds.Value;

        public BookmarkStore(Content content)
        {
            _content = content;
            _table = new Lazy<Dictionary<Range, Bookmark>>(() =>
                _content.Bookmarks()
            );

            _availableIds = new Lazy<SortedSet<int>>(() =>
                {
                    var ids = new SortedSet<int>(
                        Table.Values.Select(b => b.Id)
                    );

                    var availableIds = new SortedSet<int>(Enumerable.Range(1, ids.Max));
                    availableIds.SymmetricExceptWith(ids);
                    if (ids.Max < int.MaxValue)
                    {
                        availableIds.Add(ids.Max + 1);
                    }

                    return availableIds;
                }
            );
        }

        public IEnumerable<Range> MarkedRanges()
        {
            return Table.Keys;
        }

        public Bookmark Mark(Range range)
        {
            if (Marked(range))
            {
                throw new InvalidOperationException(
                    "The table already contains a bookmark for this range"
                );
            }

            var table = Table;
            var id = AcquireId();

            var b = new Bookmark(id, _content, range);
            b.Insert();

            table.Add(range, b);

            return b;
        }

        public bool Marked(Range range)
        {
            return Table.ContainsKey(range);
        }

        public void Unmark(Range range)
        {
            if (!Marked(range))
            {
                throw new InvalidOperationException(
                    "The range is not marked"
                );
            }

            var b = Table[range];
            b.Remove();
            ReleaseId(b.Id);
            Table.Remove(range);
        }

        private int AcquireId()
        {
            var ids = AvailableIds;
            if (ids.Count == 0)
            {
                throw new InvalidOperationException(
                    "There are no bookmark IDs available"
                );
            }

            var id = ids.Min;
            ids.Remove(id);
            if (ids.Count == 0 && id < int.MaxValue)
            {
                ids.Add(id + 1);
            }
            else if (id == int.MaxValue)
            {
                throw new InvalidOperationException(
                    "Bookmark IDs have been exhausted"
                );
            }

            return id;
        }

        private void ReleaseId(int id)
        {
            var ids = AvailableIds;
            ids.Add(id);
        }
    }
}
