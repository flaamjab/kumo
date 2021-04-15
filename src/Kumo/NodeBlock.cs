using System;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace Kumo
{
    class NodeBlock
    {
        public int Start { get; }
        public int End { get; }

        public W.Text[] Nodes { get; }

        public NodeBlock(W.Text[] nodes, int start, int end)
        {
            Nodes = nodes;
            Start = start;
            End = end;
        }
    }
}
