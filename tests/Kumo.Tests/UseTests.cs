using System.Linq;
using Xunit;

namespace Kumo.Tests
{
    [Collection("UseIO")]
    public class UseTests
    {
        [Fact]
        public void Annotate_SingleAnnotation_DocumentHasNewAnnotation()
        {
            foreach (var path in Documents.All())
            {
                using (var d = Documents.OpenInMemory(path))
                {
                    var w = d.Words().First();
                    var a = new Annotation();

                    w.Annotate(a);

                    d.Annotations().Contains(a);
                }
            }
        }
    }
}
