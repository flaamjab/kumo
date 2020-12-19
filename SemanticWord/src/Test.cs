using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using DocumentFormat.OpenXml.Packaging;

namespace SemanticWord
{
    public class Test
    {
        public static string XMLTestFilePath { get; set; } =
            "../resources/MetatagPartExample.xml";

        public static string DOCXTestFilePath { get; set; } =
            "../resources/邪術と妖術.docx";

        public static string DOCXOutputFilePath { get; set; }
            = "../resources/邪術と妖術_O.docx";

        public static bool TestXMLEditor()
        {
            Console.WriteLine("\nTesting XMLEditor!");
            MetatagXMLEditor mtxe = new MetatagXMLEditor();

            using (var fs = new FileStream(XMLTestFilePath, FileMode.Open))
            {
                try
                {
                    mtxe.LoadDocument(fs);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
            }

            try
            {
                Console.WriteLine("Attempting an erroneous remove!");
                mtxe.RemoveMetatag(-5);
            }
            catch (MetatagNotFoundException e)
            {
                Console.WriteLine(
                    $"Exception occurred as expected: {e.Message}"
                );
            }

            try
            {
                Metatag mt;

                // Get metatag list
                Console.WriteLine("Reading all metatags!");
                var mtd = mtxe.MetatagBundles();
                foreach (var a in mtd)
                    Console.WriteLine(a);

                // Removing the metatags
                mtxe.ClearMetatags();

                // Add and remove metatag
                Console.WriteLine("Adding 10 metatags!");
                var pIds = new List<int>();
                for (int ix = 0; ix < 10; ix++)
                {
                    mt = new Metatag("Text", "Context", "H");
                    pIds.Add(ix);
                    mtxe.AddMetatag(
                        new MetatagBundle(mt, ix, pIds.ToArray())
                    );
                }

                Console.WriteLine("Listing and removing these 10 metatags!");
                foreach (var mtb in mtxe.MetatagBundles())
                {
                    Console.WriteLine(mtb);
                    mtxe.RemoveMetatag(mtb.ID);
                }

                Console.WriteLine("\nCreating a metatag!");
                pIds = new List<int> { 0, 1, 2 };
                mt = new Metatag(
                    "Knock-knock",
                    "Stupid Knock-knock joke",
                    "Joke"
                );
                var oldMtb = new MetatagBundle(mt, 42, pIds.ToArray());
                mtxe.AddMetatag(oldMtb);
                Console.WriteLine(mtxe.GetRootXML());

                var newMt = new Metatag(
                    "Who's there?",
                    "Stupid Knock-knock joke (Who's there?)",
                    "Joke"
                );
                Console.WriteLine("\nReplacing the newly created metatag!");

                var newPIds = oldMtb.PIDs.ToList();
                newPIds.Add(4);
                mtxe.ReplaceMetatag(oldMtb.ID, newMt, newPIds.ToArray());

                Console.WriteLine(mtxe.GetRootXML());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        public static bool TestParaIDDocumentEditor()
        {
            using (
                var d = WordprocessingDocument.Open(DOCXTestFilePath, true)
            )
            {
                var de = new ParaIDReferenceEditor();
                de.Load(d.MainDocumentPart);

                de.ClearTags();

                Console.WriteLine("Adding 4 references!");
                de.TagByContext("英語の sorcery に対応");
                de.TagByContext("エヴァンズ・プリチャード");
                de.TagByContext("訳語としてwitchcraftという造語");

                Console.WriteLine("Trying to tag all paragraphs!\n");
                int[] ids = de.TagByContext("の");
                Console.WriteLine(ids.Length);
                Console.WriteLine("\nListing the added references!");
                foreach (int id in de.IDs)
                    Console.Write($"{id} ");
                Console.WriteLine();

                Console.WriteLine(
                    "Decrementing reference count by 1 on paragraph with id "
                    + ids[0]
                    + "(the paragraph should remain tagged)"
                );
                de.DecreaseReferenceCount(ids[0]);

                Console.WriteLine(
                    "Decrementing reference count by 2 on paragraph with id "
                    + ids[1]
                    + " (the paragraph should become untagged)"
                );
                de.DecreaseReferenceCount(ids[1]);
                de.DecreaseReferenceCount(ids[1]);

                Console.WriteLine("\nListing the result!");
                foreach (int id in de.IDs)
                    Console.Write($"{id} ");
                Console.WriteLine();

                Console.WriteLine("Decrementing it again to see if it breaks!");
                try
                {
                    de.DecreaseReferenceCount(ids[1]);
                    Console.WriteLine("Somehow it worked...");
                    return false;
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine(
                        $"Error occurred as expected: {e.Message}"
                    );
                }

                Console.WriteLine("\nTrying to access a non-existent paragraph!");
                try
                {
                    de.DecreaseReferenceCount(3);
                    Console.WriteLine("The paragraph was found...");
                    return false;
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine(
                        $"Exception occurred as expected: {e.Message}!"
                    );
                }

                Console.WriteLine(
                        "\nTrying to select paragraph by non-existent context"
                );
                try
                {
                    de.TagByContext("Beeshive");
                    Console.WriteLine("Apparently it worked...");
                    return false;
                }
                catch (InvalidMetatagException e)
                {
                    Console.WriteLine($"Exception occurred as expected: {e.Message}");
                }

                Console.WriteLine("\nSave on dispose!");
                de.Save(d.MainDocumentPart.GetStream());
            }

            using (
                var d = WordprocessingDocument.Open(DOCXTestFilePath, true)
            )
            {
                var de = new ParaIDReferenceEditor();
                de.Load(d.MainDocumentPart);
                Console.WriteLine("\nChecking that the references were saved!");
                if (de.IDs.Count() == 0)
                    return false;
                foreach (int id in de.IDs)
                    Console.Write($"{id} ");
                Console.WriteLine();
            }

            return true;
        }

        public static bool TestWordDocument()
        {
            Console.WriteLine("\nTesting WordDocument!");
            using (var wd = MetatagEditor.Open(DOCXTestFilePath))
            {
                Console.WriteLine(
                    "Opening a document!"
                );
                Console.WriteLine("\nListing the metatags!");
                foreach (var annot in wd.Metatags())
                    Console.WriteLine(annot);

                Console.WriteLine("\nCreating 4 metatags!");
                var a = new Metatag(
                    "呪術信仰", "呪術信仰において超自然的な", "A"
                );
                var b = new Metatag(
                    "エヴァンズ・プリチャード", "エヴァンズ・プリチャードに", "B"
                );
                var c = new Metatag(
                    "英語の  sorcery", "英語の sorcery に", "C"
                );
                var d = new Metatag(
                    "アフリカの",
                    "アフリカのアザンデ人",
                    "D"
                );

                Console.WriteLine("Adding the first 3 metatags!");
                wd.AddMetatag(a);
                wd.AddMetatag(b);
                wd.AddMetatag(c);

                Console.WriteLine("\nListing the metatags!");
                foreach (var annot in wd.Metatags())
                    Console.WriteLine(annot);

                try
                {
                    Console.WriteLine("Replacing A with C");
                    wd.ReplaceMetatag(a, c);
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine(
                        $"Exception occurred as expected: {e.Message}"
                    );
                }

                Console.WriteLine("Replacing A with D");
                wd.ReplaceMetatag(a, d);

                try
                {
                    Console.WriteLine("Removing A!");
                    wd.RemoveMetatag(a);
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine(
                        $"Exception occurred as expected: {e.Message}!"
                    );
                }

                Console.WriteLine("Removing B");
                wd.RemoveMetatag(b);

                Console.WriteLine("\nListing the metatags!");
                foreach (var tag in wd.Metatags())
                    Console.WriteLine(tag);

                Console.WriteLine("\nSaving on Dispose!");
            }

            using (var wd = MetatagEditor.Open(DOCXTestFilePath))
            {
                Console.WriteLine(
                    "\nReopening the document and listing saved metatags!"
                );
                foreach (var annot in wd.Metatags())
                    Console.WriteLine(annot);

                Console.WriteLine("\nSaving the result!");
                wd.SaveAs(DOCXOutputFilePath);

                Console.WriteLine("\nRemoving all the metatags!");
                wd.ClearMetatags();

                Console.WriteLine(
                    "Checking that there are none left!"
                );
                if (wd.Metatags().Count() != 0)
                {
                    Console.WriteLine("Failed to remove all metatags...");
                    return false;
                }
            }

            return true;
        }

        public static bool RunTests()
        {
            bool ok = false;
            try
            {
                ok =
                    TestXMLEditor()
                    && TestParaIDDocumentEditor()
                    && TestWordDocument();
            }
            catch (Exception e)
            {
                Console.WriteLine($"An exception occurred: {e}");
                ok = false;
            }

            if (ok)
                Console.WriteLine("\nSemantic Word passed all tests!");
            else
                Console.WriteLine("\nSomething seems not to work.");

            return true;
        }
    }
}
