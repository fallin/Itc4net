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

        public static Stamp Receive(this Stamp source, Stamp other)
        {
            return source.Join(other).Event();
        }

        public static Tuple<Stamp, Stamp> Sync(this Stamp source, Stamp other)
        {
            return source.Join(other).Fork();
        }

        public static bool Concurrent(this Stamp source, Stamp other)
        {
            return !source.Leq(other) && !other.Leq(source);
        }

        public static Tuple<Stamp, Stamp, Stamp> Fork3(this Stamp source)
        {
            Tuple<Stamp, Stamp> fork = source.Fork();
            Stamp a = fork.Item1;
            Stamp c = fork.Item2;

            Tuple<Stamp, Stamp> fork1 = a.Fork();
            a = fork1.Item1;
            Stamp b = fork1.Item2;

            return Tuple.Create(a, b, c);
        }

        public static Tuple<Stamp, Stamp, Stamp, Stamp> Fork4(this Stamp source)
        {
            Tuple<Stamp, Stamp> fork = source.Fork();
            Stamp a = fork.Item1;
            Stamp c = fork.Item2;

            Tuple<Stamp, Stamp> fork1 = a.Fork();
            a = fork1.Item1;
            Stamp b = fork1.Item2;

            Tuple<Stamp, Stamp> fork2 = c.Fork();
            c = fork2.Item1;
            Stamp d = fork2.Item2;

            return Tuple.Create(a, b, c, d);
        }
    }
}