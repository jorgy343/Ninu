namespace Ninu.Emulator.Mappers
{
    public abstract class Mapper
    {
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