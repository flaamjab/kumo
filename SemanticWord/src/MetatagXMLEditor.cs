using System;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace SemanticWord
{
    class MetatagXMLEditor
    {
        public void CreateDocument()
        {
            _mtRoot = new XElement("MetatagList");
            _document = new XDocument(
                new XDeclaration("1.0.0", "utf-8", ""),
                _mtRoot
            );
        }

        public bool LoadDocument(Stream stream)
        {
            try
            {
                _document = XDocument.Load(stream);
                _mtRoot = _document.Element("MetatagList");
                if (_mtRoot == null)
                    return false;
            }
            catch (System.Xml.XmlException e)
            {
                Console.WriteLine(e.Message);

                return false;
            }

            return true;
        }

        public bool SaveDocument(Stream stream)
        {
            _document.Save(stream);

            return true;
        }

        public bool AddMetatag(MetatagBundle mtb)
        {
            var newMetatag = Serialize(mtb.Metatag);

            var idAttr = new XAttribute("id", IDConvert.ToString(mtb.ID));
            var pIdsAttr = new XAttribute(
                "pids", IDConvert.ToString(mtb.PIDs)
            );
            newMetatag.Add(idAttr, pIdsAttr);

            _mtRoot.Add(newMetatag);

            return false;
        }

        public bool RemoveMetatag(int id)
        {
            GetMetatagByID(id).Remove();

            return false;
        }

        public bool ReplaceMetatag(
            int id, Metatag newMt, int[] newPIds
        )
        {
            var el = GetMetatagByID(id);

            var newEl = Serialize(newMt);
            var idAttr = el.Attribute("id");
            var pIdsAttr = new XAttribute("pids", IDConvert.ToString(newPIds));
            newEl.Add(idAttr, pIdsAttr);

            el.ReplaceWith(newEl);

            return true;
        }

        public void UpdatePIds(int id, int[] newPIds)
        {
            var el = GetMetatagByID(id);

            el.Attribute("pids").Value = IDConvert.ToString(newPIds);
        }

        public void ClearMetatags()
        {
            CreateDocument();
        }

        public IEnumerable<MetatagBundle> MetatagBundles()
        {
            var elements = _mtRoot.Elements("Metatag");

            var idToMtb = new HashSet<int>();
            var mtbs = new List<MetatagBundle>();
            foreach (var el in elements)
            {
                int id = IDConvert.FromString(el.Attribute("id").Value);
                if (!idToMtb.Contains(id))
                {
                    Metatag mt = Deserialize(el);
                    ExtractPIDs(el, out var pIds);

                    idToMtb.Add(id);
                    mtbs.Add(new MetatagBundle(mt, id, pIds));
                }
            }

            return mtbs;
        }

        public string GetRootXML()
        {
            return _mtRoot.ToString();
        }

        private void ExtractPIDs(XElement mtElem, out int[] pIds)
        {
            var pIdsAttr = mtElem.Attribute("pids");
            if (pIdsAttr != null)
                pIds = IDConvert.FromListString(
                    mtElem.Attribute("pids").Value
                ).ToArray();
            else
                pIds = new int[0];
        }

        private XElement Serialize(Metatag mt)
        {
            using (var ms = new MemoryStream())
            {
                var s = new XmlSerializer(typeof(Metatag));

                var ns = new XmlSerializerNamespaces();
                ns.Add("", "");

                s.Serialize(ms, mt, ns);

                ms.Position = 0;

                return XElement.Load(ms);
            }
        }

        private Metatag Deserialize(XElement mtEl)
        {
            using (var ms = new MemoryStream())
            {
                var sw = new StreamWriter(ms);
                sw.Write(mtEl.ToString());
                sw.Flush();
                ms.Position = 0;

                var s = new XmlSerializer(typeof(Metatag));
                var annot = (Metatag)s.Deserialize(ms);

                return annot;
            }
        }

        private XElement GetMetatagByID(int id)
        {
            var metatags = _mtRoot.Elements()
                .Where(
                    e => IDConvert.FromString(e.Attribute("id").Value) == id
                );

            if (metatags.Count() > 0)
            {
                return metatags.First();
            }
            else
                throw new MetatagNotFoundException(
                    $"Could not find a metatag with ID {id}"
                );
        }

        protected XDocument _document;
        XElement _mtRoot;
    }

}