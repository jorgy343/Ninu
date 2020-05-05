using System.Runtime.CompilerServices;

namespace Ninu.Visual6502
{
    public class UniqueNodeList
    {
        public Node[] Nodes1 = new Node[1725];
        public int NodeInsertIndex1;
        public bool[] NodesInList1 = new bool[1725];

        public Node[] Nodes2 = new Node[1725];
        public int NodeInsertIndex2;
        public bool[] NodesInList2 = new bool[1725];

        /// <summary>
        /// Swaps the current arrays with the previous arrays.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Swap()
        {
            (Nodes1, Nodes2) = (Nodes2, Nodes1);
            (NodesInList1, NodesInList2) = (NodesInList2, NodesInList1);
            (NodeInsertIndex1, NodeInsertIndex2) = (NodeInsertIndex2, NodeInsertIndex1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void AddToCurrent(Node node)
        {
            if (!NodesInList1[node.Number])
            {
                Nodes1[NodeInsertIndex1++] = node;
                NodesInList1[node.Number] = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public bool CurrentHasNode(Node node)
        {
            return NodesInList1[node.Number];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void ClearCurrent()
        {
            for (var i = 0; i < NodeInsertIndex1; i++)
            {
                NodesInList1[Nodes1[i].Number] = false;
            }

            NodeInsertIndex1 = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void AddToPrevious(Node node)
        {
            if (!NodesInList2[node.Number])
            {
                Nodes2[NodeInsertIndex2++] = node;
                NodesInList2[node.Number] = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public bool PreviousHasNode(Node node)
        {
            return NodesInList2[node.Number];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void ClearPrevious()
        {
            for (var i = 0; i < NodeInsertIndex2; i++)
            {
                NodesInList2[Nodes2[i].Number] = false;
            }

            NodeInsertIndex2 = 0;
        }
    }
}