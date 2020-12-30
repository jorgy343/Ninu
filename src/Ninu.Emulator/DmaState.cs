namespace Ninu.Emulator
{
    public class DmaState
    {
        /// <summary>
        /// Determines if the DMA is processing and the CPU is suspended.
        /// </summary>
        [Save]
        public bool Processing { get; set; }

        /// <summary>
        /// Determines if the DMA is properly synchronized to the CPU clock. If this is false, a
        /// dummy cycle will take place.
        /// </summary>
        [Save]
        public bool Synchronized { get; set; }

        /// <summary>
        /// The current byte that needs to be read during the DMA process. At the start of a DMA
        /// process, this will be zero and it counts up for every byte copied.
        /// </summary>
        [Save]
        public int CurrentByte { get; set; }

        /// <summary>
        /// This is the page from which data will be copied from the CPU bus. The page is the high
        /// byte of the CPU address. Data will then be read from 0xXX00 to 0xXXff during the DMA
        /// transfer where XX represents this value.
        /// </summary>
        [Save]
        public byte CpuHighAddress { get; set; }

        /// <summary>
        /// This stores the byte of data that was read during the read cycle of the DMA process.
        /// </summary>
        [Save]
        public byte ReadByte { get; set; }
    }
}