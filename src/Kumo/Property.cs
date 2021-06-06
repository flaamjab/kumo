using System;

namespace Kumo
{
    /// <summary>Represents an RDF property with a value attached.</summary>
    public record Property
    {
        /// <summary>The property name.</summary>
        public Uri Name { get; }

        /// <summary>The property value.</summary>
        public Resource Value { get; }

        /// <summary>Creates a new property.</summary>
        /// <param name="name">The name of the property</param>
        /// <param name="value">The value of the property.</param>
        public Property(Uri name, Resource value)
        {
            Name = name;
            Value = value;
        }
    }
}
