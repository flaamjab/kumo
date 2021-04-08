using System;
using System.Collections.Generic;
using System.IO;
using DocumentFormat.OpenXml.Packaging;

namespace Kumo
{
    public class Document : IDisposable, IRange
    {
        public static Document Open(string path, bool isEditable)
        {
            var d = WordprocessingDocument.Open(path, isEditable);
            return new Document(d);
        }

        public static Document Open(Stream stream, bool isEditable)
        {
            var d = WordprocessingDocument.Open(stream, isEditable);
            return new Document(d);
        }

        public void Save()
        {
            _document.Save();
        }

        public void SaveAs(string path)
        {
            _document.SaveAs(path);
        }

        public IEnumerable<Annotation> Annotations()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Range> Words()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Range> Paragraphs()
        {
            throw new NotImplementedException();
        }

        public Range Range(int offset, int length)
        {
            throw new NotImplementedException();
        }

        public string Text()
        {
            throw new NotImplementedException();
        }

        public void Annotate(Annotation annotation)
        {
            throw new NotImplementedException();
        }

        public void Annotate(IEnumerable<Annotation> annotations)
        {
            throw new NotImplementedException();
        }

        public void Annotate(IAnnotator annotator)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _document.Dispose();
        }

        private Document(WordprocessingDocument document)
        {
            _document = document;
            _annotations = new Annotations(_document.MainDocumentPart);
        }

        private WordprocessingDocument _document;
        private Annotations _annotations;
    }
}
