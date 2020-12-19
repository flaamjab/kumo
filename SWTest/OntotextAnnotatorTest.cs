using NUnit.Framework;

using SemanticWord;
using SemanticWord.Annotation;
using SemanticWord.Types;
using System.Threading.Tasks;

namespace SWTest
{
    public class OntotextAnnotatorTests
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
                var d = new Document(r);
                var text = d.Text();
                d.Close();

                var annotator = new OntotextAnnotator();
                await annotator.AnnotateAsync(text);

                var metatags = annotator.Annotations;

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
                                $"Failed to add a metatag: \"{e.Message}\""
                            );
                        }
                    }
                }
            }

        }
    }
}
