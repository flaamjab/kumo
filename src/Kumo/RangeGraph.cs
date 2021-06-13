using System;
using System.Linq;
using VDS.RDF;

namespace Kumo
{
    class RangeGraph
    {
        public static readonly Uri Uri = new Uri(
            "https://kumo.org/graph/rangeGraph"
        );

        private IGraph _graph;

        public RangeGraph(IGraph graph)
        {
            _graph = graph;
        }

        public void Assert(Link link)
        {
            var triples = link.ToTriples(_graph);

            _graph.Assert(triples);
        }

        public void Retract(Link link)
        {
            var triples = link.ToTriples(_graph);
            _graph.Retract(triples);
        }

        public bool Exists(Uri uri)
        {
            var triples = _graph.GetTriplesWithSubject(uri);
            return triples.Count() > 0;
        }

        public Link Link(Uri uri)
        {
            var triples = _graph.GetTriplesWithSubject(uri);
            return Kumo.Link.FromTriples(uri, triples);
        }

        public void MergeInto(ITripleStore store)
        {
            store.Add(_graph, true);
        }
    }
}
