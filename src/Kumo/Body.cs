using System;
using System.Linq;
using System.Collections.Generic;
using W = DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Diagnostics;
using AngleSharp.Text;

namespace Kumo
{
    class Body
    {
        private const string BOOKMARK_NAME_BASE = "Kumo Annotation ";

        private W.Document _content;
        private RdfStore _rdfStore;
        private List<Bookmark> _bookmarks = null;

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

        public Annotation Annotation(Range range)
        {
            throw new NotImplementedException();
        }

        public Annotation Annotate(
            Range range,
            Property[] properties,
            Range[] references)
        {
            Bookmark(range);

            Console.WriteLine(_content.OuterXml);

            throw new NotImplementedException();
        }

        public void Deannotate(Range range)
        {
            throw new NotImplementedException();
        }

        public NodeBlock Block(Range range)
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

        private void Bookmarks()
        {
            throw new NotImplementedException();
        }

        /// <summary>Creates a new bookmark that wraps this range.</summary>
        private Bookmark Bookmark(Range range)
        {
            string bookmarkId = "0";

            var b = Block(range);
            var (leftOffset, rightOffset) = range.Offsets(b);

            var bookmarkStart = new W.BookmarkStart();
            bookmarkStart.Id = bookmarkId;
            bookmarkStart.Name = BOOKMARK_NAME_BASE + bookmarkId;

            var bookmarkEnd = new W.BookmarkEnd();
            bookmarkEnd.Id = bookmarkId;

            var textStart = b.Nodes.First();
            var textEnd = b.Nodes.Last();

            // Range is within single run
            if (textStart.Parent == textEnd.Parent)
            {
                var originalRun = (W.Run)textStart.Parent;
                BookmarkWrap(
                    originalRun,
                    leftOffset, rightOffset,
                    bookmarkStart, bookmarkEnd
                );
            }
            else
            {
                var originalStartRun = (W.Run)textStart.Parent;
                BookmarkLeft(originalStartRun, leftOffset, bookmarkStart);

                var originalEndRun = (W.Run)textEnd.Parent;
                BookmarkRight(originalEndRun, rightOffset, bookmarkEnd);
            }

            return new Bookmark(bookmarkId, bookmarkStart, bookmarkEnd);
        }

        private void BookmarkWrap(
            W.Run run,
            int leftOffset, int rightOffset,
            W.BookmarkStart bookmarkStart,
            W.BookmarkEnd bookmarkEnd)
        {
            if (leftOffset > 0 && rightOffset > 0)
            {
                var (leftRun, restRun) = Split(run, leftOffset);
                var (middleRun, rightRun) = Split(restRun, ^rightOffset);

                run.Parent.ReplaceChild(middleRun, run);
                middleRun.InsertBeforeSelf(leftRun);
                middleRun.InsertBeforeSelf(bookmarkStart);
                middleRun.InsertAfterSelf(rightRun);
                middleRun.InsertAfterSelf(bookmarkEnd);
            }
            else if (leftOffset > 0)
            {
                var runs = BookmarkLeft(run, leftOffset, bookmarkStart);
                runs.Last().InsertAfterSelf(bookmarkEnd);
            }
            else if (rightOffset > 0)
            {
                var runs = BookmarkRight(run, rightOffset, bookmarkEnd);
                runs.First().InsertBeforeSelf(bookmarkStart);
            }
            else
            {
                run.InsertBeforeSelf(bookmarkStart);
                run.InsertAfterSelf(bookmarkEnd);
            }
        }

        private Run[] BookmarkLeft(
            W.Run run,
            int offset,
            W.BookmarkStart bookmarkStart)
        {
            if (offset > 0)
            {
                var (leftRun, rightRun) = Split(run, offset);
                run.Parent.ReplaceChild(leftRun, run);
                leftRun.InsertAfterSelf(rightRun);
                leftRun.InsertAfterSelf(bookmarkStart);

                return new Run[] { leftRun, rightRun };
            }
            else
            {
                run.InsertBeforeSelf(bookmarkStart);
                return new Run[] { run };
            }
        }

        private Run[] BookmarkRight(
            W.Run run,
            int offset,
            W.BookmarkEnd bookmarkEnd)
        {
            if (offset > 0)
            {
                var (leftRun, rightRun) = Split(run, ^offset);
                Debug.Assert(run.Parent.Contains(run));
                run.Parent.ReplaceChild(rightRun, run);
                rightRun.InsertBeforeSelf(leftRun);
                rightRun.InsertBeforeSelf(bookmarkEnd);

                return new Run[] { leftRun, rightRun };
            }
            else
            {
                run.InsertAfterSelf(bookmarkEnd);
                return new Run[] { run };
            }
        }

        private (W.Run, W.Run) Split(W.Run run, Index point)
        {
            var text = run.Descendants<W.Text>().First();

            var leftText = text.Text[..point];
            var leftPart = new W.Text(leftText);
            if (leftText.Untrimmed())
            {
                leftPart.Space = SpaceProcessingModeValues.Preserve;
            }

            var rightText = text.Text[point..];
            var rightPart = new W.Text(rightText);
            if (rightText.Untrimmed())
            {
                rightPart.Space = SpaceProcessingModeValues.Preserve;
            }

            var props = run.Descendants<W.RunProperties>().Last();
            var leftRun = new W.Run(new OpenXmlElement[] {
                props.Clone() as W.RunProperties,
                leftPart
            });

            var rightRun = new W.Run(new OpenXmlElement[] {
                props.Clone() as W.RunProperties,
                rightPart
            });

            return new (leftRun, rightRun);
        }
    }

    static partial class Extensions
    {
        public static bool Untrimmed(this string s)
        {
            bool leftOverhangs = s.First().IsWhiteSpaceCharacter();
            bool rightOverhangs = s.Last().IsWhiteSpaceCharacter();

            return leftOverhangs || rightOverhangs;
        }
    }
}
