using System;
using System.Collections.Generic;

namespace Kumo
{
    interface IContent
    {
        public string Text();

        public string Text(int start, int end)
        {
            var t = Text();
            return t.Substring(start, end - start);
        }

        public Range Range(int start, int end);

        public IEnumerable<Range> Paragraphs();

        public IEnumerable<Range> Entries(
            string text,
            StringComparison comparison);
    }
}

