using System;

namespace Kumo
{
    /// <summary>Represents an RDF property (predicate) with a value attached.</summary>
    public record Property
    {
        /// <summary>The property name.</summary>
        public Uri Name { get; }

        /// <summary>The property value.</summary>
        public Uri Value { get; }

        /// <summary>Creates a new property.</summary>
        /// <param name="name">The name of the property</param>
        /// <param name="value">The value of the property.</param>
        public Property(Uri name, Uri value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>Creates a new property.</summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property</param>
        /// <remarks>Both <c>name</c> and <c>value</c> must be valid URIs</remarks>
        public Property(string name, string value) :
            this(
                new Uri(name),
                new Uri(value)
            )
        { }
    }
}
