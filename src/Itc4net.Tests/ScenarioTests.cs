using System;
using FluentAssertions;
using NUnit.Framework;

namespace Itc4net.Tests
{
    public class ScenarioTests
    {
        [Test(Description = "Example from ITC 2008 paper, section 5.1")]
        public void Itc2008Example()
        {
            Stamp seed = new Stamp();
            seed.Should().Be(new Stamp(1, 0));

            Tuple<Stamp, Stamp> forkSeed = seed.Fork();
            Stamp p1 = forkSeed.Item1;
            p1.Should().Be(new Stamp(new Id.Node(1, 0), 0));

            Stamp p2 = forkSeed.Item2;
            p2.Should().Be(new Stamp(new Id.Node(0, 1), 0));

            p1 = p1.Event();
            p1.Should().Be(new Stamp(new Id.Node(1, 0), new Event.Node(0, 1, 0)));

            Tuple<Stamp, Stamp> fork1 = p1.Fork();
            p1 = fork1.Item1;
            p1.Should().Be(new Stamp(new Id.Node(new Id.Node(1, 0), 0), new Event.Node(0, 1, 0)));

            Stamp p3 = fork1.Item2;
            p3.Should().Be(new Stamp(new Id.Node(new Id.Node(0, 1), 0), new Event.Node(0, 1, 0)));

            p1 = p1.Event();
            p1.Should().Be(new Stamp(new Id.Node(new Id.Node(1, 0), 0), new Event.Node(0, new Event.Node(1, 1, 0), 0)));

            p2 = p2.Event();
            p2.Should().Be(new Stamp(new Id.Node(0, 1), new Event.Node(0, 0, 1)));

            p2 = p2.Event();
            p2.Should().Be(new Stamp(new Id.Node(0, 1), new Event.Node(0, 0, 2)));

            Stamp join23 = p2.Join(p3);
            join23.Should().Be(new Stamp(new Id.Node(new Id.Node(0, 1), 1), new Event.Node(1, 0, 1)));

            Tuple<Stamp, Stamp> fork23 = join23.Fork();
            p2 = fork23.Item1;
            p2.Should().Be(new Stamp(new Id.Node(new Id.Node(0, 1), 0), new Event.Node(1, 0, 1)));

            p3 = fork23.Item2;
            p3.Should().Be(new Stamp(new Id.Node(0, 1), new Event.Node(1, 0, 1)));

            p1 = p1.Receive(p2);
            p1.Should().Be(new Stamp(new Id.Node(1,0), 2));
        }

        [Test]
        public void OrderingOfEventsExample()
        {
            //         ^            ^ time ↑
            //         |            |
            // evt p3  *            |
            //         |            |
            // rcv p2  *---+    +---* q2 rcv
            //         |    \  /    |
            //         |     \/     |
            //         |     /\     |
            //         |    /  \    |
            // rcv p1  *---+    +---* q1 snd
            //         |            |

            Stamp seed = new Stamp();

            Tuple<Stamp, Stamp> fork = seed.Fork();
            Process p = new Process("p", fork.Item1);
            Process q = new Process("q", fork.Item2);

            Message msgp1 = new Message("p1");
            Stamp p1 = p.Send(msgp1);
            Console.WriteLine($"p1:{p1} after send");

            Message msgq1 = new Message("q1");
            Stamp q1 = q.Send(msgq1);
            Console.WriteLine($"q1:{q1} after send");

            Stamp p2 = p.Receive(msgp1);
            Console.WriteLine($"p2:{p2} after receive");

            Stamp q2 = q.Receive(msgq1);
            Console.WriteLine($"q1:{q1} after receive");

            Stamp p3 = p.SomethingWithCausalSignificance();
            Console.WriteLine($"p3:{p3} after event");

            p1.Leq(p2).Should().BeTrue();
            p1.Leq(p3).Should().BeTrue();
            p2.Leq(p3).Should().BeTrue();

            p1.Concurrent(q1).Should().BeTrue();
            p3.Concurrent(q2).Should().BeTrue();
        }
    }

    class Message
    {
        public string Payload { get; }
        public Stamp Stamp { get; set; }

        public Message(string payload)
        {
            Payload = payload;
        }
    }

    class Process
    {
        readonly string _name;
        Stamp _s;

        public Process(string name, Stamp s)
        {
            _name = name;
            _s = s;
        }

        public Stamp Send(Message msg)
        {
            Stamp anonymous;
            _s = _s.Send(out anonymous);
            msg.Stamp = anonymous;

            // e.g., _bus.Send(message);
            Console.WriteLine($"{_name} snd '{msg.Payload}' with stamp:{msg.Stamp}");
            return _s;
        }

        public Stamp Receive(Message msg)
        {
            // e.g., _bus.Receive(message);
            _s = _s.Receive(msg.Stamp);
            Console.WriteLine($"{_name} rcv '{msg.Payload}' with stamp:{msg.Stamp}");
            return _s;
        }

        public Stamp SomethingWithCausalSignificance()
        {
            _s = _s.Event();
            return _s;
        }
    }
}
