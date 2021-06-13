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
        private Package _package;

        /// <summary>
        ///   The text of the document, with no spaces or
        ///   newlines between paragraphs.
        /// </summary>
        public string Text => _package.Content.Text;

        private Document(Package package)
        {
            _package = package;
        }

        /// <summary>
        ///   Creates a new instance of the <c>Document</c>
        ///   class from the specified file.
        /// </summary>
        /// <param name="path">The path to the document</param>
        /// <param name="editable">
        ///   If <c>true</c>, the document will be opened in ReadWrite mode.
        /// </param>
        /// <returns>A new instance of <c>Document</c>.</returns>
        public static Document Open(string path, bool editable = false)
        {
            var options = new OpenSettings();
            return Document.Open(path, editable, options);
        }

        /// <summary>
        ///   Creates a new instance of the <c>Document</c>
        ///   class from the specified file.
        /// </summary>
        /// <param name="path">The path to the document.</param>
        /// <param name="editable">
        ///   If <c>true</c>, the document will be opened in ReadWrite mode.
        /// </param>
        /// <param name="settings">Settings for opening the document</param>
        /// <returns>A new instance of <c>Document</c>.</returns>
        public static Document Open(
            string path,
            bool editable,
            OpenSettings settings)
        {
            var openXmlSettings = settings.ToOpenXmlOpenSettings();
            var d = WordprocessingDocument.Open(
                path, editable, openXmlSettings
            );

            var p = new Package(d, editable, settings.AutoSave);
            return new Document(p);
        }

        /// <summary>
        ///   Creates a new instance of the <c>Document</c> class 
        ///   from IO stream.
        /// </summary>
        /// <param name="stream">The stream to open the document from.</param>
        /// <param name="editable">
        ///   If stream is in ReadWrite mode,
        ///   <c>false</c> means the document cannot be edited.
        /// </param>
        /// <returns>A new instance of <c>Document</c>.</returns>
        public static Document Open(
            Stream stream,
            bool editable = false)
        {
            var options = new OpenSettings();
            return Document.Open(stream, editable, options);
        }

        /// <summary>
        ///   Creates a new instance of the <c>Document</c> class 
        ///   from thes IO stream.
        /// </summary>
        /// <param name="stream">The stream to open the document from.</param>
        /// <param name="editable">
        ///   If <c>stream</c> is in <c>ReadWrite</c> mode,
        ///   <c>false</c> means the document cannot be edited.
        /// </param>
        /// <param name="settings">Options for opening the document</param>
        /// <returns>A new instance of <c>Document</c>.</returns>
        public static Document Open(
            Stream stream,
            bool editable,
            OpenSettings settings)
        {
            var openXmlSettings = settings.ToOpenXmlOpenSettings();
            var d = WordprocessingDocument.Open(
                stream, editable, openXmlSettings
            );

            var p = new Package(d, editable, settings.AutoSave);
            return new Document(p);
        }

        /// <summary>
        ///   Creates an editable clone of the document,
        ///   opened on a <c>System.IO.MemoryStream</c> with
        ///   expandable capacity and default settings.
        /// </summary>
        /// <returns>A new instance of <c>Document</c>.</returns>
        public Document Clone()
        {
            var clone = (Package)_package.Clone();
            return new Document(clone);
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
        }

        /// <summary>Enumerates all the paragraphs contained within the document.</summary>
        /// <returns>All paragraphs within the document 
        /// as instances of <c>Range</c>.</returns>
        public IEnumerable<Range> Paragraphs()
        {
            throw new NotImplementedException();
        }

        /// <summary>Flushes and saves the content,
        /// closes the document, and releases
        /// all resources.</summary>
        public void Dispose()
        {
            _package.Dispose();
        }

        /// <summary>Fetches all annotated ranges contained within the document.</summary>
        /// <returns>All the <c>Range</c>s in the document that have properties attached
        /// or relate to some other <c>Range</c>s.</returns>
        public IEnumerable<Range> Stars()
        {
            return _package.Stars();
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
        /// <returns>A range for the specified bounds.</returns>
        public Range Range(int start, int end)
        {
            return _package.Content.Range(start, end);
        }

        /// <summary>Enumerates all ranges for which text matches the provided.</summary>
        /// <param name="text">Text value to match.</param>
        /// <param name="comparison">The StringComparison to use for matching.</param>
        public IEnumerable<Range> Ranges(
            string text,
            StringComparison comparison = StringComparison.CurrentCulture)
        {
            return _package.Content.Ranges(text, comparison);
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
