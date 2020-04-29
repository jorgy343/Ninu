namespace Ninu.Emulator
{
    /// <summary>
    /// Represents a 8-bit data bus with a 16-bit address.
    /// </summary>
    public interface IBus
    {
        public byte Read(ushort address);

        public void Write(ushort address, byte data);
    }
}