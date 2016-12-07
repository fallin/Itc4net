using System;
using System.IO;

namespace Itc4net.Binary
{
    class BitReader
    {
        readonly Stream _stream;
        readonly bool _leaveOpen;
        bool _disposed;

        byte _relativePosition;
        int _currentByte;

        public const int EndOfStream = -1;

        public Stream BaseStream => _stream;

        public BitReader(byte[] bytes) : this(new MemoryStream(bytes))
        {
        }

        public BitReader(Stream stream) : this(stream, false)
        {
        }

        public BitReader(Stream stream, bool leaveOpen)
        {
            _stream = stream;
            _leaveOpen = leaveOpen;

            _relativePosition = 0;
            _currentByte = EndOfStream;
        }

        public int ReadBits(byte bitCount, out byte value)
        {
            if (bitCount > 8)
            {
                throw new ArgumentOutOfRangeException(nameof(bitCount), bitCount, "Must be between 0 and 8 (inclusive).");
            }

            // Attempt to read the next byte to prime the _currentByte
            if (_currentByte == EndOfStream)
            {
                _currentByte = BaseStream.ReadByte();
            }

            int readBits;
            int currentBits = 0x00;

            if (_currentByte != EndOfStream)
            {
                _relativePosition += bitCount;
                int shift = 8 - _relativePosition;
                currentBits |= shift > 0 ? (byte)(_currentByte >> shift) : (byte)(_currentByte << Math.Abs(shift));
                readBits = bitCount;

                // Adjust when moving beyond byte boundary
                if (_relativePosition > 7)
                {
                    _relativePosition %= 8;
                    _currentByte = BaseStream.ReadByte();
                    if (_currentByte != EndOfStream)
                    {
                        shift = 8 - _relativePosition;
                        currentBits |= shift > 0 ? (byte)(_currentByte >> shift) : (byte)(_currentByte << Math.Abs(shift));
                    }
                    else
                    {
                        readBits -= _relativePosition;
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

        /// <summary>
        /// Closes the stream.
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        public void Flush()
        {
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
        ~BitReader()
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