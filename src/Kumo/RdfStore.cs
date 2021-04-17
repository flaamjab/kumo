#nullable enable

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

        public void Assert(Description star)
        {
            throw new NotImplementedException();
        }

        public void Retract(string id)
        {
            throw new NotImplementedException();
        }

        public Description? Get(int id)
        {
            var g = Graph();
            var subject = g.GetBlankNode(id.ToString());
            var triples = g.GetTriplesWithSubject(subject).ToArray();

            if (triples.Length > 0)
            {
                return new Description(id, triples);
            }
            else
            {
                return null;
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
                "kumo",
                Schema.Namespace
            );
            var container = Container(TEXT_ANNOTATION_PART_ID);
            var reader = new StreamReader(container.GetStream());
            _parser.Load(_graph, reader);

            return _graph;
        }

        private CustomXmlPart Container(string id)
        {
            if (_textAnnotationContainer != null)
            {
                return _textAnnotationContainer;
            }

            var part = _mainPart.GetPartById(id);
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
                var stream = new StreamReader(part.GetStream());
                
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
