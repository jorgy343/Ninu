using System;
using System.IO;

namespace Ninu.Emulator
{
    public class NesImage
    {
        public int ProgramRomBankCount { get; }
        public int PatternRomBankCount { get; }
        public int ProgramRamBankCount { get; }

        public int MapperType { get; }
        public TvSystem CartridgeType { get; }

        public NameTableMirrorMode MirrorMode { get; }
        public bool BatteryBackedRam { get; }
        public bool FourScreenVramLayout { get; }

        public bool VsUnisystem { get; }
        public bool PlayChoice10 { get; }

        public byte[]? Trainer { get; }
        public byte[] ProgramRom { get; }
        public byte[] PatternRom { get; }

        public NesImage(string filePath)
        {
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath)) throw new FileNotFoundException();

            using var fileStream = File.OpenRead(filePath);
            using var reader = new BinaryReader(fileStream);

            var magicNumber = reader.ReadBytes(4);

            if (magicNumber[0] != 0x4e || magicNumber[1] != 0x45 || magicNumber[2] != 0x53 || magicNumber[3] != 0x1a)
            {
                throw new InvalidImageException();
            }

            ProgramRomBankCount = reader.ReadByte();
            PatternRomBankCount = reader.ReadByte();

            var flags6 = reader.ReadByte();
            var flags7 = reader.ReadByte();
            var flags8 = reader.ReadByte();
            var flags9 = reader.ReadByte();

            // Handle flags 6.
            MirrorMode = (flags6 & 0b0000_0001) == 0 ? NameTableMirrorMode.Horizontal : NameTableMirrorMode.Vertical;
            BatteryBackedRam = (flags6 & 0b0000_0010) != 0;
            FourScreenVramLayout = (flags6 & 0b0000_1000) != 0;

            MapperType = (flags6 & 0b1111_0000) >> 4; // The low nibble of the mapper type.

            // Handle flags 7.
            VsUnisystem = (flags7 & 0b0000_0001) != 0;
            PlayChoice10 = (flags7 & 0b0000_0010) != 0;

            MapperType |= flags7 & 0b1111_0000; // The high nibble of the mapper type.

            // Handle flags 8.
            ProgramRamBankCount = flags8 == 0 ? 1 : flags8; // Old formats would hard code this value to zero. In that case assume there is 1 ram bank.

            // Handle flags 9.
            CartridgeType = (flags9 & 0b0000_0001) == 0 ? TvSystem.Ntsc : TvSystem.Pal;

            // Read the rest of the header.
            reader.ReadBytes(6);

            // If a trainer is present, read 512 bytes.
            if ((flags6 & 0b0000_0100) != 0)
            {
                Trainer = reader.ReadBytes(512);
            }

            ProgramRom = reader.ReadBytes(16384 * ProgramRomBankCount);
            PatternRom = reader.ReadBytes(8192 * PatternRomBankCount);

            // TODO: Handle PlayChoice INST-ROM and PlayChoice PROM.
        }
    }
}