using System;
using System.IO;
using VDS.RDF.Parsing;
using VDS.RDF;
using Xunit;

namespace Kumo.Tests
{
    public class DocumentTests
    {
        [Fact]
        public void Stars_OnDocumentWithNone_ReturnsEmptyArray()
        {
            using (var d = Documents.Open("bookmarked"))
            {
                Assert.Empty(d.Stars());
            }
        }

        [Fact]
        public void Stars_DocumentWithMultipleAnnotations_MultipleAnnotations()
        {
            using (var d = Documents.Open("annotated"))
            {
                var ranges = d.Stars();

                Assert.NotEmpty(ranges);
            }
        }

        [Fact]
        public void RdfStream_DocumentWithRdfData_RdfDataIsReadSuccessfully()
        {
            using (var d = Documents.Open("annotated"))
            {
                using (var stream = d.RdfStream())
                {
                    var p = new NQuadsParser();

                    var store = new TripleStore();
                    var reader = new StreamReader(stream);
                    p.Load(store, reader);

                    Assert.NotEmpty(store.Triples);
                }
            }
        }

        [Fact]
        public void ModifyRangeProperty_OnReadOnlyDocument_Throws()
        {
            using (var d = Documents.Open("annotated"))
            {
                var r = d.Range(11, 30);
                var p = new Property(
                    new ("https://example.org/references"),
                    new Resource.Unique("https://example.org/resource")
                );

                Assert.Throws<InvalidOperationException>(() => r.Attach(p));
            }
        }

        [Fact]
        public void Save_RangeGraphModified_Success()
        {
            using (var d = Documents.Open("annotated", true))
            {
                var r = d.Range(0, 42);
                var p = new Property(
                    new ("https://example.org/references"),
                    new Resource.Unique("https://example.org/resource")
                );

                r.Attach(p);
            }
        }
    }
}
