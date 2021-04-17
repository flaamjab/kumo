using System;

namespace Kumo
{
    /// <summary>Represents an RDF property with a value attached</summary>
    public record Property
    {
        public Uri Name { get; }
        public string Value { get; }

        /// <summary>
        ///   Creates a new property with <c>name</c> and <c>value</c>.
        /// </summary>
        public Property(Uri name, string value)
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
                value
            )
        { }
    }
}
