using System;

/// <summary>Represents the object of an RDF statement.</summary>
public abstract record Resource
{
    private Resource() { }

    /// <summary>Represents an RDF literal value.</summary>
    /// <param name="Value">
    ///   The lexical form of the literal
    ///   as specified in the RDF 1.1 spec.
    /// </param>
    /// <param name="Datatype">The datatype URI of the literal.</param>
    /// <param name="Language">The language tag if present.</param>
    public sealed record Literal(
        string Value,
        Uri Datatype,
        string Language = "") : Resource
    {
        private static readonly Uri _uri = new Uri(
            "http://www.w3.org/2001/XMLSchema#string"
        );

        /// <summary>Gets the lexical form.</summary>
        public string Value { get; } = Value;

        /// <summary>Gets the datatype URI.</summary>
        public Uri Datatype { get; } = Datatype;

        /// <summary>Gets the language string.</summary>
        public string Language { get; } = Language;

        /// <summary>Creates a new string literal.</summary>
        /// <param name="value">The value of the literal.</param>
        public Literal(string value) : this(value, _uri) { }
    };

    /// <summary>Represents a resource identified by a URI.</summary>
    public sealed record Unique(Uri Uri) : Resource
    {
        /// <summary>
        ///   Creates a new <c>Thing</c> identified by the provided URI string.
        /// </summary>
        /// <exception>
        ///   Throws <c>UriFormatException</c> if the provided
        ///   URI string is invalid.
        /// </exception>
        public Unique(string uri) : this(new Uri(uri)) { }
    }
}