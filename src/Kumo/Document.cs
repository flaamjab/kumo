using System;
using System.Collections.Generic;
using System.IO;
using DocumentFormat.OpenXml.Packaging;

namespace Kumo
{
    /// <summary>Represents a Word document.</summary>
    public class Document : IDisposable, IRange
    {
        private WordprocessingDocument _document;
        private Annotations _annotations;

        private Document(WordprocessingDocument document)
        {
            _document = document;
            _annotations = new Annotations(_document.MainDocumentPart);
        }

        public static Document Open(
            string path,
            bool isEditable = false)
        {
            var options = new OpenOptions();
            return Document.Open(path, isEditable, options);
        }

        public static Document Open(
            Stream stream,
            bool isEditable = false)
        {
            var options = new OpenOptions();
            return Document.Open(stream, isEditable, options);
        }

        public static Document Open(
            string path,
            bool isEditable,
            OpenOptions options)
        {
            var settings = options.ToOpenSettings();
            var d = WordprocessingDocument.Open(path, isEditable, settings);
            return new Document(d);
        }

        public static Document Open(
            Stream stream,
            bool isEditable,
            OpenOptions options)
        {
            var settings = options.ToOpenSettings();
            var d = WordprocessingDocument.Open(stream, isEditable, settings);
            return new Document(d);
        }

        /// <summary>Creates an editable clone of the document.</summary>
        public Document Clone()
        {
            var clone = _document.Clone() as WordprocessingDocument;
            return new Document(clone);
        }

        /** <summary>
                Saves the document to the underlying stream or path
                that was used to open it.
                <br/>
                Some platforms do not support saving the document
                to the same location it was opened from due to limitations
                in <c>System.IO.Packaging.Package</c>.
            </summary>
        */
        public void Save()
        {
            _document.Save();
        }

        public void SaveAs(string path)
        {
            _document.SaveAs(path);
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

        public IEnumerable<Annotation> Annotations()
        {
            throw new NotImplementedException();
        }

        /** <summary>
                Creates a new reference to a text range
                within the document that spans characters
                from <c>start</c> to <c>end</c>, excluding
                the character at <c>end</c>.
            </summary>
        */
        public Range Range(int start, int end)
        {
            return new Range(
                start, end,
                _document.MainDocumentPart.Document,
                _annotations
            );
        }

        public IEnumerable<Range> Paragraphs()
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>Options for opening the document.</summary>
    public class OpenOptions
    {
        public bool AutoSave { get; set; }
        public long MaxCharactersInPart { get; set; }
    }

    static partial class Extensions
    {
        public static OpenSettings ToOpenSettings(this OpenOptions options)
        {
            var settings = new OpenSettings();
            settings.AutoSave = options.AutoSave;
            settings.MaxCharactersInPart = options.MaxCharactersInPart;

            return settings;
        }
    }
}
