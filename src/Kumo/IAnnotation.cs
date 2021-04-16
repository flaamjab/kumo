namespace Kumo
{
    /// <summary>
    ///   Exposes an annotation, which is metadata,
    ///   attached to a text range.
    /// </summary>
    public interface IAnnotation
    {
        public IRange Subject { get; }

        public Property[] Properties { get; }

        public IRange[] Crossrefs { get; }
    }
}
