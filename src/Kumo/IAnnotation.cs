namespace Kumo
{
    public interface IAnnotation
    {
        public IRange Range { get; }

        public Property[] Properties { get; }
    }
}
