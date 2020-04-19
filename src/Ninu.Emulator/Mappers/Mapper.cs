using Microsoft.Extensions.Logging;
using System;

namespace Ninu.Emulator.Mappers
{
    public abstract class Mapper
    {
        protected ILogger Logger { get; }

        public int ProgramRomBankCount { get; }
        public int PatternRomBankCount { get; }

        protected Mapper(int programRomBankCount, int patternRomBankCount, ILogger logger)
        {
            if (programRomBankCount < 0) throw new ArgumentOutOfRangeException(nameof(programRomBankCount));
            if (patternRomBankCount < 0) throw new ArgumentOutOfRangeException(nameof(patternRomBankCount));

            ProgramRomBankCount = programRomBankCount;
            PatternRomBankCount = patternRomBankCount;

            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public virtual bool GetMirrorMode(out NameTableMirrorMode mirrorMode)
        {
            mirrorMode = NameTableMirrorMode.Horizontal;

            return false;
        }

        public abstract bool HandleWrite(ushort address, byte data);

        public abstract bool TranslateProgramRomAddress(ushort address, out ushort translatedAddress);

        public abstract bool TranslatePatternRomAddress(ushort address, out ushort translatedAddress);
    }
}