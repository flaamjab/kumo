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

            var path = Documents.Named("small");
            using (var d = Document.Open(path))
            {
                var actualText = d.Range(0, 11).Text();

                Assert.Equal(expectedText, actualText);
            }
        }

        [Fact]
        public void Text_Range10to12_IsAP()
        {
            var expectedText = "AP";

            var path = Documents.Named("small");
            using (var d = Document.Open(path))
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

            var path = Documents.Named("small");
            using (var d = Document.Open(path))
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

                var a = r.Annotate(
                    new Property(
                        "http://example.org/rel",
                        "http://example.org/val"
                    )
                );

                Assert.Single(d.Annotations(), a);
            }
        }

        [Fact]
        public void Annotations_DocumentWithSingleAnnotation_OneAnnotation()
        {
            using (var d = Documents.Open("annotated"))
            {
                var annotations = d.Annotations();

                Assert.Single(annotations);
            }
        }

        [Fact]
        public void Annotate_Range_OneAnnotation()
        {
            using (var d = Documents.Open("medium"))
            {
                var r = d.Range(0, 5);
                r.Annotate(
                    new Property(
                        "http://example.org/rel",
                        "http://example.org/val"
                    )
                );

                Assert.True(r.Annotated());
            }
        }

        [Fact]
        public void Annotate_TwoNonIntersectingRanges_EachHasAnnotation()
        {
            using (var d = Documents.Open("medium"))
            {
                Assert.Empty(d.Annotations());

                var rA = d.Range(0, 100);
                var rB = d.Range(100, 200);

                var property = new Property(
                    "http://example.org/predicate",
                    "http://example.org/value"
                );

                rA.Annotate(property);
                rB.Annotate(property);

                Assert.Single(rA.Annotation().Properties);
                Assert.Single(rB.Annotation().Properties);
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

                rA.Annotate(property);
                rB.Annotate(property);

                Assert.Single(rA.Annotation().Properties);
                Assert.Single(rB.Annotation().Properties);

                d.Save();
            }
        }
    }
}
