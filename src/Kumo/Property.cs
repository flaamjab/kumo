using System;

namespace Kumo
{
    /// <summary>Represents an RDF property with a value attached</summary>
    public record Property
    {
        public Uri Name { get; }
        public Uri Value { get; }

        /// <summary>
        ///   Creates a new property with <c>name</c> and <c>value</c>.
        /// </summary>
        public Property(Uri name, Uri value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        ///   Creates a new property with <c>name</c> and <c>value</c>.
        /// </summary>
        public Property(string name, string value) :
            this(
                new Uri(name),
                new Uri(value)
            )
        { }
    }
}
