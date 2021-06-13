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
        private bool _autoSave;
        private RangeGraph _rangeGraph;

        public RangeGraph RangeGraph => _rangeGraph;

        public RdfStore(bool autoSave)
        {
            _tripleStore = new TripleStore();
            _autoSave = autoSave;

            var g = new Graph();
            _rangeGraph = new RangeGraph(g);
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
                _rangeGraph = new RangeGraph(g);
            }

            _tripleStore = store;
        }

        public void Save(Stream stream)
        {
            var w = new NQuadsWriter();
            var sw = new StreamWriter(stream);

            _rangeGraph.MergeInto(_tripleStore);

            w.Save(_tripleStore, sw);
        }
    }
}
