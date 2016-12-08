using System;
using Itc4net.Extensions;

namespace Itc4net.Binary
{
    /// <summary>
    /// An ITC stamp decoder
    /// </summary>
    class Decoder : IDisposable
    {
        byte[] _bytes;
        BitReader _reader;
        bool _disposed;
        int _currentPosition;

        public Decoder()
        {
            _reader = null;
        }

        public Stamp Decode(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            _bytes = bytes;
            _reader = new BitReader(bytes);

            Id i = DecodeId();
            Event e = DecodeEvent();

            return new Stamp(i, e);
        }

        string DescribeBytes()
        {
            if (_bytes == null)
            {
                return "(null)";
            }

            if (_bytes.Length == 0)
            {
                return "(empty)";
            }

            return BitConverter.ToString(_bytes);
        }

        void ThrowUnexpected(int unexpected)
        {
            int errorPosition = _currentPosition;
            string error = $"Error parsing {DescribeBytes()}. Unexpected value 0x{unexpected:X2} at bit index {errorPosition}.";

            throw new DecoderException(error, null, unexpected, errorPosition);
        }

        void ThrowUnexpectedEndOfStream()
        {
            int errorPosition = _currentPosition;
            string error = $"Error parsing {DescribeBytes()}. Unexpected EndOfStream at bit index {errorPosition}.";

            throw new DecoderException(error, null, BitProcessor.EndOfStream, errorPosition);
        }

        void ThrowExpected(int expecting, int found)
        {
            int errorPosition = _currentPosition;
            string error = $"Error parsing {DescribeBytes()}. Expecting value 0x{expecting:X2}"
                           + $", yet found 0x{found:X2} @ bit index {errorPosition}";

            throw new DecoderException(error, expecting, found, errorPosition);
        }

        byte Scan(byte bitCount)
        {
            byte value;
            var readBits = _reader.ReadBits(bitCount, out value);
            if (readBits == bitCount)
            {
                _currentPosition += bitCount;
                return value;
            }

            _currentPosition += Math.Max(readBits, 0);
            ThrowUnexpectedEndOfStream();
            return 0;
        }

        Id DecodeId()
        {
            Id id = null;

            byte prefix1 = Scan(2);
            switch (prefix1)
            {
                case 0: // 0 | 1
                    byte v = Scan(1);
                    id = new Id.Leaf(v);
                    break;
                case 1: // (0,1)
                    id = new Id.Node(0, DecodeId());
                    break;
                case 2: // (i,0)
                    id = new Id.Node(DecodeId(), 0);
                    break;
                case 3: // (l,r)
                    id = new Id.Node(DecodeId(), DecodeId());
                    break;
                default:
                    ThrowUnexpected(prefix1);
                    break;
            }

            return id;
        }

        Event DecodeEvent()
        {
            Event e = null;

            byte prefix1 = Scan(1);
            switch (prefix1)
            {
                case 0: // node
                    e = DecodeEventNode();
                    break;
                case 1: // leaf
                    int n = DecodeN(2);
                    e = new Event.Leaf(n);
                    break;
                default:
                    ThrowUnexpected(prefix1);
                    break;
            }

            return e;
        }

        Event DecodeEventNode()
        {
            Event e = null;

            byte prefix2 = Scan(2);
            switch (prefix2)
            {
                case 0: // (0,0,er)
                    e = new Event.Node(0, 0, DecodeEvent());
                    break;
                case 1: // (0,el,0)
                    e = new Event.Node(0, DecodeEvent(), 0);
                    break;
                case 2: // (0,el,er)
                    e = new Event.Node(0, DecodeEvent(), DecodeEvent());
                    break;
                case 3: // n != 0
                    byte prefix3 = Scan(1);
                    switch (prefix3)
                    {
                        case 0:
                            byte prefix4 = Scan(1);
                            switch (prefix4)
                            {
                                case 0: // (n,0,er)
                                    e = new Event.Node(DecodeN(), 0, DecodeEvent());
                                    break;
                                case 1: // (n,el,0)
                                    e = new Event.Node(DecodeN(), DecodeEvent(), 0);
                                    break;
                                default:
                                    ThrowUnexpected(prefix4);
                                    break;
                            }
                            break;
                        case 1: // (n,el,er)
                            e = new Event.Node(DecodeN(), DecodeEvent(), DecodeEvent());
                            break;
                        default:
                            ThrowUnexpected(prefix3);
                            break;
                    }
                    break;
                default:
                    ThrowUnexpected(prefix2);
                    break;
            }

            return e;
        }

        int DecodeN()
        {
            byte scan = Scan(1);
            if (scan == 1)
            {
                return DecodeN(2);
            }

            ThrowExpected(1, scan);
            return 0;
        }

        int DecodeN(byte b)
        {
            int n = 0;

            byte scan = Scan(1);
            switch (scan)
            {
                case 0:
                    n = Scan(b);
                    break;
                case 1:
                    n = (int) (DecodeN((byte) (b + 1)) + Math.Pow(2, b));
                    break;
                default:
                    ThrowUnexpected(scan);
                    break;
            }

            return n;
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
        ~Decoder()
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
                    _reader?.Dispose();
                }

                // release any unmanaged objects
                // set the object references to null

                _disposed = true;
            }
        }
    }
}