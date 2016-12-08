using System;
using System.Runtime.CompilerServices;
using Itc4net.Binary;

namespace Itc4net
{
    public abstract class Event : IEquatable<Event>
    {
        Event() { } // ensure no external classes can inherit

        internal abstract T Match<T>(Func<int, T> leaf, Func<int, Event, Event, T> node);
        internal abstract bool Leq(Event e2);
        internal abstract Event Normalize();
        internal abstract Event Lift(int m);
        internal abstract Event Sink(int m);
        internal abstract int Min();
        internal abstract int Max();
        internal abstract Event Join(Event e2);
        internal abstract void Encode(BitWriter writer);

        public sealed class Leaf : Event
        {
            public int N { get; }

            public Leaf(int n)
            {
                if (n < 0) throw new ArgumentOutOfRangeException(nameof(n), n, "Must be >= 0");

                N = n;
            }

            public override string ToString()
            {
                return $"{N}";
            }

            internal override T Match<T>(Func<int, T> leaf, Func<int, Event, Event, T> node)
            {
                return leaf(N);
            }

            internal override bool Leq(Event e2)
            {
                var n1 = N;

                return e2.Match(
                    // leq(n1, n2) = n1 <= n2
                    n2 => n1 <= n2,

                    // leq(n1,(n2,l2,r2)) = n1 <= n2
                    (n2, l2, r2) => n1 <= n2
                    );
            }

            internal override Event Normalize()
            {
                return this;
            }

            internal override Event Lift(int m)
            {
                return new Leaf(N + m);
            }

            internal override Event Sink(int m)
            {
                return new Leaf(N - m);
            }

            internal override int Min()
            {
                return N;
            }

            internal override int Max()
            {
                return N;
            }

            internal override Event Join(Event e2)
            {
                int n1 = N;

                return e2.Match(
                    n2 => new Leaf(Math.Max(n1, n2)),
                    (n2, l2, r2) => Join(new Node(n1, 0, 0), e2)
                );
            }

            internal override void Encode(BitWriter writer)
            {
                writer.EncodeN(N);
            }
        }

        public sealed class Node : Event
        {
            public int N { get; }
            public Event L { get; }
            public Event R { get; }

            public Node(int n, Event left, Event right)
            {
                if (n < 0) throw new ArgumentOutOfRangeException(nameof(n), n, "Must be >= 0");
                if (left == null) throw new ArgumentNullException(nameof(left));
                if (right == null) throw new ArgumentNullException(nameof(right));

                N = n;
                L = left;
                R = right;
            }

            public override string ToString()
            {
                return $"({N},{L},{R})";
            }

            internal override T Match<T>(Func<int, T> leaf, Func<int, Event, Event, T> node)
            {
                return node(N, L, R);
            }

            internal override bool Leq(Event e2)
            {
                var n1 = N;
                var l1 = L;
                var r1 = R;

                return e2.Match(
                    // leq((n1,l1,r1),n2) = n1 <= n2 Λ leq(l1↑n1,n2) Λ leq(r1↑n1,n2)
                    n2 => n1 <= n2 && Leq(l1.Lift(n1), e2) && Leq(r1.Lift(n1), e2),

                    // leq((n1,l1,r1),(n2,l2,r2)) = n1 <= n2 Λ leq(l1↑n1,l2↑n2) Λ leq(r1↑n1,r2↑n2)
                    (n2, l2, r2) => n1 <= n2 && Leq(l1.Lift(n1), l2.Lift(n2)) && Leq(r1.Lift(n1), r2.Lift(n2))
                    );
            }

            internal override Event Normalize()
            {
                int m1 = L.Match(n => n, (n, e1, e2) => -1);
                int m2 = R.Match(n => n, (n, e1, e2) => -1);

                if (m1 > -1 && m2 > -1 && m1 == m2)
                {
                    return Create(N + m1);
                }

                int m = Math.Min(L.Min(), R.Min());
                return Create(N + m, L.Sink(m).Normalize(), R.Sink(m).Normalize());
            }

            internal override Event Lift(int m)
            {
                return new Node(N + m, L, R);
            }

