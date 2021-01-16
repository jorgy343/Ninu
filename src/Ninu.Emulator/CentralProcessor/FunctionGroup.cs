using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ninu.Emulator.CentralProcessor
{
    /// <summary>
    /// Represents various types of possible functions but only one function will be used. The
    /// constructor defines which type of function this object represents. Calling the <see
    /// cref="CallFunction(Cpu, IBus)"/> will call the appropriate function that was configured
    /// during the construction of this object.
    /// </summary>
    public unsafe struct FunctionGroup
    {
        private readonly delegate*<Cpu, IBus, void> _voidFunction { get; }
        private readonly delegate*<Cpu, IBus, bool, void> _boolFunction { get; }
        private readonly delegate*<Cpu, IBus, int, void> _intFunction { get; }

        private readonly bool _boolArgument { get; }
        private readonly int _intArgument { get; }

        public FunctionGroup(delegate*<Cpu, IBus, void> function)
        {
            _voidFunction = function;
            _boolFunction = null;
            _intFunction = null;

            _boolArgument = false;
            _intArgument = 0;
        }

        public FunctionGroup(delegate*<Cpu, IBus, bool, void> function, bool argument)
        {
            _voidFunction = null;
            _boolFunction = function;
            _intFunction = null;

            _boolArgument = argument;
            _intArgument = 0;
        }

        public FunctionGroup(delegate*<Cpu, IBus, int, void> function, int argument)
        {
            _voidFunction = null;
            _boolFunction = null;
            _intFunction = function;

            _boolArgument = false;
            _intArgument = argument;
        }

        public void CallFunction(Cpu cpu, IBus bus)
        {
            if (_voidFunction != null)
            {
                _voidFunction(cpu, bus);
            }
            else if (_boolFunction != null)
            {
                _boolFunction(cpu, bus, _boolArgument);
            }
            else if (_intFunction != null)
            {
                _intFunction(cpu, bus, _intArgument);
            }
        }
    }
}