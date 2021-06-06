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

        public static MemoryStream MemoryStream(string name)
        {
            var path = Named(name);
            using (var fs = new FileStream(
                path, FileMode.Open, FileAccess.ReadWrite))
            {
                var ms = new MemoryStream();
                fs.CopyTo(ms);

                return ms;
            }
        }

        public static Document Open(string name, bool editable = false)
        {
            var stream = MemoryStream(name);
            return Document.Open(stream, editable);
        }
    }
}
