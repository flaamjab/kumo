using NUnit.Framework;

using SemanticWord.Types;

namespace SWTest
{
    public class DocumentTests
    {
        [Test]
        public void TestCreate()
        {
            var d = new Document(Resources.SampleDocuments[0]);   
        }

        [Test]
        public void TestGetText() {
            var d = new Document(Resources.SampleDocuments[0]);
            Assert.AreEqual(
                "The Perm State University is located "
                + "in Perm, a city in Russia.",
                d.Text()
            );
        }
    }
}
