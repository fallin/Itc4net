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

        public int ReadBits(int bitCount, out byte value)
        {
            if (bitCount < 0 || bitCount > 8)
            {
                throw new ArgumentOutOfRangeException(nameof(bitCount), bitCount, "Must be between 0 and 8 (inclusive).");
            }

            // Attempt to read the next byte to prime CurrentByte
            if (CurrentByte == EndOfStream)
            {
                CurrentByte = BaseStream.ReadByte();
            }

            value = 0;
            int actualBitsRead = 0;
            if (CurrentByte != EndOfStream)
            {
                int shift = 8 - RelativePosition - bitCount;
                value = shift > 0 ? (byte)(CurrentByte >> shift) : (byte)(CurrentByte << Math.Abs(shift));
                value = (byte) (value & CreateMask(bitCount));
                actualBitsRead += bitCount;

                if (shift > 0)
                {
                    RelativePosition += bitCount;
                }
                else // reached or exceeded byte boundary
                {
                    RelativePosition = 0;
                    CurrentByte = BaseStream.ReadByte();

                    if (shift < 0)
                    {
                        actualBitsRead = bitCount + shift;

                        byte remainingValue;
                        actualBitsRead += ReadBits((byte) Math.Abs(shift), out remainingValue);
                        value |= remainingValue;
                    }
                }
            }

            return actualBitsRead;
        }
    }
}