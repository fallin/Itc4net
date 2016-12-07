using System;
using System.IO;

namespace Itc4net.Binary
{
    class BitWriter : IDisposable
    {
        readonly Stream _stream;
        readonly bool _leaveOpen;
        bool _disposed;

        byte _relativePosition;
        byte _currentByte;

        public Stream BaseStream => _stream;

        public BitWriter(Stream stream) : this(stream, false)
        {
        }

        public BitWriter(Stream stream, bool leaveOpen)
        {
            _stream = stream;
            _leaveOpen = leaveOpen;

            _relativePosition = 0;
            _currentByte = 0;
        }

        public void WriteBits(byte value, byte bitCount)
        {
            if (bitCount > 8)
            {
                throw new ArgumentOutOfRangeException(nameof(bitCount), bitCount, "Must be between 0 and 8 (inclusive).");
            }

            value = (byte) (value & CreateMask(bitCount));

            int shift = 8 - _relativePosition - bitCount;
            byte writeValue = shift > 0 ? (byte)(value << shift) : (byte)(value >> Math.Abs(shift));

            _currentByte |= writeValue;

            if (shift > 0)
            {
                _relativePosition += bitCount;
            }
            else
            {
                _stream.WriteByte(_currentByte);
                _relativePosition = 0;
                _currentByte = 0;

                if (shift < 0)
                {
                    WriteBits(value, (byte)Math.Abs(shift));
                }
            }
        }

        /// <summary>
        /// Closes the stream.
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        public void Flush()
        {
            if (_relativePosition > 0)
            {
                _stream.WriteByte(_currentByte);
                _relativePosition = 0;
                _currentByte = 0;
            }
        }

        /// <summary>
        /// Creates a mask with the specified number of 1 bits (from the least-significant bits)
        /// </summary>
        /// <param name="bits">A count of the number of bits.</param>
        /// <returns>System.Byte.</returns>
        static byte CreateMask(int bits)
        {
            switch (bits)
            {
                case 0:
                    return 0x00;
                case 1:
                    return 0x01;
                case 2:
                    return 0x03;
                case 3:
                    return 0x07;
                case 4:
                    return 0x0F;
                case 5:
                    return 0x1F;
                case 6:
                    return 0x3F;
                case 7:
                    return 0x7F;
                case 8:
                    return 0xFF;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bits), bits, "Must be between 0 and 8 (inclusive).");
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="BitWriter"/> class.
        /// </summary>
        ~BitWriter()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases unmanaged and managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Flush();

                    if (_leaveOpen)
                    {
                        _stream.Flush();
                    }
                    else
                    {
                        _stream.Dispose();
                    }
                }

                // release any unmanaged objects
                // set the object references to null

                _disposed = true;
            }
        }
    }
}