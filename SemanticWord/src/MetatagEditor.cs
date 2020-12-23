using System;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using System.Collections.Generic;

using DocumentFormat.OpenXml.Packaging;

namespace SemanticWord
{
    public class MetatagEditor : IDisposable
    {
        /// <summary>Gets a value indicating whether saving 
        /// the package is supported by calling
        /// <c>DocumentFormat.OpenXml.Packaging.OpenXmlPackage.Save</c>. 
        /// Some platforms (such as .NET Core), 
        /// have limited support for saving.
        /// If false, in order to save, the document and/or package needs 
        /// to be fully closed and disposed and then reopened.</summary>
        public bool CanSave { get { return WordprocessingDocument.CanSave; } }

        public string Path { get { return _path; } }

        /// <summary>Create a new instance of MetatagEditor
        /// class from the specified file</summary>
        static public MetatagEditor Open(string path, bool isEditable = true)
        {
            return new MetatagEditor(path, isEditable);
        }

        /// <summary>Retrieves the list of metatags
        /// contained in the document.</summary>
        public IEnumerable<Metatag> Metatags()
        {
            if (_mtToMtb == null)
            {
                InitializeMetatagPart();
            }

            return _mtToMtb.Keys;
        }

        /// <summary>Adds a metatag to the document.
        /// All paragraphs that contain a sequence of words
        /// matching that of metatag <c>Context</c>
        /// are tagged.</summary>
        public MetatagEditor AddMetatag(Metatag metatag)
        {
            var id = MetatagBundle.GenerateID();
            var pIds = _dXmlEditor.TagByContext(metatag.Context);

            var mtb = new MetatagBundle(metatag, id, pIds);
            _mtToMtb.Add(metatag, mtb);

            _mtXmlEditor.AddMetatag(mtb);

            return this;
        }

        /// <summary>Removes the specified metatag from the document.</summary>
        public MetatagEditor RemoveMetatag(Metatag metatag)
        {
            if (_mtToMtb.Count == 0)
                throw new InvalidOperationException(
                    "The document does not contain metatags"
                );

            if (_mtToMtb.TryGetValue(metatag, out MetatagBundle mtb))
            {
                _mtToMtb.Remove(metatag);
                _mtXmlEditor.RemoveMetatag(mtb.ID);

                foreach (var pId in mtb.PIDs)
                    _dXmlEditor.DecreaseReferenceCount(pId);
            }
            else
                throw new ArgumentException(
                    "Metatag was not found"
                );

            return this;
        }

        /// <summary>Replaces the old metatag with the new one
        /// in the document. The new tag will have the same ID
        /// as the one it replaces but the paragraphs it annotates 
        /// will be recalculated based on its <c>Context</c>.</summary>
        public MetatagEditor ReplaceMetatag(
            Metatag oldMetatag,
            Metatag newMetatag
        )
        {
            if (_mtToMtb.Count == 0)
                throw new InvalidOperationException(
                    "The document does not contain metatags"
                );

            if (_mtToMtb.ContainsKey(newMetatag))
                throw new InvalidOperationException(
                    "Cannot replace the metatag with one already "
                    + "contained within the document as this will lead to "
                    + "duplicate entries"
                );

            if (_mtToMtb.TryGetValue(oldMetatag, out MetatagBundle oldMtb))
            {
                _mtToMtb.Remove(oldMetatag);

                oldMtb.Metatag = newMetatag;
                _mtToMtb[newMetatag] = oldMtb;

                int[] ids = _dXmlEditor.TagByContext(newMetatag.Context);
                foreach (var pId in oldMtb.PIDs)
                    _dXmlEditor.DecreaseReferenceCount(pId);

                _mtXmlEditor.ReplaceMetatag(oldMtb.ID, newMetatag, ids);
            }
            else
                throw new ArgumentException(
                    "Metatag to be replaced was not found"
                );

            return this;
        }

