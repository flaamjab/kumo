using System;
using System.Linq;
using Xunit;

namespace Kumo.Tests
{
    [Collection("IO")]
    public class UseTests
    {
        [Fact]
        public void Annotate_SingleAnnotation_DocumentHasNewAnnotation()
        {
            foreach (var path in Documents.All())
            {
                using (var d = Documents.OpenInMemory(path))
                {
                    var p = d.Paragraphs().First();

                    var rel = new Uri("http://example.org/rel");
                    var val = new Uri("http://example.org/val");
                    var a = p.Annotate(
                        new Property(rel, val)
                    );

                    Assert.Single(d.Annotations(), a);
                }
            }
        }
    }
}
