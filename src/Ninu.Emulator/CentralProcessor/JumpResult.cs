namespace Ninu.Emulator.CentralProcessor
{
    public struct JumpResult
    {
        public JumpResult(JumpType jumpType, ushort source, ushort destination, bool taken)
        {
            JumpType = jumpType;
            Source = source;
            Destination = destination;
            Taken = taken;
        }

        public JumpType JumpType { get; }
        public ushort Source { get; }
        public ushort Destination { get; }
        public bool Taken { get; }
    }
}