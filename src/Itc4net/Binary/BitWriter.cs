using System;
using System.IO;

namespace Itc4net.Binary
{
    class BitWriter : BitProcessor
    {
        public BitWriter(Stream stream) : this(stream, false)
        {
        }

        public BitWriter(Stream stream, bool leaveOpen) : base(stream, leaveOpen)
        {
            RelativePosition = 0;
            CurrentByte = 0;
        }

        public void WriteBits(byte value, int bitCount)
        {
            if (bitCount < 0 || bitCount > 8)
            {
                throw new ArgumentOutOfRangeException(nameof(bitCount), bitCount, "Must be between 0 and 8 (inclusive).");
            }

            value = (byte) (value & CreateMask(bitCount));

            int shift = 8 - RelativePosition - bitCount;
            byte writeValue = shift > 0 ? (byte)(value << shift) : (byte)(value >> Math.Abs(shift));

            CurrentByte |= writeValue;

            if (shift > 0)
            {
                RelativePosition += bitCount;
            }
            else
            {
                BaseStream.WriteByte((byte) CurrentByte);
                RelativePosition = 0;
                CurrentByte = 0;

                if (shift < 0)
                {
                    WriteBits(value, (byte)Math.Abs(shift));
                }
            }
        }

        public void Complete()
        {
            if (RelativePosition > 0)
            {
                BaseStream.WriteByte((byte) CurrentByte);
                RelativePosition = 0;
                CurrentByte = 0;
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Complete();
            }

            base.Dispose(disposing);
        }
    }
}