            internal override Event Sink(int m)
            {
                int newN = N - m;
                if (newN < 0)
                {
                    int overflow = Math.Abs(newN);
                    return new Node(0, L.Sink(overflow), R.Sink(overflow));
                }

                return new Node(newN, L, R);
            }

            internal override int Min()
            {
                return N + Math.Min(L.Min(), R.Min());
            }

            internal override int Max()
            {
                return N + Math.Max(L.Max(), R.Max());
            }

            internal override Event Join(Event e2)
            {
                var n1 = N;
                var l1 = L;
                var r1 = R;

                return e2.Match(
                    n2 => Join(this, new Node(n2, 0, 0)),
                    (n2, l2, r2) =>
                    {
                        if (n1 > n2)
                        {
                            return Join(e2, this);
                        }

                        Event l = Join(l1, l2.Lift(n2 - n1));
                        Event r = Join(r1, r2.Lift(n2 - n1));
                        Event e = new Node(n1, l, r);
                        return e.Normalize();
                    }
                );
            }

            internal override void Encode(BitWriter writer)
            {
                if (N == 0 && L == 0)
                {
                    writer.WriteBits(0, 1);
                    writer.WriteBits(0, 2);
                    R.Encode(writer);
                }
                else if (N == 0 && R == 0)
                {
                    writer.WriteBits(0, 1);
                    writer.WriteBits(1, 2);
                    L.Encode(writer);
                }
                else if (N == 0)
                {
                    writer.WriteBits(0, 1);
                    writer.WriteBits(2, 2);
                    L.Encode(writer);
                    R.Encode(writer);
                }
                else if (L == 0)
                {
                    writer.WriteBits(0, 1);
                    writer.WriteBits(3, 2);
                    writer.WriteBits(0, 1);
                    writer.WriteBits(0, 1);
                    writer.EncodeN(N);
                    R.Encode(writer);
                }
                else if (R == 0)
                {
                    writer.WriteBits(0, 1);
                    writer.WriteBits(3, 2);
                    writer.WriteBits(0, 1);
                    writer.WriteBits(1, 1);
                    writer.EncodeN(N);
                    L.Encode(writer);
                }
                else
                {
                    writer.WriteBits(0, 1);
                    writer.WriteBits(3, 2);
                    writer.WriteBits(1, 1);
                    writer.EncodeN(N);
                    L.Encode(writer);
                    R.Encode(writer);
                }
            }
        }

        public static Event Create(int n)
        {
            return new Leaf(n);
        }

        public static Event Create(int n, Event e1, Event e2)
        {
            return new Node(n, e1, e2);
        }

        internal void Match(Action<int> leaf, Action<int, Event, Event> node)
        {
            Func<int, object> leafFunc = n =>
            {
                leaf?.Invoke(n);
                return null;
            };
            Func<int, Event, Event, object> nodeFunc = (n, l, r) =>
            {
                node?.Invoke(n, l, r);
                return null;
            };

            Match(leafFunc, nodeFunc);
        }

        internal static bool Leq(Event e1, Event e2)
        {
            return e1.Leq(e2);
        }

        internal static Event Join(Event e1, Event e2)
        {
            return e1.Join(e2);
        }

        public static implicit operator Event(int n)
        {
            return new Leaf(n);
        }

        //public static implicit operator Event(Tuple<int> t)
        //{
        //    return new Leaf(t.Item1);
        //}

        //public static implicit operator Event(Tuple<int, Event, Event> t)
        //{
        //    return new Node(t.Item1, t.Item2, t.Item3);
        //}

        public bool Equals(Event other)
        {
            if (other == null) return false;

            return Match(
                n1 =>
                {
                    return other.Match(n2 => n1 == n2, (n2, l2, r2) => false);
                },
                (n1, l1, r1) =>
                {
                    return other.Match(n2 => false, (n2, l2, r2) => n1 == n2 && l1.Equals(l2) && r1.Equals(r2));
                });
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as Event;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            return Match(n => n, (n, l, r) => unchecked(n * 13 ^ l.GetHashCode() * 397 ^ r.GetHashCode()));
        }

        public static bool operator ==(Event left, Event right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Event left, Event right)
        {
            return !Equals(left, right);
        }
    }
}