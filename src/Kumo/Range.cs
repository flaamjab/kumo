#nullable enable

using System;
using System.Linq;
using System.Collections.Generic;
using Word = DocumentFormat.OpenXml.Wordprocessing;

namespace Kumo
{
    class Range : IRange, IEquatable<Range>
    {
        private int _start;
        private int _end;
        private AnnotationStore _annotationStore;
        private Word.Document _content;
        private Span? _span = null;

        public IAnnotation? Annotation { get; } = null;

        public Range(
            int start, int end,
            Word.Document content,
            AnnotationStore annotationStore,
            Annotation? annotation = null)
        {
            _start = start;
            _end = end;
            _content = content;
            _annotationStore = annotationStore;
            Annotation = annotation;
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

        public IEnumerable<IRange> Paragraphs()
        {
            throw new NotImplementedException();
        }

        public void Annotate(IAnnotator annotator)
        {
            throw new NotImplementedException();
        }

        public IAnnotation Annotate(Property property)
        {
            throw new NotImplementedException();
        }

        public IAnnotation Annotate(IEnumerable<Property> properties)
        {
            throw new NotImplementedException();
        }

        public void Reannotate(object obj)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Annotation);
        }

        public bool Equals(Range? other)
        {
            if (other is null)
            {
                return false;
            }

            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            if (GetType() != other.GetType())
            {
                return false;
            }

            return (_start, _end) == (other._start, other._end);
        }

        public override int GetHashCode() => (_start, _end).GetHashCode();

        public static bool operator==(Range lhs, Range rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                return false;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator!=(
            Range lhs, Range rhs
        ) => !(lhs == rhs);

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
