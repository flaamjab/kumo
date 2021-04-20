using System;
using Xunit;
using VDS.RDF;
using VDS.RDF.Writing;

namespace Kumo.Tests
{
    public class DotNetRdfTests
    {
        [Fact]
        public void ToText_UriNode_ReturnsUriStringWithSlashAtEnd()
        {
            var g = new Graph();
            
            INode n = g.CreateUriNode(UriFactory.Create("http://example.org"));
            Assert.Equal("http://example.org/", n.ToString());
        }

        [Fact]
        public void ToText_BlankNode_ReturnsIdWithAnonymousNamespace()
        {
            var g = new Graph();
            INode n = g.CreateBlankNode("id");

            Assert.Equal("_:id", n.ToString());
        }

        [Fact]
        public void ToText_BlankNode_InternalIdIsOriginalId()
        {
            var g = new Graph();
            IBlankNode n = g.CreateBlankNode("id");

            Assert.Equal("id", n.InternalID);
        }

        [Fact]
        public void Write_WellFormedGraphWithStringWriter_Works()
        {
            var g = new Graph();
            g.NamespaceMap.AddNamespace(
                "rel", new Uri("https://example.org/relations")
            );

            INode subject = g.CreateUriNode(new Uri(
                "http://dbpedia.org/resource/Perm"
            ));
            INode predicate = g.CreateUriNode(new Uri(
                "https://kumo.org/InstanceOf"
            ));
            INode obj = g.CreateUriNode(new Uri(
                "http://ontology.ontotext.com/taxonomy/Location"
            ));

            g.Assert(subject, predicate, obj);
            
            var writer = new RdfXmlWriter();

            var result = StringWriter.Write(g, writer);
            Console.WriteLine(result);
        }

        [Fact]
        public void Equals_TwoNodesWithSameUri_True()
        {
            var g = new Graph();

            var uri = new Uri("http://example.org");
            var nodeA = g.CreateUriNode(uri);
            var nodeB = g.CreateUriNode(uri);

            Assert.Equal(nodeA, nodeB);
        }

        [Fact]
        public void Equals_TwoBlankNodesWithSameId_True()
        {
            var g = new Graph();
            
            var id = "id";
            var nodeA = g.CreateBlankNode(id);
            var nodeB = g.CreateBlankNode(id);

            Assert.Equal(nodeA, nodeB);
        }

        [Fact]
        public void Equals_TwoTriplesWithMatchingNodes_True()
        {
            var g = new Graph();

            var id = "id";
            var uri = new Uri("http://example.org");

            var subj = g.CreateBlankNode(id);
            var pred = g.CreateUriNode(uri);
            var obj = g.CreateUriNode(uri);

            var tripleA = new Triple(subj, pred, obj);
            var tripleB = new Triple(subj, pred, obj);

            Assert.Equal(tripleA, tripleB);
        }

        [Fact]
        public void Assert_DuplicateTriple_TriplesAssertedOnce()
        {
            var g = new Graph();
            
            var subj = g.CreateUriNode(new Uri("http://example.org/subject"));
            var pred = g.CreateUriNode(
                new Uri("http://example.org/predicate")
            );
            var obj = g.CreateUriNode(new Uri("http://example.org/object"));

            g.Assert(subj, pred, obj);
            g.Assert(subj, pred, obj);

            Assert.Single(g.Triples);
        }
    }
}
