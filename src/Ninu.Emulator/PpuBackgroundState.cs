namespace Ninu.Emulator
{
    public class PpuBackgroundState : IPersistable
    {
        public byte NextNameTableTileId { get; set; }
        public byte NextNameTableAttribute { get; set; }
        public byte NextLowPatternByte { get; set; }
        public byte NextHighPatternByte { get; set; }

        public ushort ShiftNameTableAttributeLow { get; set; }
        public ushort ShiftNameTableAttributeHigh { get; set; }
        public ushort ShiftLowPatternByte { get; set; }
        public ushort ShiftHighPatternByte { get; set; }

        public void SaveState(SaveStateContext context)
        {
            context.AddToState("PpuBackgroundState.NextNameTableTileId", NextNameTableTileId);
            context.AddToState("PpuBackgroundState.NextNameTableAttribute", NextNameTableAttribute);
            context.AddToState("PpuBackgroundState.NextLowPatternByte", NextLowPatternByte);
            context.AddToState("PpuBackgroundState.NextHighPatternByte", NextHighPatternByte);

            context.AddToState("PpuBackgroundState.ShiftNameTableAttributeLow", ShiftNameTableAttributeLow);
            context.AddToState("PpuBackgroundState.ShiftNameTableAttributeHigh", ShiftNameTableAttributeHigh);
            context.AddToState("PpuBackgroundState.ShiftLowPatternByte", ShiftLowPatternByte);
            context.AddToState("PpuBackgroundState.ShiftHighPatternByte", ShiftHighPatternByte);
        }

        public void LoadState(SaveStateContext context)
        {
            NextNameTableTileId = context.GetFromState<byte>("PpuBackgroundState.NextNameTableTileId");
            NextNameTableAttribute = context.GetFromState<byte>("PpuBackgroundState.NextNameTableAttribute");
            NextLowPatternByte = context.GetFromState<byte>("PpuBackgroundState.NextLowPatternByte");
            NextHighPatternByte = context.GetFromState<byte>("PpuBackgroundState.NextHighPatternByte");

            ShiftNameTableAttributeLow = context.GetFromState<ushort>("PpuBackgroundState.ShiftNameTableAttributeLow");
            ShiftNameTableAttributeHigh = context.GetFromState<ushort>("PpuBackgroundState.ShiftNameTableAttributeHigh");
            ShiftLowPatternByte = context.GetFromState<ushort>("PpuBackgroundState.ShiftLowPatternByte");
            ShiftHighPatternByte = context.GetFromState<ushort>("PpuBackgroundState.ShiftHighPatternByte");
        }
    }
}