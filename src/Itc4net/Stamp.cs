using System;
using System.Diagnostics.Contracts;
using System.IO;
using Itc4net.Binary;
using Itc4net.Text;

namespace Itc4net
{
    /// <summary>
    /// An ITC stamp (logical clock)
    /// </summary>
    public class Stamp : IEquatable<Stamp>, IComparable<Stamp>
    {
        readonly Id _i;
        readonly Event _e;

        /// <summary>
        /// Initializes a new seed instance of the <see cref="Stamp"/> class.
        /// </summary>
        public Stamp() // seed
        {
            _i = 1;
            _e = new Event.Leaf(0);
        }

        public Stamp(Id i, Event e)
        {
            _i = i ?? throw new ArgumentNullException(nameof(i));
            _e = e ?? throw new ArgumentNullException(nameof(e));
        }

        public bool IsAnonymous => _i == 0;

        /// <summary>
        /// Happened-before relation
        /// </summary>
        public bool Leq(Stamp other)
        {
            return _e.Leq(other._e);
        }

        //// <summary>
        //// Happened-before relation
        //// </summary>
        //public static bool Leq(Stamp s1, Stamp s2)
        //{
        //    return s1._e.Leq(s2._e);
        //}

        /// <summary>
        /// Fork a stamp into 2 distinct stamps (returns a 2-tuple)
        /// </summary>
        /// <returns>
        /// A 2-tuple, each with a unique identity and a copy of the causal history.
        /// </returns>
        [Pure]
        public (Stamp, Stamp) Fork()
        {
            Id.Node i = _i.Split();
            return (
                new Stamp(i.L, _e),
                new Stamp(i.R, _e)
            );
        }

        /// <summary>
        /// Create an anonymous stamp with a copy of the causal history.
        /// </summary>
        [Pure]
        public Stamp Peek()
        {
            return new Stamp(0, _e);
        }

        /// <summary>
        /// Inflate (increment) the stamp.
        /// </summary>
        /// <remarks>
        /// In the ITC paper, this kernel operation is named "event".
        /// </remarks>
        [Pure]
        public Stamp Event()
        {
            Event fillEvent = Fill();
            if (_e != fillEvent)
            {
                return new Stamp(_i, fillEvent);
            }

            return new Stamp(_i, Grow().Item1);
        }

        /// <summary>
        /// Merge stamps.
        /// </summary>
        [Pure]
        public Stamp Join(Stamp other)
        {
            Id i = _i.Sum(other._i);
            Event e = _e.Join(other._e);

            return new Stamp(i, e);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"({_i},{_e})";
        }

        public bool Equals(Stamp other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _i.Equals(other._i) && _e.Equals(other._e);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as Stamp;
            return other != null && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (_i.GetHashCode()*397) ^ _e.GetHashCode();
            }
        }

        /// <inheritdoc/>
        public static bool operator ==(Stamp left, Stamp right)
        {
            return Equals(left, right);
        }

        /// <inheritdoc/>
        public static bool operator !=(Stamp left, Stamp right)
        {
            return !Equals(left, right);
        }

        internal (Event, int cost) Grow()
        {
            (Event, int) error = ((Event)null, -1);

            (Event, int) result = _e.Match(n =>
                {
                    if (_i == 1)
                    {
                        return (n + 1, 0);
                    }

                    (Event eʹ, int c) = new Stamp(_i, new Event.Node(n, 0, 0)).Grow();
                    return (eʹ, c + 1000);
                },
                (n, el, er) =>
                {
                    return _i.Match(
                        v => error,
                        (il, ir) =>
                        {
                            if (il == 0)
                            {
                                (Event eʹr, int cr) = new Stamp(ir, er).Grow();
                                return (new Event.Node(n, el, eʹr), cr + 1);
                            }

                            if (ir == 0)
                            {
                                (Event eʹl, int cl) = new Stamp(il, el).Grow();
                                return (new Event.Node(n, eʹl, er), cl + 1);
                            }

                            {
                                (Event eʹl, int cl) = new Stamp(il, el).Grow();
                                (Event eʹr, int cr) = new Stamp(ir, er).Grow();

                                if (cl < cr)
                                {
                                    return (new Event.Node(n, eʹl, er), cl + 1);
                                }
                                else // cl >= cr
                                {
                                    return (new Event.Node(n, el, eʹr), cr + 1);
                                }
                            }
                        });
                });

            if (result.Equals(error))
            {
                throw new InvalidOperationException($"Failed to grow stamp {this}");
            }

            return result;
        }

