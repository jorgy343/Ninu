﻿// Some code ported from this license:

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

using Ninu.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Ninu.Visual6502
{
    public class Simulator
    {
        private readonly List<Node> _nodes = new();
        private readonly Dictionary<int, Node> _nodesById = new();
        private readonly Dictionary<string, Node> _nodesByName = new();

        private readonly Dictionary<string, Transistor> _transistors = new();

        private readonly UniqueNodeList _updates = new();
        private readonly UniqueNodeList _group = new();

#nullable disable
        private Node _groundNode;
        private Node _powerNode;

        private Node _rwNode;
#nullable restore

        private readonly Node[] _abNodes = new Node[16];
        private readonly Node[] _dbNodes = new Node[8];

        public IMemory Memory { get; }

        public Simulator()
            : this(new ArrayMemory(65536))
        {

        }

        public Simulator(IMemory memory)
        {
            Memory = memory ?? throw new ArgumentNullException(nameof(memory));
        }

        public void SetVectors(ushort nmiVector, ushort resetVector, ushort irqVector)
        {
            Memory[0xfffa] = (byte)((nmiVector & 0x00ff) >> 0);
            Memory[0xfffb] = (byte)((nmiVector & 0x00ff) >> 8);

            Memory[0xfffc] = (byte)((resetVector & 0x00ff) >> 0);
            Memory[0xfffd] = (byte)((resetVector & 0x00ff) >> 8);

            Memory[0xfffe] = (byte)((irqVector & 0x00ff) >> 0);
            Memory[0xffff] = (byte)((irqVector & 0x00ff) >> 8);
        }

        public void HalfClock()
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

        public void Clock()
        {
            HalfClock();
            HalfClock();
        }

        public void ExecuteCycles(int cycleCount)
        {
            if (cycleCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cycleCount));
            }

            for (var i = 0; i < cycleCount; i++)
            {
                Clock();
            }
        }

        private void HandleBusRead()
        {
            if (_rwNode.State)
            {
                var address = ReadAddressBus();

                var data = Memory[(ushort)(address & 0xffff)];

                WriteDataBus(data);
            }
        }

        private void HandleBusWrite()
        {
            if (!_rwNode.State)
            {
                var address = ReadAddressBus();
                var data = ReadDataBus();

                Memory[(ushort)(address & 0xffff)] = (byte)(data & 0xff);
            }
        }

        /// <summary>
        /// This method must be called before the simulator can run cycles. This method sets up the
        /// simulator to the point where the start program can run.
        /// </summary>
        /// <param name="cycleCallback">Optionally, a callback that is called every time after the simulator is clocked during the init routine.</param>
        public void Init(Action? cycleCallback = null)
        {
            SetupNodes();
            SetupTransistors();

            _groundNode.State = false;
            _groundNode.Floating = false;

            _powerNode.State = true;
            _powerNode.Floating = false;

            SetLow("res");
            SetLow("clk0");
            SetHigh("rdy");
            SetLow("so");
            SetHigh("irq");
            SetHigh("nmi");

            RecalcNodeList(_nodes
                .Where(x => x != _groundNode && x != _powerNode));

            // These clocks are just to stablize the CPU and get it into a valid configuration. You
            // can actually increase the number of cycles and it won't have any affect because the
            // reset pin is held high right after causing teh CPU to begin it's reset routine.
            for (var i = 0; i < 8; i++)
            {
                SetHigh("clk0");
                SetLow("clk0");

                cycleCallback?.Invoke();
            }

            SetHigh("res");
        }

        /// <summary>
        /// This method must be called after <see cref="Init"/> and before the simulator can run cycles. This method
        /// runs the start program which primes the first instruction that the reset vector points to.
        /// </summary>
        /// <param name="cycleCallback">Optionally, a callback that is called every time after the simulator is clocked during the start routine.</param>
        public void RunStartProgram(Action? cycleCallback = null)
        {
            for (var i = 0; i < 9; i++)
            {
                Clock();

                cycleCallback?.Invoke();
            }

            // We have to do an additional half clock here in order to get everything synced up.
            // I'm not really sure why but doing this matches how the JS version of Visual 6502
            // works. When the JS version loads, it presents you with a row of the current state of
            // the CPU which is indicated as cycle 0. This is really the status of the CPU after
            // the last half cycle of the start program. The next step that you take in the
            // simulation then does another half cycle and it shows up as cycle 0 which is what
            // this half clock below is doing.
            HalfClock();
        }

        private void SetupNodes()
        {
            foreach (var nodeDefinition in NodeDefinitions.Definitions)
            {
                var node = new Node(nodeDefinition.W, nodeDefinition.PullUp, nodeDefinition.Name);

                _nodes.Add(node);
                _nodesById[node.Number] = node;

                if (node.Name is not null)
                {
                    _nodesByName[node.Name] = node;
                }
            }

            _groundNode = _nodesById[558];
            _powerNode = _nodesById[657];

            _rwNode = _nodesByName["rw"];

            for (var i = 0; i < 16; i++)
            {
                _abNodes[i] = _nodesByName["ab" + i];
            }

            for (var i = 0; i < 8; i++)
            {
                _dbNodes[i] = _nodesByName["db" + i];
            }
        }

        private void SetupTransistors()
        {
            foreach (var transistorDefinition in TransistorDefinitions.Definitions)
            {
                var c1 = transistorDefinition.C1;
                var c2 = transistorDefinition.C2;

                if (c1 == _groundNode.Number)
                {
                    c1 = c2;
                    c2 = _groundNode.Number;
                }

                if (c1 == _powerNode.Number)
                {
                    c1 = c2;
                    c2 = _powerNode.Number;
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

        private void RecalcNodeList(IEnumerable<Node> nodeList)
        {
            if (nodeList is null)
            {
                throw new ArgumentNullException(nameof(nodeList));
            }

            _updates.ClearCurrent();
            _updates.ClearPrevious();

            foreach (var node in nodeList)
            {
                _updates.AddToCurrent(node);
            }

            for (var i = 0; i < 100; i++)
            {
                if (_updates.NodeInsertIndex1 == 0) // If there are no items in the list.
                {
                    return;
                }

                for (var j = 0; j < _updates.NodeInsertIndex1; j++)
                {
                    RecalcNode(_updates.Nodes1[j]);
                }

                _updates.Swap();
                _updates.ClearPrevious();
            }
        }

        private void RecalcNodeList(Node node)
        {
            if (node is null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            _updates.ClearCurrent();
            _updates.ClearPrevious();

            _updates.AddToCurrent(node);

            for (var i = 0; i < 100; i++)
            {
                if (_updates.NodeInsertIndex1 == 0) // If there are no items in the list.
                {
                    return;
                }

                for (var j = 0; j < _updates.NodeInsertIndex1; j++)
                {
                    RecalcNode(_updates.Nodes1[j]);
                }

                _updates.Swap();
                _updates.ClearPrevious();
            }
        }

        private void RecalcNode(Node node)
        {
            if (node == _groundNode || node == _powerNode)
            {
                return;
            }

            _group.ClearCurrent();

            AddNodeToGroup(node);
            var newState = GetNodeValue();

            for (var i = 0; i < _group.NodeInsertIndex1; i++)
            {
                var groupNode = _group.Nodes1[i];

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
            if (node != _groundNode && node != _powerNode)
            {
                _updates.AddToPrevious(node);
            }
        }

        private void AddNodeToGroup(Node node)
        {
            // Don't do anything if the node has already been added to the group.
            if (_group.CurrentHasNode(node))
            {
                return;
            }

            _group.AddToCurrent(node);

            if (node == _groundNode || node == _powerNode)
            {
                return;
            }

            foreach (var transistor in node.C1C2S)
            {
                if (transistor.On)
                {
                    AddNodeToGroup(transistor.C1 == node ? transistor.C2 : transistor.C1);
                }
            }
        }

        private bool GetNodeValue()
        {
            if (_group.CurrentHasNode(_groundNode))
            {
                return false;
            }

            if (_group.CurrentHasNode(_powerNode))
            {
                return true;
            }

            for (var i = 0; i < _group.NodeInsertIndex1; i++)
            {
                var node = _group.Nodes1[i];

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

            RecalcNodeList(node);
        }

        private void SetLow(string nodeName)
        {
            var node = _nodesByName[nodeName];

            node.PullUp = false;
            node.PullDown = true;

            RecalcNodeList(node);
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

        public int ReadBit(string name)
        {
            return _nodesByName[name].State ? 1 : 0;
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

        public int ReadBits8(string namePrefix)
        {
            return
                (_nodesByName[namePrefix + 0].State ? 1 : 0) << 0
                | (_nodesByName[namePrefix + 1].State ? 1 : 0) << 1
                | (_nodesByName[namePrefix + 2].State ? 1 : 0) << 2
                | (_nodesByName[namePrefix + 3].State ? 1 : 0) << 3
                | (_nodesByName[namePrefix + 4].State ? 1 : 0) << 4
                | (_nodesByName[namePrefix + 5].State ? 1 : 0) << 5
                | (_nodesByName[namePrefix + 6].State ? 1 : 0) << 6
                | (_nodesByName[namePrefix + 7].State ? 1 : 0) << 7;
        }

        public int ReadBits(Node[] nodes)
        {
            var value = 0;

            for (var i = 0; i < nodes.Length; i++)
            {
                value |= (nodes[i].State ? 1 : 0) << i;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public int ReadAddressBus() => ReadBits(_abNodes);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public int ReadDataBus() => ReadBits(_dbNodes);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public int ReadPCLow() => ReadBits8("pcl");

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public int ReadPCHigh() => ReadBits8("pch");

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public int ReadA() => ReadBits8("a");

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public int ReadX() => ReadBits8("x");

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public int ReadY() => ReadBits8("y");

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public int ReadS() => ReadBits8("s");

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public int ReadPC() => ReadPCLow() | (ReadPCHigh() << 8);

        public int ReadP()
        {
            var c = ReadBit("p0") << 0; // Carry
            var z = ReadBit("p1") << 1; // Zero
            var i = ReadBit("p2") << 2; // Disable Interrupts
            var d = ReadBit("p3") << 3; // Decimal Mode
            var v = ReadBit("p6") << 6; // Zero
            var n = ReadBit("p7") << 7; // Negative

            return c | z | i | d | v | n;
        }

        public string ReadPString()
        {
            var c = ReadBit("p0"); // Carry
            var z = ReadBit("p1"); // Zero
            var i = ReadBit("p2"); // Disable Interrupts
            var d = ReadBit("p3"); // Decimal Mode
            var v = ReadBit("p6"); // Zero
            var n = ReadBit("p7"); // Negative

            return $"{(n == 1 ? "N" : "n")}{(v == 1 ? "V" : "v")}--{(d == 1 ? "D" : "d")}{(i == 1 ? "I" : "i")}{(z == 1 ? "Z" : "z")}{(c == 1 ? "C" : "c")}";
        }

        public void WriteBit(string name, bool data)
        {
            var nodeRecalcs = new Node[1];

            var node = _nodesByName[name];
            nodeRecalcs[0] = node;

            if (!data)
            {
                node.PullUp = false;
                node.PullDown = true;
            }
            else
            {
                node.PullUp = true;
                node.PullDown = false;
            }

            RecalcNodeList(nodeRecalcs);
        }

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

        public void WriteDataBus(int data)
        {
            for (var i = 0; i < 8; i++)
            {
                var node = _dbNodes[i];

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

            RecalcNodeList(_dbNodes);
        }
    }
}