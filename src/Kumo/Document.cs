using System;
using System.Collections.Generic;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using OpenXmlOpenSettings = DocumentFormat.OpenXml.Packaging.OpenSettings;

namespace Kumo
{
    /// <summary>Represents an MS Word document.</summary>
    public class Document : IDisposable
    {
        private WordprocessingDocument _package;
        private Body _body;
        private RdfStore _rdf;

        private Document(WordprocessingDocument document, bool autoSave)
        {
            _package = document;
            _rdf = new RdfStore(_package.MainDocumentPart, autoSave);
            _body = new Body(_package.MainDocumentPart.Document, _rdf);
        }

        /// <summary>
        ///   Creates a new instance of the <c>Document</c>
        ///   class from the specified file.
        /// </summary>
        /// <param name="isEditable">
        ///   If <c>true</c>, the document will be opened in ReadWrite mode.
        /// </param>
        public static Document Open(
            string path,
            bool isEditable = false)
        {
            var options = new OpenSettings();
            return Document.Open(path, isEditable, options);
        }

        /// <summary>
        ///   Creates a new instance of the <c>Document</c>
        ///   class from the specified file.
        /// </summary>
        /// <param name="path">The path to the document.</para>
        /// <param name="isEditable">
        ///   If <c>true</c>, the document will be opened in ReadWrite mode.
        /// </param>
        /// <param name="settings">Settings for opening the document</param>
        public static Document Open(
            string path,
            bool isEditable,
            OpenSettings settings)
        {
            var openXmlSettings = settings.ToOpenXmlOpenSettings();
            var d = WordprocessingDocument.Open(
                path, isEditable, openXmlSettings
            );

            return new Document(d, settings.AutoSave);
        }

        /// <summary>
        ///   Creates a new instance of the <c>Document</c> class 
        ///   from IO stream.
        /// </summary>
        /// <param name="stream">The stream to open the document from.</param>
        /// <param name="isEditable">
        ///   If stream is in ReadWrite mode,
        ///   <c>false</c> means the document cannot be edited.
        /// </param>
        public static Document Open(
            Stream stream,
            bool isEditable = false)
        {
            var options = new OpenSettings();
            return Document.Open(stream, isEditable, options);
        }

        /// <summary>
        ///   Creates a new instance of the <c>Document</c> class 
        ///   from thes IO stream.
        /// </summary>
        /// <param name="stream">The stream to open the document from.</param>
        /// <param name="isEditable">
        ///   If stream is in ReadWrite mode,
        ///   <c>false</c> means the document cannot be edited.
        /// </param>
        /// <param name="settings">Options for opening the document</param>
        public static Document Open(
            Stream stream,
            bool isEditable,
            OpenSettings settings)
        {
            var openXmlSettings = settings.ToOpenXmlOpenSettings();
            var d = WordprocessingDocument.Open(
                stream, isEditable, openXmlSettings
            );

            return new Document(d, settings.AutoSave);
        }

        /// <summary>
        ///   <para> Saves the document to the underlying stream or path
        ///   that was used to open it.</para>
        ///   <para>Some platforms do not support saving
        ///   the document while it is still open due to 
        ///   limitations in <c>System.IO.Packaging.Package</c>.</para>
        /// </summary>
        public void Save()
        {
            _package.Save();
            _rdf.Save();
        }

        /// <summary>Fetches all the paragraphs contained within the document.</summary>
        public IEnumerable<IRange> Paragraphs()
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
            return _body.Annotations();
        }

        /// <summary>
        ///   <para>Creates a new reference to a text range
        ///   within the document that spans characters
        ///   from <c>start</c> up to <c>end</c>.</para>
        ///   <para>For example, given a document with text "Hello, world!"
        ///   a range from 1 up to 5 will select the "ello" fragment.</para>
        /// </summary>
        /// <param name="start">The start character position of the range.</param>
        /// <param name="end">
        ///   The character position directly after the end of the range.
        /// </param>
        public IRange Range(int start, int end)
        {
            return _body.Range(start, end);
        }
    }

    /// <summary>Settings for opening the document.</summary>
    public class OpenSettings
    {
        public bool AutoSave { get; set; } = true;
        public long MaxCharactersInPart { get; set; }
    }

    static partial class ConversionExtensions
    {
        public static OpenXmlOpenSettings ToOpenXmlOpenSettings(
            this OpenSettings settings)
        {
            var oXmlSettings = new OpenXmlOpenSettings();
            oXmlSettings.AutoSave = settings.AutoSave;
            oXmlSettings.MaxCharactersInPart = settings.MaxCharactersInPart;

            return oXmlSettings;
        }
    }
}
