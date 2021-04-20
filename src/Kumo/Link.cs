using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;

namespace Kumo
{
    /// <summary>
    ///   Represents a subject node, with all 
    ///   properties attached to it.
    /// </summary>
    class Link
    {
        public int Subject { get; }

        public IEnumerable<Property> Properties { get; }

        public IEnumerable<int> Relations { get; }

        public static Link FromTriples(
            int subject,
            IEnumerable<Triple> triples)
        {
            var properties = triples
                .Where(t => t.Object.NodeType == NodeType.Uri)
                .Select(t => new Property(
                    ((IUriNode)t.Predicate).Uri,
                    ((IUriNode)t.Object).Uri
                ));

            var relations = triples
                .Where(t => t.Object.NodeType == NodeType.Blank)
                .Select(t => Schema.Unprefixed(
                    ((IBlankNode)t.Object).InternalID)
                );

            return new Link(subject, properties, relations);
        }

        public Link(
            int subject,
            IEnumerable<Property> properties,
            IEnumerable<int> relations)
        {
            Subject = subject;
            Properties = properties;
            Relations = relations;
        }

        public IEnumerable<Triple> ToTriples(IGraph g)
        {
            var subject = g.CreateBlankNode(Schema.Prefixed(Subject));

            var propertyTriples = Properties.Select(p => 
                {
                    var pred = g.CreateUriNode(p.Name);
                    var obj = g.CreateUriNode(p.Value);
                    return new Triple(subject, pred, obj);
                }
            );

            var crossrefTriples = Relations.Select(id =>
                {
                    var pred = g.CreateUriNode(Schema.QName(Schema.RefersTo));
                    var obj = g.CreateBlankNode(id.ToString());
                    return new Triple(subject, pred, obj);
                }
            );

            var triples = propertyTriples.Concat(crossrefTriples);

            return triples;
        }
    }
}
