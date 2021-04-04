using System;
using System.Linq;
using Xunit;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;

namespace Kumo.Tests
{
    public class OpenXmlTests
    {
        [Theory]
        [InlineData("../../../../data/small.docx")]
        public void OpenReadonly(string path)
        {
            using (var d = WordprocessingDocument.Open(path, false))
            {
                var ds = d.MainDocumentPart.Document.Descendants<Text>();
                foreach (var t in ds)
                {
                    Console.WriteLine(t.Text);
                }
            }
        }

        [Theory]
        [InlineData("../../../../data/small.docx")]
        public void RootIsDocument(string path)
        {
            using (var d = WordprocessingDocument.Open(path, false))
            {
                var ts = d.MainDocumentPart.Document.Descendants<Text>();
                var t = ts.First();

                var p = t.Parent;
                while (p != null)
                {
                    Console.WriteLine(p.LocalName);
                    if (p.Parent == null)
                    {
                        Assert.IsNotType<Document>(p);
                    }
                    p = p.Parent;
                }
            }
        }
    }
}
