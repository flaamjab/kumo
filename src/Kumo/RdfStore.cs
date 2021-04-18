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
        private CustomXmlPart? _container = null;
        private Graph? _graph = null;
        private bool _autoSave;

        private CustomXmlPart? Container
        {
            get
            {
                if (_container == null)
                {
                    _container = Load();
                }
                
                return _container;
            }
            set => _container = value;
        }

        private Graph Graph
        {
            get
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

                if (Container != null)
                {
                    using (var reader = new StreamReader(Container.GetStream()))
                    {
                        if (!reader.EndOfStream)
                        {
                            _parser.Load(_graph, reader);
                        }
                    }
                }

                return _graph;
            }
        }

        private RdfXmlWriter _writer;
        private RdfXmlParser _parser;

        public RdfStore(MainDocumentPart mainPart, bool autoSave)
        {
            _mainPart = mainPart;

            _writer = new RdfXmlWriter();
            _parser = new RdfXmlParser();

            _autoSave = autoSave;
        }

        public void Assert(Description description)
        {
            var triples = description.ToTriples(Graph);
            
            Console.WriteLine("The following triples will be added to the store:");
            foreach (var t in triples)
            {
                Console.WriteLine(t);
            }
            
            Graph.Assert(triples);

            if (_autoSave)
            {
                Save();
            }
        }

        public void Retract(string id)
        {
            throw new NotImplementedException();

            if (_autoSave)
            {
                Save();
            }
        }

        public Description Get(int id)
        {
            var subject = Graph.GetBlankNode(Schema.Prefixed(id));
            if (subject == null)
            {
                throw new InvalidOperationException(
                    $"no triples with ID \"{subject}\" exist"
                );
            }

            var triples = Graph.GetTriplesWithSubject(subject).ToArray();

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
            var subject = Graph.GetBlankNode(Schema.Prefixed(id));
            var triples = Graph.GetTriplesWithSubject(subject);

            if (triples.FirstOrDefault() == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void Save()
        {
            if (Container == null)
            {
                Container = NewContainer();
            }

            using (var ms = new MemoryStream())
            {
                var w = new StreamWriter(ms);
                _writer.Save(Graph, w, true);

                ms.Position = 0;
                Container.FeedData(ms);
            }

            Console.WriteLine("The following triples are present in the store:");
            foreach (var t in Graph.Triples)
            {
                Console.WriteLine(t);
            }
        }

        private CustomXmlPart? Load()
        {
            string id = TEXT_ANNOTATION_PART_ID;
            _mainPart.TryGetPartById(id, out var part);
            if (part is CustomXmlPart)
            {
                return (CustomXmlPart)part;
            }
            else if (part == null)
            {
                return null;
            }
            else
            {
                throw new ArgumentException(
                    $"a part with ID \"{id}\" is not a custom XML part"
                );
            }
        }

        private CustomXmlPart NewContainer()
        {
            return _mainPart.AddCustomXmlPart(
                CustomXmlPartType.CustomXml, TEXT_ANNOTATION_PART_ID
            );
        }
    }
}
