using NUnit.Framework;

using SemanticWord;
using SemanticWord.Annotation;
using SemanticWord.Types;
using System.Threading.Tasks;

namespace SWTest
{
    public class WikifierAnnotatorTests
    {

        [Test]
        public void TestCreate()
        {
            var a = new OntotextAnnotator();
        }

        [Test]
        public async Task TestAnnotate()
        {
            foreach (var r in Resources.SampleDocuments)
            {
                string text;
                using (var d = new Document(r))
                {
                    text = d.Text();
                }

                var annotator = new WikifierAnnotator();
                var metatags = await annotator.AnnotateAsync(text);

                using (var me = MetatagEditor.Open(r))
                {
                    me.ClearMetatags();
                    foreach (var mt in metatags)
                    {
                        try
                        {
                            me.AddMetatag(mt);
                        }
                        catch (InvalidMetatagException e)
                        {
                            System.Console.Error.WriteLine(
                                $"Failed to add a metatag: \"{e.Message}\"\n"
                                + $"{mt.Context}"
                            );
                        }
                    }
                }
            }
        }
    }
}
