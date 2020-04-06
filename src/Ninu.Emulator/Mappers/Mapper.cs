namespace Ninu.Emulator.Mappers
{
    public abstract class Mapper
    {
        public abstract bool TranslateProgramRomAddress(ushort address, out ushort translatedAddress);

        public abstract bool TranslatePatternRomAddress(ushort address, out ushort translatedAddress);
    }
}