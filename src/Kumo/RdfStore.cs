using System;
using System.IO;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace Kumo
{
    class RdfStore : IDisposable
    {
        public const string ID = "kumo-rdf-store";

        private TripleStore _tripleStore;
        private IGraph _rangeGraph;

        public RangeGraph RangeGraph => new RangeGraph(_rangeGraph);

        public RdfStore()
        {
            _tripleStore = new TripleStore();

            _rangeGraph = new Graph();
            _rangeGraph.BaseUri = RangeGraph.Uri;
            _tripleStore.Add(_rangeGraph);
        }

        public void Dispose()
        {
            _tripleStore.Dispose();
        }

        public void AddGraph(Stream stream)
        {
            var p = new NTriplesParser();
            var g = new Graph();
            var sr = new StreamReader(stream);
            p.Load(g, sr);

            _tripleStore.Add(g);
        }

        public void RemoveGraph(Uri uri)
        {
            if (uri == RangeGraph.Uri)
            {
                throw new ArgumentException(
                    "The range graph cannot be removed"
                );
            }

            _tripleStore.Remove(uri);
        }

        public void Load(Stream stream)
        {
            var store = new TripleStore();

            var p = new NQuadsParser();
            var sr = new StreamReader(stream);
            p.Load(store, sr);

            if (store.HasGraph(RangeGraph.Uri))
            {
                var g = store[RangeGraph.Uri];
                g.Merge(_rangeGraph);
                _rangeGraph = g;
            }
            else
            {
                store.Add(_rangeGraph);
            }

            _tripleStore = store;
        }

        public void Save(Stream stream)
        {
            var w = new NQuadsWriter();
            var sw = new StreamWriter(stream);

            w.Save(_tripleStore, sw);
        }
    }
}
