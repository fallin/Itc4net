using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Itc4net.Extensions
{
    static class ByteArrayExtensions
    {
        public static string Hexify(this IEnumerable<byte> bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            StringBuilder builder = bytes.Aggregate(
                new StringBuilder(),
                (a, b) => a.AppendFormat("{0:X2}", b));
            return builder.ToString();
        }
    }
}
