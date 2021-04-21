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
        /// <returns>A new instance of <c>Document</c>.</returns>
        public static Document Open(string path, bool isEditable = false)
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
        /// <returns>A new instance of <c>Document</c>.</returns>
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
        /// <returns>A new instance of <c>Document</c>.</returns>
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
        /// <returns>A new instance of <c>Document</c>.</returns>
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
        ///   Creates an editable clone of the document,
        ///   opened on a System.IO.MemoryStream with
        ///   expandable capacity and default settings.
        /// </summary>
        /// <returns>A new instance of <c>Document</c>.</returns>
        public Document Clone()
        {
            var clone = (WordprocessingDocument)_package.Clone();
            return new Document(clone, true);
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
        /// <returns>All paragraphs within the document 
        /// as instances of <c>IRange</c>.</returns>
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

        /// <summary>Fetches all annotated ranges contained within the document.</summary>
        /// <returns>All the <c>IRange</c>s in the document that have properties attached
        /// or relate to some other <c>IRange</c>s.</returns>
        public IEnumerable<IRange> Stars()
        {
            return _body.Stars();
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
        /// <returns>A range for the specified bounds.</range>
        public IRange Range(int start, int end)
        {
            return _body.Range(start, end);
        }
    }

    /// <summary>Settings for opening the document.</summary>
    public class OpenSettings
    {
        /// <summary>Gets or sets a value indicating whether to
        /// autosave document modifications. The default value is true.</summary>
        public bool AutoSave { get; set; } = true;

        /// <summary><para>Gets or sets a value that indicates the maximum number of
        /// allowable characters in an Open XML part.
        /// A zero (0) value indicates that there are no limits on the size
        /// of the part. A non-zero value specifies the maximum size,
        /// in characters.</para>
        /// <para>This property allows you to mitigate denial of service attacks
        /// where the attacker submits a package with an extremely
        /// large Open XML part. By limiting the size of the part, you can
        /// detect the attack and recover reliably.</para></summary>


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
