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
    }
}
