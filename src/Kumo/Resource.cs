using System;

/// <summary>Represents the object of an RDF statement.</summary>
public abstract record Resource
{
    private Resource() { }

    /// <summary>Represents an RDF literal value</summary>
    public sealed record Literal(
        string Value,
        string Language,
        Uri Datatype) : Resource
    {
        /// <summary>Creates a new <c>Literal</c> with default datatype.</summary>
        public Literal(string value, string language) : this(
            value, language, new Uri(""))
        { }
    };

    /// <summary>Represents a resource identified by a URI.</summary>
    public sealed record Thing(Uri Uri) : Resource
    {
        /// <summary>
        ///   Creates a new <c>Thing</c> identified by the provided URI string.
        /// </summary>
        /// <exception>
        ///   Throws <c>UriFormatException</c> if the provided
        ///   URI string is invalid.
        /// </exception>
        public Thing(string uri) : this(new Uri(uri)) { }
    }
}