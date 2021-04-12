using System;
using System.Linq;
using System.Collections.Generic;
using Word = DocumentFormat.OpenXml.Wordprocessing;

namespace Kumo
{
    public class Range : IRange
    {
        private int _start;
        private int _end;
        private Annotations _annotations;
        private Word.Document _content;
        private Span _span = null;

        internal Range(
            int start, int end,
            Word.Document content,
            Annotations annotations)
        {
            _start = start;
            _end = end;
            _content = content;
            _annotations = annotations;
        }

        public string Text()
        {
            var s = Span();
            var text = string.Join("", s.Nodes.Select(n => n.Text));

            return text.Substring(
                s.LeftOffset,
                text.Length - s.LeftOffset - s.RightOffset
            );
        }

        public IEnumerable<Range> Paragraphs()
        {
            throw new NotImplementedException();
        }

        public void Annotate(Annotation annotation)
        {
            throw new NotImplementedException();
        }

        public void Annotate(IEnumerable<Annotation> annotations)
        {
            throw new NotImplementedException();
        }

        public void Annotate(IAnnotator annotator)
        {
            throw new NotImplementedException();
        }

        /** <summary>
                <para>Checks whether this <c>Range</c> is valid.</para>
                
                <para>A range is considered valid if it spans a sequence
                of neighboring runs. Runs are considered neighbors if they
                are within the same paragraph or directly neighboring paragraphs
                and there are no other runs between them.</para>

                <para>For example, the following ranges are not valid:</para>
                <list>
                    <item>
                        - A range that starts within a main body paragraph
                        but ends inside a table paragraph.
                    </item>
                    <item>
                        - A range that spans multiple table cells.
                    </item>
                </list>

                <para>Only valid ranges can be annotated.</para>
            </summary>
        */
        public bool IsValid()
        {
            throw new NotImplementedException();
        }

        private Span Span()
        {
            if (_span != null)
            {
                return _span;
            }

            var nodes = new LinkedList<Word.Text>();
            int offset = 0;
            int leftOffset = 0;
            int rightOffset = 0;
            var allTextNodes = _content.Descendants<Word.Text>();

            foreach (var t in allTextNodes)
            {
                int tStart = offset;
                int tEnd = offset + t.Text.Length;

                bool tFirstInRange = _start >= tStart && _start <= tEnd;
                bool tAfterFirstInRange = _start <= tStart;
                if (tFirstInRange || tAfterFirstInRange)
                {
                    if (tFirstInRange)
                    {
                        leftOffset = _start - tStart;
                    }

                    nodes.AddLast(t);

                    bool tLastInRange = _end <= tEnd;
                    if (tLastInRange)
                    {
                        rightOffset = tEnd - _end;
                        break;
                    }
                }

                offset = tEnd;
            }

            return new Span(
                nodes.ToArray(),
                leftOffset,
                rightOffset
            );
        }
    }
}
