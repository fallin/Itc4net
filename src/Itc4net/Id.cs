using System;
using System.Runtime.InteropServices;
using Itc4net.Binary;

namespace Itc4net
{
    public abstract class Id : IEquatable<Id>
    {
        Id() { } // ensure no external classes can inherit

        internal abstract T Match<T>(Func<int, T> leaf, Func<Id, Id, T> node);
        internal abstract Id Normalize();
        internal abstract Node Split();
        internal abstract Id Sum(Id i2);
        internal abstract void Encode(BitWriter writer);

        public class Leaf : Id
        {
            public int Value { get; }

            public Leaf(int value)
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Must be 0 or 1");
                }

                Value = value;
            }

            internal override T Match<T>(Func<int, T> leaf, Func<Id, Id, T> node)
            {
                return leaf(Value);
            }

            internal override Id Normalize()
            {
                return this;
            }

            internal override Node Split()
            {
                switch (Value)
                {
                    case 0:
                        return new Node(0, 0);
                    case 1:
                        return new Node(new Node(1, 0), new Node(0, 1));
                    default:
                        throw new InvalidOperationException("Value must be 0 or 1");
                }
            }

            internal override Id Sum(Id i2)
            {
                switch (Value)
                {
                    case 0:
                        return i2;
                    case 1:
                        return 1;
                    default:
                        throw new InvalidOperationException("Value must be 0 or 1");
                }
            }

            public override string ToString()
            {
                return $"{Value}";
            }

            public override int GetHashCode()
            {
                return Value;
            }

            internal override void Encode(BitWriter writer)
            {
                writer.WriteBits(0, 2);
                writer.WriteBits((byte) Value, 1);
            }
        }

        public class Node : Id
        {
            public Id L { get; }
            public Id R { get; }

            public Node(Id left, Id right)
            {
                if (left == null) throw new ArgumentNullException(nameof(left));
                if (right == null) throw new ArgumentNullException(nameof(right));

                L = left;
                R = right;
            }

            internal override T Match<T>(Func<int, T> leaf, Func<Id, Id, T> node)
            {
                return node(L, R);
            }

            internal override Id Normalize()
            {
                if (L == 0 && R == 0)
                {
                    return 0;
                }

                if (L == 1 && R == 1)
                {
                    return 1;
                }

                return this;
            }

            internal override Node Split()
            {
                Node result;

                if (L == 0)
                {
                    Node i = R.Split();
                    result = new Node(new Node(0, i.L), new Node(0, i.R));
                }
                else if (R == 0)
                {
                    Node i = L.Split();
                    result = new Node(new Node(i.L, 0), new Node(i.R, 0));
                }
                else
                {
                    result = new Node(new Node(L, 0), new Node(0, R));
                }

                return result;
            }

            internal override Id Sum(Id i2)
            {
                var l1 = L;
                var r1 = R;

                return i2.Match(
                    v2 => v2 == 0 ? this : i2,
                    (l2, r2) => new Node(Sum(l1, l2), Sum(r1, r2)).Normalize()
                );
            }

            internal override void Encode(BitWriter writer)
            {
                if (L == 0)
                {
                    writer.WriteBits(1, 2);
                    R.Encode(writer);
                }
                else if (R == 0)
                {
                    writer.WriteBits(2, 2);
                    L.Encode(writer);
                }
                else
                {
                    writer.WriteBits(3, 2);
                    L.Encode(writer);
                    R.Encode(writer);
                }
            }

            public override string ToString()
            {
                return $"({L},{R})";
            }
        }

        public static Id Create(int value)
        {
            return new Leaf(value);
        }

        public static Id Create(Id left, Id right)
        {
            return new Node(left, right);
        }

        internal void Match(Action<int> leaf, Action<Id, Id> node)
        {
            // Adapt leaf action to func (ret null)
            Func<int, object> adaptLeaf = value =>
            {
                leaf?.Invoke(value);
                return null;
            };

            // Adapt node action to func (ret null)
            Func<Id, Id, object> adaptNode = (il, ir) =>
            {
                node?.Invoke(il, ir);
                return null;
            };

            Match(adaptLeaf, adaptNode);
        }

        internal static Id Sum(Id i1, Id i2)
        {
            return i1.Sum(i2);
        }

        public static implicit operator Id(int i)
        {
            return new Leaf(i);
        }

        //public static implicit operator Id(Tuple<int> t)
        //{
        //    return new Leaf(t.Item1);
        //}

        //public static implicit operator Id(Tuple<Id, Id> t)
        //{
        //    return new Node(t.Item1, t.Item2);
        //}

        public bool Equals(Id other)
        {
            if (other == null) return false;

            return Match(
                v1 =>
                {
                    return other.Match(v2 => v1 == v2, (l2, r2) => false);
                },
                (l1, r1) =>
                {
                    return other.Match(v2 => false, (l2, r2) => l1.Equals(l2) && r1.Equals(r2));
                }
                );
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as Id;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            return Match(v => v, (l, r) => unchecked (l.GetHashCode()*397 ^ r.GetHashCode()));
        }

        public static bool operator ==(Id left, Id right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Id left, Id right)
        {
            return !Equals(left, right);
        }
    }
}
