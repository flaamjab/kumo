using System;
using DocumentFormat.OpenXml.Packaging;
using Xunit;

namespace Kumo.Tests
{
    public class UriStoreTests
    {
        [Fact]
        public void UriStore_GetWithCleanDocument_IsNull()
        {
            using (var d = WordprocessingDocument.Open(
                Documents.MemoryStream("small"), true))
            {
                var store = new UriStore(d.MainDocumentPart);

                Assert.True(store.Uri.Host == Schema.Namespace.Host);
                Assert.True(store.Uri.AbsolutePath.StartsWith("/document"));
            }
        }

        [Fact]
        public void UriStore_GetReopenGet_UriIsPersisted()
        {
            using (var s = Documents.MemoryStream("small"))
            {
                Uri uri;
                using (var d = WordprocessingDocument.Open(s, true))
                {
                    var store = new UriStore(d.MainDocumentPart);
                    uri = store.Uri;
                }

                using (var d = WordprocessingDocument.Open(s, false))
                {
                    var store = new UriStore(d.MainDocumentPart);
                    Assert.Equal(uri, store.Uri);
                }
            }
        }
    }
}
