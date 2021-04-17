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
            if (!triples.All(t => t.Subject.ToString() == subject.ToString()))
            {
                throw new ArgumentException(
                    "all triples must have the same subject ID"
                );
            }

            var properties = triples
                .Where(t => t.Object.NodeType == NodeType.Uri)
                .Select(t => new Property(
                    ((IUriNode)t.Predicate).Uri,
                    ((IUriNode)t.Object).Uri
                ))
                .ToArray();

            var crossrefs = triples
                .Where(t => t.Subject.NodeType == NodeType.Blank)
                .Select(t => int.Parse(t.Object.ToString()))
                .ToArray();

            return new Description(subject, properties, crossrefs);
        }

        public Description(int subject, Property[] properties, int[] crossrefs)
        {
            Subject = subject;
            Properties = properties;
            Crossrefs = crossrefs;
        }

        public Triple[] ToTriples(INodeFactory f)
        {
            var subject = f.CreateBlankNode(Subject.ToString());

            var propertyTriples = Properties.Select(p => 
                {
                    var pred = f.CreateUriNode(p.Name);
                    var obj = f.CreateUriNode(p.Value);
                    return new Triple(subject, pred, obj);
                }
            );

            var crossrefTriples = Crossrefs.Select(id =>
                {
                    var pred = f.CreateUriNode(Schema.Uri(Schema.RefersTo));
                    var obj = f.CreateBlankNode(id.ToString());
                    return new Triple(subject, pred, obj);
                }
            );

            var triples = propertyTriples.Concat(crossrefTriples);

            return triples.ToArray();
        }
    }
}
