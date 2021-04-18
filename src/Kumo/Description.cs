using System;
using System.Linq;
using VDS.RDF;

namespace Kumo
{
    /// <summary>
    ///   Represents a subject node, with all 
    ///   properties attached to it.
    /// </summary>
    class Description
    {
        public int Subject { get; }

        public Property[] Properties { get; }

        public int[] Crossrefs { get; }

        public static Description FromTriples(int subject, Triple[] triples)
        {
            var properties = triples
                .Where(t => t.Object.NodeType == NodeType.Uri)
                .Select(t => new Property(
                    ((IUriNode)t.Predicate).Uri,
                    ((IUriNode)t.Object).Uri
                ))
                .ToArray();

            var crossrefs = triples
                .Where(t => t.Object.NodeType == NodeType.Blank)
                .Select(t => Schema.Unprefixed(
                    ((IBlankNode)t.Object).InternalID)
                )
                .ToArray();

            return new Description(subject, properties, crossrefs);
        }

        public Description(int subject, Property[] properties, int[] crossrefs)
        {
            Subject = subject;
            Properties = properties;
            Crossrefs = crossrefs;
        }

        public Triple[] ToTriples(IGraph g)
        {
            var subject = g.CreateBlankNode(Schema.Prefixed(Subject));

            var propertyTriples = Properties.Select(p => 
                {
                    var pred = g.CreateUriNode(p.Name);
                    var obj = g.CreateUriNode(p.Value);
                    return new Triple(subject, pred, obj);
                }
            );

            var crossrefTriples = Crossrefs.Select(id =>
                {
                    var pred = g.CreateUriNode(Schema.QName(Schema.RefersTo));
                    var obj = g.CreateBlankNode(id.ToString());
                    return new Triple(subject, pred, obj);
                }
            );

            var triples = propertyTriples.Concat(crossrefTriples);

            return triples.ToArray();
        }
    }
}
