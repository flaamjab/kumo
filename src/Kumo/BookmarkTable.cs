#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace Kumo
{
    class BookmarkTable
    {
        private Dictionary<Range, Bookmark>? _bookmarks;
        private SortedSet<int>? _availableIds;
        private Package _holder;

        public BookmarkTable(Package holder)
        {
            _holder = holder;
            _bookmarks = null;
            _availableIds = null;
        }

        public IEnumerable<Bookmark> Bookmarks()
        {
            var table = Table();
            return table.Values;
        }

        public Bookmark Get(Range range)
        {
            if (!Marked(range))
            {
                throw new InvalidOperationException(
                    "the range is not marked"
                );
            }

            var table = Table();
            return table[range];
        }

        public Bookmark Get(int id)
        {
            var table = Table().Values;
            return table.First(b => b.Id == id);
        }

        public Bookmark Mark(Range range)
        {
            if (Marked(range))
            {
                throw new InvalidOperationException(
                    "the table already contains a bookmark for this range"
                );
            }

            var table = Table();
            var id = AcquireId();

            var b = new Bookmark(id, _holder.Content, range);
            b.Insert();

            table.Add(range, b);

            return b;
        }

        public bool Marked(Range range)
        {
            var table = Table();
            return table.ContainsKey(range);
        }

        public void Unmark(Range range)
        {
            if (!Marked(range))
            {
                throw new InvalidOperationException(
                    "the range is not marked"
                );
            }

            var table = Table();
            var b = table[range];
            b.Remove();
            ReleaseId(b.Id);
            table.Remove(range);
        }

        private int AcquireId()
        {
            var ids = AvailableIds();
            if (ids.Count == 0)
            {
                throw new InvalidOperationException(
                    "there are no bookmark IDs available"
                );
            }

            var id = ids.Min;
            ids.Remove(id);
            if (ids.Count == 0 && id < int.MaxValue)
            {
                ids.Add(id + 1);
            }
            else
            {
                throw new InvalidOperationException(
                    "bookmark IDs have been exhausted"
                );
            }

            return id;
        }

        private void ReleaseId(int id)
        {
            var ids = AvailableIds();
            ids.Add(id);
        }

        private SortedSet<int> AvailableIds()
        {
            if (_availableIds != null)
            {
                return _availableIds;
            }

            var table = Table();
            var ids = new SortedSet<int>(
                table.Values.Select(b => b.Id)
            );

            _availableIds = new SortedSet<int>(Enumerable.Range(1, ids.Max));
            _availableIds.SymmetricExceptWith(ids);
            if (ids.Max < int.MaxValue)
            {
                _availableIds.Add(ids.Max + 1);
            }

            return _availableIds;
        }

        private Dictionary<Range, Bookmark> Table()
        {
            if (_bookmarks != null)
            {
                return _bookmarks;
            }

            _bookmarks = _holder.Content.Bookmarks();
            return _bookmarks;
        }
    }
}
