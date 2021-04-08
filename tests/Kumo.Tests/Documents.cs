using System;
using System.Collections.Generic;
using System.IO;
using DocumentFormat.OpenXml.Packaging;

namespace Kumo.Tests
{
    static class Documents
    {
        const string DIRECTORY = "../../../../data/";

        public static IEnumerable<string> All()
        {
            return Directory.EnumerateFiles(DIRECTORY);
        }

        public static Document OpenInMemory(string path)
        {
            var d = Document.Open(path);
            return d.Clone();
        }
    }
}
