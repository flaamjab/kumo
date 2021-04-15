using W = DocumentFormat.OpenXml.Wordprocessing;

namespace Kumo
{
    class NodeBlock
    {
        public W.Text[] Nodes { get; }

        public int Start { get; }

        public int End { get; }

        public NodeBlock(W.Text[] nodes, int start, int end)
        {
            Nodes = nodes;
            Start = start;
            End = end;
        }
    }
}
