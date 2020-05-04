// Some code ported from this license:

/*
 Copyright (c) 2010 Brian Silverman, Barry Silverman

 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:

 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.

 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 THE SOFTWARE.
*/

// JavaScript hash functions.

/*
var computeNodeHash = function()
{
    var hash = 5381|0;

    nodes.forEach(x => hash = (((hash|0) << (5|0)) + (hash|0))
        + ((x.state ? 1 : 2)|0)
        + ((x.pullup ? 3 : 4)|0)
        + ((x.pulldown ? 5 : 6)|0));

    return hash|0;
}

var computeTransistorHash = function()
{
    var hash = 5381|0;

    Object.keys(transistors).forEach(x => hash = (((hash|0) << (5|0)) + (hash|0)) + ((transistors[x].on ? 1 : 0)|0));

    return hash|0;
}
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Ninu.Visual6502
{
    public class Simulator
    {
        private readonly List<Node> _nodes = new List<Node>();
        private readonly Dictionary<int, Node> _nodesById = new Dictionary<int, Node>();
        private readonly Dictionary<string, Node> _nodesByName = new Dictionary<string, Node>();

        private readonly Dictionary<string, Transistor> _transistors = new Dictionary<string, Transistor>();

        private readonly HashSet<Node> _updates = new HashSet<Node>();

        private Node? _ground;
        private Node? _power;

        private const int GroundNumber = 558;
        private const int PowerNumber = 657;

        private readonly byte[] _memory = new byte[65536];

        public void SetMemory(ReadOnlySpan<byte> data, int offset = 0)
        {
            if (offset + data.Length > _memory.Length)
            {
                throw new InvalidOperationException();
            }

            data.CopyTo(_memory.AsSpan(offset, data.Length));
        }

        public void SetVectors(ushort nmiVector, ushort resetVector, ushort irqVector)
        {
            _memory[0xfffa] = (byte)((nmiVector & 0x00ff) >> 0);
            _memory[0xfffb] = (byte)((nmiVector & 0x00ff) >> 8);

            _memory[0xfffc] = (byte)((resetVector & 0x00ff) >> 0);
            _memory[0xfffd] = (byte)((resetVector & 0x00ff) >> 8);

            _memory[0xfffe] = (byte)((irqVector & 0x00ff) >> 0);
            _memory[0xffff] = (byte)((irqVector & 0x00ff) >> 8);
        }

        public void HalfStep()
        {
            var node = _nodesByName["clk0"];
            var clk = node.State;

            if (clk)
            {
                SetLow("clk0");

                HandleBusRead();
            }
            else
            {
                SetHigh("clk0");

                HandleBusWrite();
            }
        }

        public void ExecuteCycles(int cycleCount)
        {
            if (cycleCount < 0) throw new ArgumentOutOfRangeException(nameof(cycleCount));

            for (var i = 0; i < cycleCount; i++)
            {
                HalfStep();
                HalfStep();
            }
        }

        private void HandleBusRead()
        {
            if (_nodesByName["rw"].State)
            {
                var address = ReadAddressBus();

                var data = _memory[address & 0xffff];

                WriteDataBus(data);
            }
        }

        private void HandleBusWrite()
        {
            if (!_nodesByName["rw"].State)
            {
                var address = ReadAddressBus();
                var data = ReadDataBus();

                _memory[address & 0xffff] = (byte)(data & 0xff);
            }
        }

        public void Init()
        {
            _memory[0xfffe] = 0;
            _memory[0xffff] = 0;

            _memory[0xfffc] = 20;
            _memory[0xfffd] = 0;

            _memory[0xfffa] = 0;
            _memory[0xfffb] = 0;

            SetupNodes();
            SetupTransistors();

            var ground = _nodesById[GroundNumber];
            var power = _nodesById[PowerNumber];

            ground.State = false;
            ground.Floating = false;

            power.State = true;
            power.Floating = false;

            SetLow("res");
            SetLow("clk0");
            SetHigh("rdy");
            SetLow("so");
            SetHigh("irq");
            SetHigh("nmi");

            RecalcNodeList(_nodes
                .Where(x => x != _ground && x != _power)
                .Select(x => x));

            for (var i = 0; i < 8; i++)
            {
                SetHigh("clk0");
                SetLow("clk0");
            }

            SetHigh("res");
        }

        public void RunStartProgram()
        {
            for (var i = 0; i < 18; i++)
            {
                HalfStep();
            }
        }

        private void SetupNodes()
        {
            foreach (var nodeDefinition in NodeDefinitions.Definitions)
            {
                var node = new Node(nodeDefinition.W, nodeDefinition.PullUp, nodeDefinition.Name);

                _nodes.Add(node);
                _nodesById[node.Number] = node;

                if (node.Name != null)
                {
                    _nodesByName[node.Name] = node;
                }
            }

            _ground = _nodesById[GroundNumber];
            _power = _nodesById[PowerNumber];
        }

        private void SetupTransistors()
        {
            foreach (var transistorDefinition in TransistorDefinitions.Definitions)
            {
                var c1 = transistorDefinition.C1;
                var c2 = transistorDefinition.C2;

                if (c1 == GroundNumber)
                {
                    c1 = c2;
                    c2 = GroundNumber;
                }

                if (c1 == PowerNumber)
                {
                    c1 = c2;
                    c2 = PowerNumber;
                }

                var transistor = new Transistor(
                    transistorDefinition.Name,
                    _nodesById[transistorDefinition.Gate],
                    _nodesById[c1],
                    _nodesById[c2]);

                transistor.Gate.Gates.Add(transistor);

                _nodesById[c1].C1C2S.Add(transistor);
                _nodesById[c2].C1C2S.Add(transistor);

                _transistors[transistorDefinition.Name] = transistor;
            }
        }

        private void RecalcNodeList(IEnumerable<Node> nodeNumberList)
        {
            if (nodeNumberList == null) throw new ArgumentNullException(nameof(nodeNumberList));

            _updates.Clear();

            for (var i = 0; i < 100; i++)
            {
                if (!nodeNumberList.Any())
                {
                    return;
                }

                foreach (var nodeNumber in nodeNumberList)
                {
                    RecalcNode(nodeNumber);
                }

                nodeNumberList = _updates.ToArray();

                _updates.Clear();
            }
        }

        private void RecalcNode(Node node)
        {
            if (node == _ground || node == _power)
            {
                return;
            }

            var group = GetNodeGroup(node);
            var newState = GetNodeValue(group);

            foreach (var groupNode in group)
            {
                if (groupNode.State == newState)
                {
                    continue;
                }

                groupNode.State = newState;

                foreach (var gate in groupNode.Gates)
                {
                    if (groupNode.State)
                    {
                        TurnTransistorOn(gate);
                    }
                    else
                    {
                        TurnTransistorOff(gate);
                    }
                }
            }
        }

        private void TurnTransistorOn(Transistor transistor)
        {
            if (!transistor.On)
            {
                transistor.On = true;

                AddRecalcNode(transistor.C1);
            }
        }

        private void TurnTransistorOff(Transistor transistor)
        {
            if (transistor.On)
            {
                transistor.On = false;

                AddRecalcNode(transistor.C1);
                AddRecalcNode(transistor.C2);
            }
        }

        private void AddRecalcNode(Node node)
        {
            if (node != _ground && node != _power)
            {
                _updates.Add(node);
            }
        }

        private HashSet<Node> GetNodeGroup(Node node)
        {
            var group = new HashSet<Node>();

            AddNodeToGroup(group, node);

            return group;
        }

        private void AddNodeToGroup(HashSet<Node> group, Node node)
        {
            if (group.Contains(node))
            {
                return;
            }

            group.Add(node);

            if (node == _ground || node == _power)
            {
                return;
            }

            foreach (var transistor in node.C1C2S)
            {
                if (transistor.On)
                {
                    AddNodeToGroup(group, transistor.C1 == node ? transistor.C2 : transistor.C1);
                }
            }
        }

        private bool GetNodeValue(HashSet<Node> group)
        {
            if (group.Contains(_ground))
            {
                return false;
            }

            if (group.Contains(_power))
            {
                return true;
            }

            foreach (var node in group)
            {
                if (node.PullUp)
                {
                    return true;
                }

                if (node.PullDown)
                {
                    return false;
                }

                if (node.State)
                {
                    return true;
                }
            }

            return false;
        }

        private void SetHigh(string nodeName)
        {
            var node = _nodesByName[nodeName];

            node.PullUp = true;
            node.PullDown = false;

            RecalcNodeList(new[] { node });
        }

        private void SetLow(string nodeName)
        {
            var node = _nodesByName[nodeName];

            node.PullUp = false;
            node.PullDown = true;

            RecalcNodeList(new[] { node });
        }

        public int ComputeNodeHash()
        {
            var hash = 5381;

            foreach (var node in _nodes)
            {
                hash = ((hash << 5) + hash) + (node.State ? 1 : 2) + (node.PullUp ? 3 : 4) + (node.PullDown ? 5 : 6);
            }

            return hash;
        }

        public int ComputeTransistorHash()
        {
            var hash = 5381;

            foreach (var transistor in _transistors.Values)
            {
                hash = ((hash << 5) + hash) + (transistor.On ? 1 : 0);
            }

            return hash;
        }

        public int ReadBits(string namePrefix, int size)
        {
            var value = 0;

            for (var i = 0; i < size; i++)
            {
                value |= (_nodesByName[namePrefix + i].State ? 1 : 0) << i;
            }

            return value;
        }

        public int ReadAddressBus() => ReadBits("ab", 16);
        public int ReadDataBus() => ReadBits("db", 8);
        public int ReadPCLow() => ReadBits("pcl", 8);
        public int ReadPCHigh() => ReadBits("pch", 8);
        public int ReadA() => ReadBits("a", 8);
        public int ReadX() => ReadBits("x", 8);
        public int ReadY() => ReadBits("y", 8);

        public int ReadPC() => ReadPCLow() | (ReadPCHigh() << 8);

        public void WriteBits(string namePrefix, int size, int data)
        {
            var nodeRecalcs = new Node[size];

            for (var i = 0; i < size; i++)
            {
                var node = _nodesByName[namePrefix + i];

                nodeRecalcs[i] = node;

                if (((data >> i) & 0x1) == 0)
                {
                    node.PullUp = false;
                    node.PullDown = true;
                }
                else
                {
                    node.PullUp = true;
                    node.PullDown = false;
                }
            }

            RecalcNodeList(nodeRecalcs);
        }

        public void WriteDataBus(int data) => WriteBits("db", 8, data);
    }
}