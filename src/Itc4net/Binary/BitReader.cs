using System;
using System.IO;

namespace Itc4net.Binary
{
    class BitReader : BitProcessor
    {
        public BitReader(byte[] bytes) : this(new MemoryStream(bytes))
        {
        }

        public BitReader(Stream stream) : this(stream, false)
        {
        }

        public BitReader(Stream stream, bool leaveOpen) : base(stream, leaveOpen)
        {
            RelativePosition = 0;
            CurrentByte = EndOfStream;
        }

        public int ReadBits(byte bitCount, out byte value)
        {
            if (bitCount > 8)
            {
                throw new ArgumentOutOfRangeException(nameof(bitCount), bitCount, "Must be between 0 and 8 (inclusive).");
            }

            // Attempt to read the next byte to prime the _currentByte
            if (CurrentByte == EndOfStream)
            {
                CurrentByte = BaseStream.ReadByte();
            }

            int readBits;
            int currentBits = 0x00;

            if (CurrentByte != EndOfStream)
            {
                RelativePosition += bitCount;
                int shift = 8 - RelativePosition;
                currentBits |= shift > 0 ? (byte)(CurrentByte >> shift) : (byte)(CurrentByte << Math.Abs(shift));
                readBits = bitCount;

                // Adjust when moving beyond byte boundary
                if (RelativePosition > 7)
                {
                    RelativePosition %= 8;
                    CurrentByte = BaseStream.ReadByte();
                    if (CurrentByte != EndOfStream)
                    {
                        shift = 8 - RelativePosition;
                        currentBits |= shift > 0 ? (byte)(CurrentByte >> shift) : (byte)(CurrentByte << Math.Abs(shift));
                    }
                    else
                    {
                        readBits -= RelativePosition;
                    }
                }

                byte mask = CreateMask(bitCount);
                currentBits &= mask;
            }
            else
            {
                readBits = EndOfStream;
            }

            value = (byte)currentBits;
            return readBits;
        }
    }
}