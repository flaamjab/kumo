using Xunit;

namespace Kumo.Tests
{
    public class DocumentTests
    {
        [Fact]
        public void Annotations_OnDocumentWithNone_ReturnsEmptyArray()
        {
            string path = Documents.WithName("bookmarks");
            using (var d = Document.Open(path))
            {
                Assert.Empty(d.Annotations());
            }
        }
    }
}
