using System;
using System.Linq;
using Xunit;

namespace Kumo.Tests
{
    [Collection("IO")]
    public class RangeTests
    {
        [Fact]
        public void Text_Range0to11_IsParagraphA()
        {
            var expectedText = "Paragraph A";

            using (var d = Documents.Open("small"))
            {
                var actualText = d.Range(0, 11).Text();

                Assert.Equal(expectedText, actualText);
            }
        }

        [Fact]
        public void Text_Range10to12_IsAP()
        {
            var expectedText = "AP";

            using (var d = Documents.Open("small"))
            {
                var actualText = d.Range(10, 12).Text();

                Assert.Equal(expectedText, actualText);
            }
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0, 5)]
        [InlineData(0, 11)]
        [InlineData(2, 15)]
        [InlineData(2, 24)]
        [InlineData(25, 33)]
        public void Text_Range_TextIsCorrect(int start, int end)
        {
            var text = "Paragraph AParagraph BParagraph C";
            var expectedText = text.Substring((int)start, (int)(end - start));

            using (var d = Documents.Open("small"))
            {
                var actualText = d.Range(start, end).Text();

                Assert.Equal(expectedText, actualText);
            }
        }

        [Theory]
        [InlineData(0, 4)]
        [InlineData(1, 5)]
        [InlineData(9, 15)]
        [InlineData(1, 11)]
        [InlineData(0, 11)]
        public void Annotate_SingleAnnotation_DocumentHasNewAnnotation(
            int start, int end)
        {
            using (var d = Documents.Open("small"))
            {
                var r = d.Range(start, end);

                r.Attach(
                    new Property(
                        "http://example.org/rel",
                        "http://example.org/val"
                    )
                );

                Assert.Single(d.Stars(), r);
            }
        }

        [Fact]
        public void Annotations_DocumentWithSingleAnnotation_OneAnnotation()
        {
            using (var d = Documents.Open("annotated"))
            {
                var annotations = d.Stars();

                Assert.Single(annotations);
            }
        }

        [Fact]
        public void Annotate_TwoNonIntersectingRanges_EachHasAnnotation()
        {
            using (var d = Documents.Open("medium"))
            {
                Assert.Empty(d.Stars());

                var rA = d.Range(0, 100);
                var rB = d.Range(100, 200);

                var property = new Property(
                    "http://example.org/predicate",
                    "http://example.org/value"
                );

                rA.Attach(property);
                rB.Attach(property);

                Assert.Single(rA.Properties);
                Assert.Single(rB.Properties);
            }
        }

        [Fact]
        public void Annotate_RangeWithinRangeOuterInner_EachHasAnnotation()
        {
            using (var d = Documents.Open("medium"))
            {
                var rA = d.Range(0, 10);
                var rB = d.Range(3, 7);

                var property = new Property(
                    "https://example.org/rel",
                    "https://example.org/val"
                );

                rA.Attach(property);
                rB.Attach(property);

                Assert.Single(rA.Properties);
                Assert.Single(rB.Properties);
            }
        }

        [Fact]
        public void Annotate_MultiParagraphRange_RangeHasAnnotation()
        {
            using (var d = Documents.Open("small"))
            {
                var r = d.Range(0, 15);

                var property = new Property(
                    "http://example.org/rel",
                    "http://example.org/val"
                );

                r.Attach(property);

                Assert.Single(r.Properties, property);
            }
        }
    }
}
