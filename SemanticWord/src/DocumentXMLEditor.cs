using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace SemanticWord
{
    abstract class DocumentXMLEditor
    {
        public abstract IEnumerable<int> IDs { get; }

        public DocumentXMLEditor()
        {
            _docRoot = new Document();
        }

        public DocumentXMLEditor(MainDocumentPart mainDocumentPart)
        {
            _docRoot = mainDocumentPart.Document;
        }

        public abstract int[] TagByContext(
            string context, bool increaseRefCount = true
        );
        public abstract void ClearTags();
        public abstract int IncreaseReferenceCount(int id);
        public abstract int DecreaseReferenceCount(int id);

        public void Load(MainDocumentPart mainDocumentPart)
        {
            _docRoot.Load(mainDocumentPart);
        }

        public void Save(Stream stream)
        {
            stream.SetLength(0);
            _docRoot.Save(stream);
        }

        public string Text()
        {
            return _docRoot.InnerText;
        }

        public string GetParagraph(int id)
        {
            return _taggedParagraphs[id].InnerText;
        }

        public IEnumerable<string> GetParagraphs(int[] ids)
        {
            return ids.Select(id => _taggedParagraphs[id].InnerText);
        }

        protected abstract void Untag(int id);

        protected int GetRefCount(string id)
        {
            int usIx = id.LastIndexOf("_");

            return Convert.ToInt32(id.Substring(usIx + 1));
        }

        protected string SetRefCount(string id, int refCount)
        {
            int usIx = id.LastIndexOf("_");

            return id.Substring(0, usIx + 1) + refCount;
        }

        protected string[] SplitNameID(string value)
        {
            if (value.Count(c => c == '_') == 3)
                return value.Split('_', 4);
            else
                return new string[0];
        }

        protected bool IsValidID(string[] parts)
        {
            if (parts.Length != 4)
                return false;

            if (
                parts[0] == "SW"
                && parts[1] == "METATAG"
                && parts[2].Length == 8
            )
                return true;

            return false;
        }

        protected int GenerateID()
        {
            return (new Random()).Next();
        }

        protected Dictionary<int, Paragraph> _taggedParagraphs;
        protected Document _docRoot;
    }

    // class BookmarkDocumentXMLEditor : DocumentXMLEditor
    // {
    // }

    class ParaIDReferenceEditor : DocumentXMLEditor
    {
        override public IEnumerable<int> IDs
        {
            get
            {
                if (_taggedParagraphs == null)
                    _taggedParagraphs = TaggedParagraphs();

                return _taggedParagraphs.Keys;
            }
        }

        public ParaIDReferenceEditor() { }

        public ParaIDReferenceEditor(MainDocumentPart part)
        {
            _docRoot = part.Document;
        }

        override public int[] TagByContext(
            string context, bool increaseRefCount = true
        )
        {
            if (_taggedParagraphs == null)
                _taggedParagraphs = TaggedParagraphs();

            context = TextTransform
                .RemoveNonAlphanum(context)
                .ToLowerInvariant();
            var ps = _docRoot.Body
                .Elements<Paragraph>()
                .Where(
                    x =>
                    {
                        var t = TextTransform
                            .RemoveNonAlphanum(x.InnerText)
                            .ToLowerInvariant();

                        System.Console.WriteLine($"{t} | {context}");

                        return t.Contains(context);
                    } 
                );

            if (ps.Count() == 0)
                throw new InvalidMetatagException(
                    "The provided context string was "
                    + "not found within the text"
                );

            var pIds = new List<int>();
            foreach (var p in ps)
            {
                int id = 0;
                if (p.ParagraphId == null)
                {
                    id = GenerateID();
                    p.ParagraphId = new HexBinaryValue(
                        _swMetatagPrefix + IDConvert.ToString(id) + "_1"
                    );
                }
                else
                {
                    var idParts = SplitNameID(p.ParagraphId);
                    p.ParagraphId.Value = _swMetatagPrefix;
                    if (!IsValidID(idParts))
                    {
                        id = GenerateID();
                        p.ParagraphId.Value +=
                            IDConvert.ToString(id) + "_1";
                    }
                    else
                    {
                        id = IDConvert.FromString(idParts[2]);
                        int refCount = Convert.ToInt32(idParts[3]);

                        if (increaseRefCount)
                            refCount++;

                        p.ParagraphId.Value +=
                            idParts[2] + "_" + refCount;
                    }
                }
                _taggedParagraphs[id] = p;
                pIds.Add(id);
            }

            return pIds.ToArray();
        }

        override public int IncreaseReferenceCount(int id)
        {
            if (_taggedParagraphs == null)
                _taggedParagraphs = TaggedParagraphs();

            if (!_taggedParagraphs.ContainsKey(id))
                throw new InvalidOperationException(
                    $"The paragraph with ID {id} was not found"
            );

            var p = _taggedParagraphs[id].ParagraphId;
            int refCount = GetRefCount(p);
            p.Value = SetRefCount(p, ++refCount);

            return refCount;
        }

        override public int DecreaseReferenceCount(int id)
        {
            if (_taggedParagraphs == null)
                _taggedParagraphs = TaggedParagraphs();

            if (!_taggedParagraphs.ContainsKey(id))
                throw new InvalidOperationException(
                    $"The paragraph with ID {id} was not found"
                );

            var p = _taggedParagraphs[id].ParagraphId;
            int refCount = GetRefCount(p);

            if (--refCount <= 0)
                Untag(id);
            else
                p.Value = SetRefCount(p, refCount);

            return refCount;
        }

        override public void ClearTags()
        {
            if (_taggedParagraphs == null)
                _taggedParagraphs = TaggedParagraphs();

            foreach (var p in _taggedParagraphs.Values)
                p.ParagraphId = null;

            _taggedParagraphs.Clear();
        }

        override protected void Untag(int id)
        {
            if (_taggedParagraphs == null)
                _taggedParagraphs = TaggedParagraphs();

            if (_taggedParagraphs.Count == 0)
                throw new InvalidOperationException(
                    "No tagged paragraphs found in the document"
                );

            if (!_taggedParagraphs.ContainsKey(id))
                throw new ArgumentException(
                    "The paragraph with the provided ID "
                    + "does not exist"
                );

            var p = _taggedParagraphs[id];
            p.ParagraphId = null;
            _taggedParagraphs.Remove(id);
        }

        private Dictionary<int, Paragraph> TaggedParagraphs()
        {
            var ps = _docRoot.Body.Elements<Paragraph>();
            return ps.Where(p => p.ParagraphId != null)
                .Select(p => SplitNameID(p.ParagraphId.Value))
                .Where(idParts => IsValidID(idParts))
                .Select(idParts => IDConvert.FromString(idParts[2]))
                .Zip(ps, (id, p) => (id, p))
                .ToDictionary(kv => kv.id, kv => kv.p);
        }

        private const string _swMetatagPrefix = "SW_METATAG_";
    }
}