using System;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace Kumo
{
    /// <summary>Represents a node block.</summary>
    class Block
    {
        public int Start { get; }
        public int End { get; }

        public W.Text[] Nodes { get; }

        public Block(W.Text[] nodes, int start, int end)
        {
            Nodes = nodes;
            Start = start;
            End = end;
        }
    }
}