        public IEnumerable<string> AnnotatedParagraphs(Metatag metatag)
        {
            return _dXmlEditor.GetParagraphs(_mtToMtb[metatag].PIDs);
        }

        /// <summary>Remove all the metatags from the document</summary>
        public MetatagEditor ClearMetatags()
        {
            if (_mtToMtb == null)
            {
                InitializeMetatagPart();
            }

            _mtToMtb.Clear();
            _mtXmlEditor.ClearMetatags();
            _dXmlEditor.ClearTags();

            return this;
        }

        /// <summary>Saves the document to the specified file.
        /// On some platforms the document cannot be saved until 
        /// it's closed.</summary>
        public MetatagEditor Save()
        {
            SaveParts();
            _document.Save();

            return this;
        }

        /// <summary>Saves the document to the specified file</summary>
        public MetatagEditor SaveAs(string path)
        {
            SaveParts();
            _document.SaveAs(path);

            return this;
        }

        /// <summary>Flushes and saves the content, closes the
        /// document and releases all resources.</summary>
        public void Dispose()
        {
            SaveParts();
            _document.Dispose();
        }

        /// <summary>Saves the document and releases all resources.</summary>
        public void Close()
        {
            Dispose();
        }

        private MetatagEditor(string path, bool isEditable)
        {
            _path = path;
            _document = WordprocessingDocument.Open(path, isEditable);

            _dXmlEditor = new ParaIDReferenceEditor(
                _document.MainDocumentPart
            );
            _mtXmlEditor = new MetatagXMLEditor();

            if (ReadMetatagPart())
                SynchronizeParts();
        }

        private void SaveParts()
        {
            if (_metatagPart != null)
                using (var ms = new MemoryStream())
                {
                    _mtXmlEditor.SaveDocument(ms);
                    ms.Position = 0;
                    _metatagPart.FeedData(ms);
                }
        }

        private void InitializeMetatagPart()
        {
            if (!ReadMetatagPart())
                CreateMetatagPart();
        }

        private void SynchronizeParts()
        {
            foreach (var v in _mtToMtb.Values)
            {
                int[] pIds = _dXmlEditor.TagByContext(
                    v.Metatag.Context, false
                );

                if (pIds.Length > 0)
                {
                    v.PIDs = v.PIDs.Union(pIds).ToArray();
                    _mtXmlEditor.UpdatePIds(v.ID, v.PIDs);
                }
                else
                {
                    _mtXmlEditor.RemoveMetatag(v.ID);
                    _mtToMtb.Remove(v.Metatag);
                }
            }
        }

        private bool ReadMetatagPart()
        {
            var m = _document.MainDocumentPart;

            var mtParts = m.CustomXmlParts;
            if (mtParts.Count() == 0)
                return false;

            _mtToMtb = new Dictionary<Metatag, MetatagBundle>();
            foreach (var p in mtParts)
            {
                using
                (
                    var s = p.GetStream(
                        FileMode.Open, FileAccess.Read
                    )
                )
                {
                    if (_mtXmlEditor.LoadDocument(s))
                    {
                        Console.WriteLine("Document loaded successfully");
                        _mtToMtb = _mtXmlEditor.MetatagBundles()
                            .ToDictionary(v => v.Metatag);
                        _metatagPart = p;

                        return true;
                    }
                }
            }

            return false;
        }

        private void CreateMetatagPart()
        {
            _mtToMtb = new Dictionary<Metatag, MetatagBundle>();
            var m = _document.MainDocumentPart;
            _metatagPart = m.AddCustomXmlPart(CustomXmlPartType.CustomXml);
            _mtXmlEditor.CreateDocument();
        }

        private Dictionary<Metatag, MetatagBundle> _mtToMtb;
        private readonly string _path;
        private readonly DocumentXMLEditor _dXmlEditor;
        private readonly MetatagXMLEditor _mtXmlEditor;
        private readonly WordprocessingDocument _document;
        private CustomXmlPart _metatagPart;
        private const string _mtsPartRootName = "MetatagList";
    }

}