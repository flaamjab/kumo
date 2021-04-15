using System;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace Kumo
{
    class NodeBlock
    {
        private int _start;
        private int _end;

        public W.Text[] Nodes { get; }

        public NodeBlock(W.Text[] nodes, int start, int end)
        {
            Nodes = nodes;
            _start = start;
            _end = end;
        }

        public (int, int) Offsets(Range range)
        {
            if (range.Start < _start || _end < range.End
                || _end < range.Start)
            {
                throw new ArgumentOutOfRangeException(
                    "The Range must be contained within the block"
                );
            }

            int leftOffset = range.Start - _start;
            int rightOffset = _end - range.End;

            return (leftOffset, rightOffset);
        }
    }
}
