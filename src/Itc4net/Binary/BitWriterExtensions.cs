using System;
using System.Collections.Generic;
using System.Linq;

namespace Itc4net.Binary
{
    static class BitWriterExtensions
    {
        public static void EncodeN(this BitWriter writer, int n)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            writer.WriteBits(1, 1);
            writer.EncodeN(n, 2);
        }

        static void EncodeN(this BitWriter writer, int n, int b)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            long twoB = (long)Math.Pow(2, b);

            if (n < twoB)
            {
                writer.WriteBits(0, 1);

                if (b <= 8)
                {
                    writer.WriteBits((byte)n, b);
                }
                else
                {
                    // The current implementation of BitWriter currently only supports
                    // writing 0-8 bits at a time. I may fix this in the future, but
                    // until then, just break writing 'n' into multiple writes.
                    int remainder;
                    int quotient = Math.DivRem(b, 8, out remainder);

                    if (remainder > 0)
                    {
                        byte value = (byte) ((n >> (8 * quotient)) & 0x000000FF);
                        writer.WriteBits(value, remainder);
                    }

                    for (int index = quotient - 1; index >= 0; index--)
                    {
                        byte value = (byte)((n >> (8 * index)) & 0x000000FF);
                        writer.WriteBits(value, 8);
                    }
                }
            }
            else
            {
                writer.WriteBits(1, 1);
                writer.EncodeN((int) (n - twoB), b + 1);
            }
        }
    }
}
