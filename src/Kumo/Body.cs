using System;
using System.Linq;
using System.Collections.Generic;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace Kumo
{
    class Body
    {
        private W.Document _content;
        private RdfStore _rdfStore;
        private List<Bookmark> _bookmarks = null;

        public Body(W.Document content, RdfStore rdfStore)
        {
            _content = content;
            _rdfStore = rdfStore;
        }

        public Annotation Annotation(Range range)
        {
            throw new NotImplementedException();
        }

        public Annotation Annotate(
            Range range,
            Property[] properties,
            Range[] references)
        {
            throw new NotImplementedException();
        }

        public void Deannotate(Range range)
        {
            throw new NotImplementedException();
        }

        public string Text(Range range)
        {
            var block = Block(range);
            var textValues = block.Nodes.Select(n => n.Text);
            var text = String.Join("", textValues);

            int leftOffset = range.Start - block.Start;
            int rightOffset = block.End - range.End;

            return text.Substring(
                leftOffset,
                text.Length - leftOffset - rightOffset
            );
        }

        public Range[] Paragraphs(Range range)
        {
            throw new NotImplementedException();
        }

        public bool IsValid(Range range)
        {
            throw new NotImplementedException();
        }

        private void ExtractBookmarks()
        {
            throw new NotImplementedException();
        }

        private void CreateBookmark(Range range)
        {

        }

        private NodeBlock Block(Range range)
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
                int tEnd  = offset + t.Text.Length;

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

            return new NodeBlock(nodes.ToArray(), blockStart, blockEnd);
        }
    }
}
