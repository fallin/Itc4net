using System;
using System.IO;

namespace Itc4net.Binary
{
    abstract class BitProcessor : IDisposable
    {
        public const int EndOfStream = -1;
        readonly Stream _stream;
        readonly bool _leaveOpen;
        bool _disposed;

        protected BitProcessor(Stream stream) : this(stream, false)
        {
        }

        protected BitProcessor(Stream stream, bool leaveOpen)
        {
            _stream = stream;
            _leaveOpen = leaveOpen;
        }

        protected Stream BaseStream => _stream;
        protected byte RelativePosition { get; set; }
        protected int CurrentByte { get; set; }

        /// <summary>
        /// Creates a mask with the specified number of 1 bits (from the least-significant bits)
        /// </summary>
        /// <param name="bits">A count of the number of bits.</param>
        /// <returns>System.Byte.</returns>
        protected static byte CreateMask(int bits)
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
        /// Closes the stream.
        /// </summary>
        public void Close()
        {
            Dispose();
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
        ~BitProcessor()
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