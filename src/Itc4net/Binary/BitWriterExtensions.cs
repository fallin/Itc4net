using System;

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

            int twoB = (int)Math.Pow(2, b);

            if (n < twoB)
            {
                writer.WriteBits(0, 1);
                writer.WriteBits((byte)n, (byte)b);
            }
            else
            {
                writer.WriteBits(1, 1);
                writer.EncodeN(n - twoB, b + 1);
            }
        }
    }
}