        internal Event Fill()
        {
            Event unchanged = _e;

            return _i.Match(
                v =>
                {
                    switch (v)
                    {
                        case 0:
                            return _e;
                        case 1:
                            return Itc4net.Event.Create(_e.Max());
                        default:
                            throw new InvalidOperationException();
                    }
                },
                (il, ir) =>
                {
                    if (il == 1)
                    {
                        return _e.Match(n => unchanged, (n, el, er) =>
                        {
                            var eʹr = new Stamp(ir, er).Fill();
                            Event e = Itc4net.Event.Create(n, Itc4net.Event.Create(Math.Max(el.Max(), eʹr.Min())), eʹr);
                            return e.Normalize();
                        });
                    }

                    if (ir == 1)
                    {
                        return _e.Match(n => unchanged, (n, el, er) =>
                        {
                            var eʹl = new Stamp(il, el).Fill();
                            Event e = Itc4net.Event.Create(n, eʹl, Itc4net.Event.Create(Math.Max(er.Max(), eʹl.Min())));
                            return e.Normalize();
                        });
                    }

                    return _e.Match(n => unchanged, (n, el, er) =>
                    {
                        var eʹl = new Stamp(il, el).Fill();
                        var eʹr = new Stamp(ir, er).Fill();
                        Event e = Itc4net.Event.Create(n, eʹl, eʹr);
                        return e.Normalize();
                    });
                }
            );
        }

        /// <summary>
        /// Converts the text encoding of an ITC stamp to its object equivalent.
        /// </summary>
        /// <param name="text">A string containing an ITC stamp text encoding.</param>
        /// <returns>An ITC stamp equivalent to the text contained in <para>text</para>.</returns>
        public static Stamp Parse(string text)
        {
            var parser = new Parser();
            return parser.ParseStamp(text);
        }

        /// <summary>
        /// Converts an ITC stamp to its equivalent binary encoding.
        /// </summary>
        /// <returns>A binary encoding of the ITC stamp.</returns>
        public byte[] ToBinary()
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BitWriter(stream, leaveOpen: true))
                {
                    _i.Encode(writer);
                    _e.Encode(writer);
                }

                return stream.ToArray();
            }
        }

        /// <summary>
        /// Converts the binary encoding of an ITC stamp to its object equivalent.
        /// </summary>
        /// <param name="bytes">A byte[] containing an ITC stamp binary encoding.</param>
        /// <returns>An ITC stamp equivalent to the bytes contained in <para>bytes</para>.</returns>
        public static Stamp FromBinary(byte[] bytes)
        {
            using (var decoder = new Decoder())
            {
                return decoder.Decode(bytes);
            }
        }

        /// <summary>
        /// Compares the current Stamp with another Stamp for a partial order.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>
        /// A value that indicates the partial order of the objects being compared.
        /// The return value has the following meanings:
        ///
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <term>Meaning</term>
        ///     </listheader>
        ///     <item>
        ///         <term>Less than zero</term>
        ///         <term>This stamp happens before <paramref name="other" />.</term>
        ///     </item>
        ///     <item>
        ///         <term>Zero</term>
        ///         <term>This stamp either equals or is concurrent with <paramref name="other" /></term>
        ///     </item>
        ///     <item>
        ///         <term>Greater than zero</term>
        ///         <term>The <paramref name="other" /> stamp happens before this stamp.</term>
        ///     </item>
        /// </list>
        /// </returns>
        /// <exception cref="System.ArgumentNullException">other</exception>
        /// <remarks>
        /// ITC stamps only provide a partial order. CompareTo returns zero when the compared
        /// stamps are equal or concurrent. When used with Sort methods, the stamps will be
        /// partially ordered.
        /// </remarks>
        public int CompareTo(Stamp other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            if (Equals(other))
            {
                return 0;
            }

            if (Leq(other))
            {
                return -1;
            }

            if (other.Leq(this))
            {
                return 1;
            }

            return 0;
        }

        public static implicit operator Stamp((Id i, Event e) tuple)
        {
            return new Stamp(tuple.i, tuple.e);
        }
    }
}