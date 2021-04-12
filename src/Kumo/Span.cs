using DocumentFormat.OpenXml.Wordprocessing;

namespace Kumo
{
    class Span
    {
        public Text[] Nodes { get; }

        public int LeftOffset { get; }

        public int RightOffset { get; }

        public Span(Text[] nodes, int leftOffset, int rightOffset)
        {
            Nodes = nodes;
            LeftOffset = leftOffset;
            RightOffset = rightOffset;
        }
    }
}
