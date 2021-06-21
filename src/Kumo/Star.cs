using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;

namespace Kumo
{
    /// <summary>
    ///   Represents the links between the subject node and
    ///   its properties.
    /// </summary>
    class Star
    {
        public Uri Subject { get; }

        public IEnumerable<Property> Properties { get; }

        public static Star FromTriples(
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
                        UriNode n => new Resource.Unique(n.Uri),
                        var n => throw new ArgumentException(
                            $"Unsupported node type (node {n}) "
                            + $"in triple {t}"
                        )
                    }
                ));

            return new Star(subject, properties);
        }

        public Star(
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
                        Resource.Unique t => g.CreateUriNode(t.Uri),
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
