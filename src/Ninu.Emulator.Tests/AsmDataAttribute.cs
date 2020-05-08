using Patcher6502;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace Ninu.Emulator.Tests
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class AsmDataAttribute : DataAttribute
    {
        private readonly string _filename;

        public AsmDataAttribute(string filename)
        {
            _filename = filename ?? throw new ArgumentNullException(nameof(filename));
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var parameters = testMethod.GetParameters();
            var parameterTypes = parameters.Select(x => x.ParameterType).ToArray();

            if (!File.Exists(_filename))
            {
                throw new Exception();
            }

            var asm = File.ReadAllText(_filename);

            var assembler = new PatchAssembler();
            var simulationMemory = assembler.Assemble(0, null, asm);

            yield return new object[] { simulationMemory };
        }
    }
}