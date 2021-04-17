#nullable enable

using System.Diagnostics;
using System;
using System.Linq;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using VDS.RDF;
using VDS.RDF.Writing;
using VDS.RDF.Parsing;

namespace Kumo
{
    class RdfStore
    {
        private const string TEXT_ANNOTATION_PART_ID = "kumo-text-annotations";

        private MainDocumentPart _mainPart;
        private CustomXmlPart? _textAnnotationContainer = null;
        private Graph? _graph = null;

        private RdfXmlWriter _writer;
        private RdfXmlParser _parser;

        public RdfStore(MainDocumentPart mainPart)
        {
            _mainPart = mainPart;

            _writer = new RdfXmlWriter();
            _parser = new RdfXmlParser();
        }

        public void Assert(Description description)
        {
            var g = Graph();
            var triples = description.ToTriples(g);
            
            Debug.Assert(triples.Length > 0);
            g.Assert(triples);

            var c = Container(TEXT_ANNOTATION_PART_ID);
            
            using (var ms = new MemoryStream())
            {
                var w = new StreamWriter(ms);
                _writer.Save(g, w, true);

                ms.Position = 0;
                c.FeedData(ms);
            }
        }

        public void Retract(string id)
        {
            throw new NotImplementedException();
        }

        public Description Get(int id)
        {
            var g = Graph();
            var subject = g.GetBlankNode(id.ToString());
            var triples = g.GetTriplesWithSubject(subject).ToArray();

            if (triples.Length > 0)
            {
                return Description.FromTriples(id, triples);
            }
            else
            {
                throw new InvalidOperationException(
                    $"a description with ID \"{id}\" does not exist"
                );
            }
        }

        public bool Exists(int id)
        {
            var g = Graph();
            var subject = g.GetBlankNode(id.ToString());
            var triples = g.GetTriplesWithSubject(subject).ToArray();

            if (triples.FirstOrDefault() == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private Graph Graph()
        {
            if (_graph != null)
            {
                return _graph;
            }

            _graph = new Graph();
            _graph.NamespaceMap.AddNamespace(
                Schema.Prefix,
                Schema.Namespace
            );
            var container = Container(TEXT_ANNOTATION_PART_ID);
            using (var reader = new StreamReader(container.GetStream()))
            {
                if (!reader.EndOfStream)
                {
                    _parser.Load(_graph, reader);
                }
            }

            return _graph;
        }

        private CustomXmlPart Container(string id)
        {
            if (_textAnnotationContainer != null)
            {
                return _textAnnotationContainer;
            }

            _mainPart.TryGetPartById(id, out var part);
            if (part == null)
            {
                _textAnnotationContainer = _mainPart.AddCustomXmlPart(
                    CustomXmlPartType.CustomXml, id
                );

                return _textAnnotationContainer;
            }
            else if (part is CustomXmlPart)
            {
                _textAnnotationContainer = (CustomXmlPart)part;
                return _textAnnotationContainer;
            }
            else
            {
                throw new ArgumentException(
                    $"a part with ID \"{id}\" is not a custom XML part"
                );
            }
        }
    }
}
