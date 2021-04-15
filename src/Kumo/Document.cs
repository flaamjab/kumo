using System;
using System.Collections.Generic;
using System.IO;
using DocumentFormat.OpenXml.Packaging;

namespace Kumo
{
    /// <summary>Represents an MS Word document.</summary>
    public class Document : IDisposable
    {
        private WordprocessingDocument _package;
        private Body _body;

        private Document(WordprocessingDocument document)
        {
            _package = document;
            _body = new Body(
                _package.MainDocumentPart.Document,
                new RdfStore(_package.MainDocumentPart)
            );
        }

        /// <summary>
        ///   Creates a new instance of the <c>Document</c>
        ///   class from the specified file.
        /// </summary>
        public static Document Open(
            string path,
            bool isEditable = false)
        {
            var options = new OpenOptions();
            return Document.Open(path, isEditable, options);
        }

        /// <summary>
        ///   Creates a new instance of the <c>Document</c>
        ///   class from the specified file.
        /// </summary>
        public static Document Open(
            string path,
            bool isEditable,
            OpenOptions options)
        {
            var settings = options.ToOpenSettings();
            var d = WordprocessingDocument.Open(path, isEditable, settings);
            return new Document(d);
        }

        /// <summary>
        ///   Creates a new instance of the <c>Document</c> class 
        ///   from IO stream.
        /// </summary>
        public static Document Open(
            Stream stream,
            bool isEditable = false)
        {
            var options = new OpenOptions();
            return Document.Open(stream, isEditable, options);
        }

        /// <summary>
        ///   Creates a new instance of the <c>Document</c> class 
        ///   from IO stream.
        /// </summary>
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
            var clone = _package.Clone() as WordprocessingDocument;
            return new Document(clone);
        }

        /// <summary>
        ///   Saves the document to the underlying stream or path
        ///   that was used to open it.
        ///   <br/>
        ///   Some platforms do not support saving the document
        ///   to the same location it was opened from due to limitations
        ///   in <c>System.IO.Packaging.Package</c>.
        /// </summary>
        public void Save()
        {
            _package.Save();
        }

        /// <summary>The text content of the document.</summary>
        public string Text()
        {
            return _package.MainDocumentPart.Document.InnerText;
        }

        /// <summary>Fetches all the paragraphs contained within the document.</summary>
        public IEnumerable<IRange> Paragraphs()
        {
            throw new NotImplementedException();
        }

        /// <summary>Annotate the document text using the <c>annotator</c>.</summary>
        public void Annotate(IAnnotator annotator)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///   Flushes and saves the content,
        ///   closes the document, and releases all resources.
        /// </summary>
        public void Dispose()
        {
            _package.Dispose();
        }

        /// <summary>Fetches all the annotations contained within the document.</summary>
        public IEnumerable<IAnnotation> Annotations()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///   Creates a new reference to a text range
        ///   within the document that spans characters
        ///   from <c>start</c> to <c>end</c>, excluding
        ///   the character at <c>end</c>.
        /// </summary>
        public IRange Range(int start, int end)
        {
            return _body.Range(start, end);
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
