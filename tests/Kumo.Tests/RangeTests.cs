using Xunit;

namespace Kumo.Tests
{
    public class RangeTests
    {
        [Fact]
        public void Text_Range0to11_IsParagraphA()
        {
            var expectedText = "Paragraph A";

            var path = Documents.WithName("small");
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

            var path = Documents.WithName("small");
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
            var expectedText = text.Substring((int)start, (int)(end-start));

            var path = Documents.WithName("small");
            using (var d = Document.Open(path))
            {
                var actualText = d.Range(start, end).Text();
                
                Assert.Equal(expectedText, actualText);
            }
        }
    }
}