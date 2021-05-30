using System;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace Kumo
{
    class Bookmark
    {
        public const string BASENAME = "Kumo_Reference_";

        private Body _holder;

        private W.BookmarkStart _bookmarkStart;
        private W.BookmarkEnd _bookmarkEnd;

        public int Id { get; }

        public Range Range { get; }

        public Bookmark(int id, Body holder, Range range)
        {
            Id = id;
            _holder = holder;
            Range = range;
        }

        public void Link()
        {
            if (_bookmarkStart != null || _bookmarkEnd != null)
            {
                throw new InvalidOperationException(
                    "the bookmark is already applied"
                );
            }

            var b = _holder.Block(Range);
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

        public Block Unlink()
        {
            if (_bookmarkStart == null || _bookmarkEnd == null)
            {
                throw new InvalidOperationException(
                    "the bookmark must be applied before being removed"
                );
            }

            throw new NotImplementedException();
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

    static class StringExtensions
    {
        public static bool Untrimmed(this string s)
        {
            bool leftOverhangs = s.First() == ' ';
            bool rightOverhangs = s.Last() == ' ';

            return leftOverhangs || rightOverhangs;
        }
    }
}
