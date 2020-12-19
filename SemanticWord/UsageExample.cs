using System;
using System.Linq;

using SemanticWord;

class Program
{
    static void Main(string[] args)
    {
        string file = "MrBean.docx";
        using (var mte = MetatagEditor.Open(file))
        {
            var mtA = new Metatag(
                "Mr. Bean",
                "Mr. Bean is a British sitcom",
                "TV Show"
            );
            mte.AddMetatag(mtA);

            var mtB = new Metatag(
                "Rowan Atkinson",
                "created by Rowan Atkinson and",
                "Actor"
            );
            mte.ReplaceMetatag(mtA, mtB);

            Console.WriteLine(mte.Metatags().First());

            var ps = mte.AnnotatedParagraphs(mtB);
            foreach (var p in ps)
                Console.WriteLine(p);

            mte.RemoveMetatag(mtB);
        }
    }
}