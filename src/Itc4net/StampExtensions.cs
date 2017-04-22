using System;

namespace Itc4net
{
    public static class StampExtensions
    {
        public static Stamp Send(this Stamp source, out Stamp anonymous)
        {
            Stamp inflated = source.Event();
            anonymous = inflated.Peek();
            return inflated;
        }

        public static (Stamp inflated, Stamp anonymous) Send(this Stamp source)
        {
            Stamp inflated = source.Event();
            Stamp anonymous = inflated.Peek();
            return (inflated, anonymous);
        }

        public static Stamp Receive(this Stamp source, Stamp other)
        {
            return source.Join(other).Event();
        }

        public static (Stamp, Stamp) Sync(this Stamp source, Stamp other)
        {
            return source.Join(other).Fork();
        }

        /// <summary>
        /// Indicates whether the source stamp is concurrent with the other stamp.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="other">The other.</param>
        /// <returns><c>true</c> if a !--> b &amp;&amp; b !--> a, <c>false</c> otherwise. (Where "!-->" is not happends-before)</returns>
        public static bool Concurrent(this Stamp source, Stamp other)
        {
            return !source.Leq(other) && !other.Leq(source);
        }

        /// <summary>
        /// Fork a stamp into 3 distinct stamps (returns a 3-tuple)
        /// </summary>
        /// <param name="source">The source stamp.</param>
        /// <returns>A 3-tuple, each with a unique identity and a copy of the causal history.</returns>
        /// <remarks>
        /// A convenience method for forking 1 stamp into 3 unique stamps.
        /// </remarks>
        public static (Stamp, Stamp, Stamp) Fork3(this Stamp source)
        {
            (Stamp, Stamp) fork = source.Fork();
            Stamp a = fork.Item1;
            Stamp c = fork.Item2;

            (Stamp, Stamp) fork1 = a.Fork();
            a = fork1.Item1;
            Stamp b = fork1.Item2;

            return (a, b, c);
        }

        /// <summary>
        /// Fork a stamp into 4 distinct stamps (returns a 4-tuple)
        /// </summary>
        /// <param name="source">The source stamp.</param>
        /// <returns>A 4-tuple, each with a unique identity and a copy of the causal history.</returns>
        /// <remarks>
        /// A convenience method for forking 1 stamp into 4 unique stamps.
        /// </remarks>
        public static (Stamp, Stamp, Stamp, Stamp) Fork4(this Stamp source)
        {
            (Stamp, Stamp) fork = source.Fork();
            Stamp a = fork.Item1;
            Stamp c = fork.Item2;

            (Stamp, Stamp) fork1 = a.Fork();
            a = fork1.Item1;
            Stamp b = fork1.Item2;

            (Stamp, Stamp) fork2 = c.Fork();
            c = fork2.Item1;
            Stamp d = fork2.Item2;

            return (a, b, c, d);
        }

        /// <summary>
        /// Fork a stamp into 2 distinct stamps (as out parameters)
        /// </summary>
        /// <param name="source">The source stamp.</param>
        /// <param name="s1">1st stamp with a unique identity and a copy of the causal history.</param>
        /// <param name="s2">2nd stamp with a unique identity and a copy of the causal history.</param>
        /// <remarks>
        /// A convenience method when out parameters are preferred over a return 2-tuple.
        /// </remarks>
        public static void Fork(this Stamp source, out Stamp s1, out Stamp s2)
        {
            var forked = source.Fork();
            s1 = forked.Item1;
            s2 = forked.Item2;
        }

        /// <summary>
        /// Fork a stamp into 3 distinct stamps (as out parameters)
        /// </summary>
        /// <param name="source">The source stamp.</param>
        /// <param name="s1">1st stamp with a unique identity and a copy of the causal history.</param>
        /// <param name="s2">2nd stamp with a unique identity and a copy of the causal history.</param>
        /// <param name="s3">3rd stamp with a unique identity and a copy of the causal history.</param>
        /// <remarks>
        /// A convenience method when out parameters are preferred over a return 3-tuple.
        /// </remarks>
        public static void Fork(this Stamp source, out Stamp s1, out Stamp s2, out Stamp s3)
        {
            var forked = source.Fork3();
            s1 = forked.Item1;
            s2 = forked.Item2;
            s3 = forked.Item3;
        }

        /// <summary>
        /// Fork a stamp into 4 distinct stamps (as out parameters)
        /// </summary>
        /// <param name="source">The source stamp.</param>
        /// <param name="s1">1st stamp with a unique identity and a copy of the causal history.</param>
        /// <param name="s2">2nd stamp with a unique identity and a copy of the causal history.</param>
        /// <param name="s3">3rd stamp with a unique identity and a copy of the causal history.</param>
        /// <param name="s4">4th stamp with a unique identity and a copy of the causal history.</param>
        /// <remarks>
        /// A convenience method when out parameters are preferred over a return 4-tuple.
        /// </remarks>
        public static void Fork(this Stamp source, out Stamp s1, out Stamp s2, out Stamp s3, out Stamp s4)
        {
            var forked = source.Fork4();
            s1 = forked.Item1;
            s2 = forked.Item2;
            s3 = forked.Item3;
            s4 = forked.Item4;
        }
    }
}