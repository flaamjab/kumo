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
        public Uri Subject { get; }

        public IEnumerable<Property> Properties { get; }

        public static Link FromTriples(
            Uri subject,
            IEnumerable<Triple> triples)
        {
            var properties = triples
                .Where(t => t.Object.NodeType == NodeType.Uri)
                .Select(t => new Property(
                    ((IUriNode)t.Predicate).Uri,
                    t.Object switch
                    {
                        LiteralNode n => new Resource.Literal(
                            n.Value, n.DataType, n.Language
                        ),
                        UriNode n => new Resource.Thing(n.Uri),
                        var n => throw new ArgumentException(
                            $"Unsupported node type (node {n}) "
                            + $"in triple {t}"
                        )
                    }
                ));

            return new Link(subject, properties);
        }

        public Link(
            Uri subject,
            IEnumerable<Property> properties)
        {
            Subject = subject;
            Properties = properties;
        }

        public IEnumerable<Triple> ToTriples(INodeFactory g)
        {
            var subject = g.CreateUriNode(Subject);

            var triples = Properties.Select(p => 
                {
                    var pred = g.CreateUriNode(p.Name);
                    INode obj = p.Value switch
                    {
                        Resource.Thing t => g.CreateUriNode(t.Uri),
                        Resource.Literal L => g.CreateLiteralNode(
                            L.Value, L.Datatype
                        ),
                        _ => throw new ArgumentException(
                            $"Property {p} has a value of unsupported type"
                            + $"{p.GetType()}"
                        )
                    };
                    return new Triple(subject, pred, obj);
                }
            );

            return triples;
        }
    }
}
