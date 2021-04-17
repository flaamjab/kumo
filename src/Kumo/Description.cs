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
        public int Id { get; }
        public Property[] Properties { get; }

        public Description(int id, Triple[] triples)
        {
            Id = id;

            if (!triples.All(t => t.Subject.ToString() == id.ToString()))
            {
                throw new ArgumentException(
                    "all triples must have the same subject ID"
                );
            }

            Properties = triples.Select(t => new Property(
                ((IUriNode)t.Predicate).Uri,
                t.Object.ToString()
            )).ToArray();
        }

        public Triple[] ToTriples(INodeFactory f)
        {
            var subject = f.CreateBlankNode(Id.ToString());
            var triples = Properties.Select(p =>
                {
                    var pred = f.CreateUriNode(p.Name);
                    if (Uri.IsWellFormedUriString(p.Value, UriKind.Absolute))
                    {
                        var obj = f.CreateUriNode(new Uri(p.Value));
                        return new Triple(subject, pred, obj);
                    }
                    else
                    {
                        var obj = f.CreateBlankNode(p.Value);
                        return new Triple(subject, pred, obj);
                    }
                }
            );

            return triples.ToArray();
        }
    }
}
