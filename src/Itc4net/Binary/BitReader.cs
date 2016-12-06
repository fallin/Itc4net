using System;
using System.IO;

namespace Itc4net.Binary
{
    /*
    public class BitReader : BitIO
    {
        public BitReader(Stream stream)
            : base(stream)
        {
        }

        public BitReader(Stream stream, bool leaveOpen) : base(stream, leaveOpen)
        {
            RelativePosition = 0;
            CurrentByte = -1;
        }

        public int ReadCompleteChunk(int countBits, out byte value)
        {
            int readBits = ReadChunk(countBits, out value);
            if (readBits == countBits || readBits == EndOfStream) // complete chunk
            {
                return readBits;
            }

            Seek(-countBits, SeekOrigin.Current);
            value = 0x00;
            return 0;
        }

        public int ReadChunk(int countBits, out byte value)
        {
            if (countBits < 0 || countBits > 8)
            {
                throw new ArgumentOutOfRangeException(nameof(countBits), countBits, "Must be between 0 and 8 (inclusive).");
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
                RelativePosition += countBits;
                int shift = 8 - RelativePosition;
                currentBits |= shift > 0 ? (byte)(CurrentByte >> shift) : (byte)(CurrentByte << Math.Abs(shift));
                readBits = countBits;

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

                byte mask = CreateMask(countBits);
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
    */
}