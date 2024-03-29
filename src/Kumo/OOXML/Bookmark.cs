using System;
using System.Linq;
using DocumentFormat.OpenXml;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace Kumo.OOXML
{
    class Bookmark
    {
        public const string BASENAME = "Kumo_Reference_";

        private Content _parent;

        private W.BookmarkStart _bookmarkStart;
        private W.BookmarkEnd _bookmarkEnd;

        public int Id { get; }

        public Range Range { get; }

        public Bookmark(int id, Content holder, Range range) :
            this(id, holder, range, (null, null))
        { }

        public Bookmark(
            int id,
            Content holder,
            Range range,
            (W.BookmarkStart start, W.BookmarkEnd end) bounds)
        {
            Id = id;
            _parent = holder;
            Range = range;
            _bookmarkStart = bounds.start;
            _bookmarkEnd = bounds.end;
        }

        public void Insert()
        {
            if (_bookmarkStart != null || _bookmarkEnd != null)
            {
                throw new InvalidOperationException(
                    "the bookmark is already applied"
                );
            }

            var b = _parent.Block(Range);
            var (leftOffset, rightOffset) = Range.Offsets(b);

            _bookmarkStart = new W.BookmarkStart();
            _bookmarkStart.Id = Id.ToString();
            _bookmarkStart.Name = BASENAME + Id;

            _bookmarkEnd = new W.BookmarkEnd();
            _bookmarkEnd.Id = Id.ToString();

            var textStart = b.Nodes.First();
            var textEnd = b.Nodes.Last();

            // Range is within single run
            if (textStart.Parent == textEnd.Parent)
            {
                var originalRun = (W.Run)textStart.Parent;
                WrapMiddle(originalRun, leftOffset, rightOffset);
            }
            else
            {
                var originalStartRun = (W.Run)textStart.Parent;
                WrapLeft(originalStartRun, leftOffset);

                var originalEndRun = (W.Run)textEnd.Parent;
                WrapRight(originalEndRun, rightOffset);
            }
        }

        public void Remove()
        {
            if (_bookmarkStart == null || _bookmarkEnd == null)
            {
                throw new InvalidOperationException(
                    "the bookmark must be applied before being removed"
                );
            }

            _bookmarkStart.Remove();
            _bookmarkEnd.Remove();
        }

        private void WrapMiddle(W.Run run, int leftOffset, int rightOffset)
        {
            if (leftOffset > 0 && rightOffset > 0)
            {
                var (leftRun, restRun) = run.Split(leftOffset);
                var (middleRun, rightRun) = restRun.Split(^rightOffset);

                run.Parent.ReplaceChild(middleRun, run);
                middleRun.InsertBeforeSelf(leftRun);
                middleRun.InsertBeforeSelf(_bookmarkStart);
                middleRun.InsertAfterSelf(rightRun);
                middleRun.InsertAfterSelf(_bookmarkEnd);
            }
            else if (leftOffset > 0)
            {
                var runs = WrapLeft(run, leftOffset);
                runs.Last().InsertAfterSelf(_bookmarkEnd);
            }
            else if (rightOffset > 0)
            {
                var runs = WrapRight(run, rightOffset);
                runs.First().InsertBeforeSelf(_bookmarkStart);
            }
            else
            {
                run.InsertBeforeSelf(_bookmarkStart);
                run.InsertAfterSelf(_bookmarkEnd);
            }
        }

        private W.Run[] WrapLeft(W.Run run, int offset)
        {
            if (offset > 0)
            {
                var (leftRun, rightRun) = run.Split(offset);
                run.Parent.ReplaceChild(leftRun, run);
                leftRun.InsertAfterSelf(rightRun);
                leftRun.InsertAfterSelf(_bookmarkStart);

                return new W.Run[] { leftRun, rightRun };
            }
            else
            {
                run.InsertBeforeSelf(_bookmarkStart);
                return new W.Run[] { run };
            }
        }

        private W.Run[] WrapRight(W.Run run, int offset)
        {
            if (offset > 0)
            {
                var (leftRun, rightRun) = run.Split(^offset);
                run.Parent.ReplaceChild(rightRun, run);
                rightRun.InsertBeforeSelf(leftRun);
                rightRun.InsertBeforeSelf(_bookmarkEnd);

                return new W.Run[] { leftRun, rightRun };
            }
            else
            {
                run.InsertAfterSelf(_bookmarkEnd);
                return new W.Run[] { run };
            }
        }
    }

    static class RunExtensions
    {
        public static (W.Run, W.Run) Split(this W.Run run, Index point)
        {
            var text = run.Descendants<W.Text>().First();

            var leftText = text.Text[..point];
            var leftPart = new W.Text(leftText);
            if (!leftText.Trimmed())
            {
                leftPart.Space = SpaceProcessingModeValues.Preserve;
            }

            var rightText = text.Text[point..];
            var rightPart = new W.Text(rightText);
            if (!rightText.Trimmed())
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

            return (leftRun, rightRun);
        }
    }

    static class StringExtensions
    {
        public static bool Trimmed(this string s)
        {
            bool leftOverhangs = s.First() == ' ';
            bool rightOverhangs = s.Last() == ' ';

            return leftOverhangs || rightOverhangs;
        }
    }
}
