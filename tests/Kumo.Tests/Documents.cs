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

        public static string Named(string name)
        {
            return Path.Join(DIRECTORY, name + ".docx");
        }

        public static Document Open(string name, bool autosave = false)
        {
            var path = Path.Join(DIRECTORY, name + ".docx");
            var settings = new OpenSettings();
            settings.AutoSave = autosave;

            return Document.Open(path, true, settings);
        }
    }
}
