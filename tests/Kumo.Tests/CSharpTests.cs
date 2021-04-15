using System;
using DocumentFormat.OpenXml.Drawing.Charts;
using Xunit;

namespace Kumo.Tests
{
    public class CSharpTests
    {
        [Fact]
        public void GetHashCode_TwoDistinctTuplesWithSameValues_True()
        {
            var tupleA = (1, 2);
            var tupleB = (1, 2);

            Assert.Equal(tupleA.GetHashCode(), tupleB.GetHashCode());
        }

        [Fact]
        public void Uri_OddString_Throws()
        {
            Assert.Throws<UriFormatException>(() => {
                new Uri("Odd");
            });
        }

        [Fact]
        public void Uri_OddProtocol_Ok()
        {
            new Uri("odd://example.com");
        }
    }
}
