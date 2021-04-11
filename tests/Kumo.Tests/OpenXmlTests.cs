using System.IO;
using System.Linq;
using Xunit;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Kumo.Tests
{
    [Collection("UseIO")]
    public class OpenXmlTests
    {
        [Fact]
        public void AddCustomXmlPart_Readonly_Throws()
        {
            var path = Documents.All().First();
            using (var d = WordprocessingDocument.Open(path, false))
            {
                Assert.Throws<IOException>(() => {
                    d.MainDocumentPart.AddCustomXmlPart("text/txt", "id");
                });
            }
        }

        [Fact]
        public void Parent_GoingUp_RootIsDocument()
        {
            var path = Documents.All().First();
            using (var d = WordprocessingDocument.Open(path, false))
            {
                var ts = d.MainDocumentPart.Document.Descendants<Text>();
                var t = ts.First();

                var p = t.Parent;
                while (p != null)
                {
                    if (p.Parent == null)
                    {
                        Assert.IsNotType<Document>(p);
                    }
                    p = p.Parent;
                }
            }
        }

        [Fact]
        public void Parts_Get_PartsIncludesOneMainDocumentPart()
        {
            var path = Documents.All().First();
            using (var d = WordprocessingDocument.Open(path, false))
            {
                var mainDocumentParts = d.Parts.Where(idPart =>
                    idPart.OpenXmlPart == d.MainDocumentPart
                );

                Assert.Single(mainDocumentParts);
            }
        }

        [Fact]
        public void InnerText_OnDocument_ReturnsAllTextWithinDocument()
        {
            var path = Documents.WithName("small");
            using (var d = WordprocessingDocument.Open(path, false))
            {
                var text = d.MainDocumentPart.Document.InnerText;

                Assert.StartsWith("Lorem ipsum dolor sit amet", text);
                Assert.EndsWith("tristique.", text);
            }
        }
    }
}
