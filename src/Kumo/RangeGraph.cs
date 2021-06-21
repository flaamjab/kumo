using System;
using System.Linq;
using VDS.RDF;

namespace Kumo
{
    class RangeGraph
    {
        private IGraph _graph;

        public RangeGraph(IGraph graph)
        {
            _graph = graph;
        }

        public void Assert(Star link)
        {
            var triples = link.ToTriples(_graph);
            _graph.Assert(triples);
        }

        public void Retract(Star link)
        {
            var triples = link.ToTriples(_graph);
            _graph.Retract(triples);
        }

        public bool Exists(Uri uri)
        {
            var triples = _graph.GetTriplesWithSubject(uri);
            return triples.Count() > 0;
        }

        public Star Link(Uri uri)
        {
            var triples = _graph.GetTriplesWithSubject(uri);
            return Kumo.Star.FromTriples(uri, triples);
        }
    }
}
