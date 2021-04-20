using Xunit;

namespace Kumo.Tests
{
    public class DocumentTests
    {
        [Fact]
        public void Annotations_OnDocumentWithNone_ReturnsEmptyArray()
        {
            using (var d = Documents.Open("bookmarked"))
            {
                Assert.Empty(d.Stars());
            }
        }
    }
}
