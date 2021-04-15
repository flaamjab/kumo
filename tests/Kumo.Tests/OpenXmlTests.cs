using System;
using System.IO;
using System.Linq;
using Xunit;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;

namespace Kumo.Tests
{
    [Collection("IO")]
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
            var path = Documents.WithName("medium");
            using (var d = WordprocessingDocument.Open(path, false))
            {
                var text = d.MainDocumentPart.Document.InnerText;

                Assert.StartsWith("Lorem ipsum dolor sit amet", text);
                Assert.EndsWith("tristique.", text);
            }
        }

        [Fact]
        public void Equals_TwoDifferentParagraphs_False()
        {
            var path = Documents.WithName("small");
            using (var d = WordprocessingDocument.Open(path, false))
            {
                var content = d.MainDocumentPart.Document;
                var ps = content.Descendants<Paragraph>().ToArray();

                Assert.False(ps[0] == ps[1]);
            }
        }

        [Fact]
        public void Run_SplittingWithCloningProperties_Work()
        {
            var path = Documents.WithName("small");
            using (var d = WordprocessingDocument.Open(path, false))
            {
                var run = d
                    .MainDocumentPart.Document
                    .Descendants<Run>().First();
                var text = run.Descendants<Text>().First();
                int point = 5;

                var props = run.Descendants<RunProperties>().Last();

                var leftPart = new Text(text.Text[..point]);
                var rightPart = new Text(text.Text[point..]);

                var leftRun = new Run(new OpenXmlElement[] {
                    props.Clone() as RunProperties,
                    leftPart
                });

                var rightRun = new Run(new OpenXmlElement[] {
                    props.Clone() as RunProperties,
                    rightPart
                });

                Console.WriteLine(leftRun.OuterXml);
                Console.WriteLine(rightRun.OuterXml);
            }
        }

        [Fact]
        public void InsertAfterSelf_NewOpenXmlElement_Throws()
        {
            var node = new Run();
            Assert.Throws<InvalidOperationException>(
                () => node.InsertAfterSelf(new Run())
            );
        }
    }
}
