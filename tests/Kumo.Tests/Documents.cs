using System.Collections.Generic;
using System.IO;

namespace Kumo.Tests
{
    static class Documents
    {
        const string DIRECTORY = "../../../../Data/";

        public static IEnumerable<string> All()
        {
            return Directory.EnumerateFiles(DIRECTORY);
        }

        public static string WithName(string name)
        {
            return Path.Join(DIRECTORY, name + ".docx");
        }

        public static Document OpenInMemory(string path)
        {
            var d = Document.Open(path);
            return d.Clone();
        }
    }
}
