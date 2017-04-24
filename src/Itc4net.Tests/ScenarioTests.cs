using System;
using FluentAssertions;
using NUnit.Framework;

namespace Itc4net.Tests
{
    /// <summary>
    /// This test fixture includes examples of using interval tree clock timestamps
    /// for causality tracking in different scenarios (e.g., version vectors and vector
    /// clocks). These are far from *unit* tests, but test assertions are handy for illustrating
    /// expectations throughout the scenarios.
    /// </summary>
    [TestFixture]
    public class ScenarioTests
    {
        // Example from ITC 2008 paper, section 5.1
        //
        //                                    e
        //                             a3-----------a4----------+
        //                            /                          \                 
        //                e          /                            \          e       
        //            a1-----a2-----f                              j-----a5-----a6    
        //           /               \                            /
        //          /                 \                          /
        //         /                   c1---+              c3---+                  
        //  seed--f                          \            /
        //         \                          \          /
        //          \                          j--------f
        //           \                        /  (sync)  \
        //            \                      /            \
        //             b1-----b2-----b3-----+              b4                     
        //                 e      e
        //
        [Test(Description = "Example from ITC 2008 paper, section 5.1")]
        public void Itc2008Example()
        {
            // Start with seed stamp
            Stamp seed = new Stamp();
            seed.Should().Be(new Stamp(1, 0));

            // Fork seed stamp to create stamps for process A and B
            (Stamp a1, Stamp b1) = seed.Fork();
            a1.Should().Be(new Stamp(new Id.Node(1, 0), 0));
            b1.Should().Be(new Stamp(new Id.Node(0, 1), 0));

            // Process A suffers an event
            Stamp a2 = a1.Event();
            a2.Should().Be(new Stamp(new Id.Node(1, 0), new Event.Node(0, 1, 0)));

            // Process A forks to create stamp for new process C (dynamic number of participants)
            (Stamp a3, Stamp c1) = a2.Fork();
            a3.Should().Be(new Stamp(new Id.Node(new Id.Node(1, 0), 0), new Event.Node(0, 1, 0)));
            c1.Should().Be(new Stamp(new Id.Node(new Id.Node(0, 1), 0), new Event.Node(0, 1, 0)));

            // Process A suffers an event
            Stamp a4 = a3.Event();
            a4.Should().Be(new Stamp(new Id.Node(new Id.Node(1, 0), 0), new Event.Node(0, new Event.Node(1, 1, 0), 0)));

            // Process B suffers an event
            Stamp b2 = b1.Event();
            b2.Should().Be(new Stamp(new Id.Node(0, 1), new Event.Node(0, 0, 1)));

            // Process B suffers an event
            Stamp b3 = b2.Event();
            b3.Should().Be(new Stamp(new Id.Node(0, 1), new Event.Node(0, 0, 2)));

            // Process B and C synchronize (join and fork)
            (Stamp c3, Stamp b4) = b3.Sync(c1);
            c3.Should().Be(new Stamp(new Id.Node(new Id.Node(0, 1), 0), new Event.Node(1, 0, 1)));
            b4.Should().Be(new Stamp(new Id.Node(0, 1), new Event.Node(1, 0, 1)));

            // Process C retires and joins with process A
            Stamp a5 = a4.Join(c3);
            a5.Should().Be(new Stamp(new Id.Node(1, 0), new Event.Node(1, new Event.Node(0, 1, 0), 1)));

            // Process A suffers an event (inflates and simplifies to single integer event)
            Stamp a6 = a5.Event();
            a6.Should().Be(new Stamp(new Id.Node(1,0), 2));
        }

        // Example from ITC 2008 paper, section 5.1
        //
        //                                    e
        //                             a3-----------a4----------+
        //                            /                          \                 
        //                e          /                            \          e       
        //            a1-----a2-----f                              j-----a5-----a6    
        //           /               \                            /
        //          /                 \                          /
        //         /                   c1---+              c3---+                  
        //  seed--f                          \            /
        //         \                          \          /
        //          \                          j--------f
        //           \                        /  (sync)  \
        //            \                      /            \
        //             b1-----b2-----b3-----+              b4                     
        //                 e      e
        //
        [Test(Description = "Example from ITC 2008 paper, section 5.1 (with implicit conversion)")]
        public void Itc2008ExampleWithImplicitConversionOperators()
        {
            // Start with seed stamp
            Stamp seed = new Stamp();
            seed.Should().Be((1, 0));

            // Fork seed stamp to create stamps for process A and B
            (Stamp a1, Stamp b1) = seed.Fork();
            a1.Should().Be(((1, 0), 0));
            b1.Should().Be(((0, 1), 0));

            // Process A suffers an event
            Stamp a2 = a1.Event();
            a2.Should().Be(((1, 0), (0, 1, 0)));

            // Process A forks to create stamp for new process C (dynamic number of participants)
            (Stamp a3, Stamp c1) = a2.Fork();
            a3.Should().Be((((1, 0), 0), (0, 1, 0)));
            c1.Should().Be((((0, 1), 0), (0, 1, 0)));

            // Process A suffers an event
            Stamp a4 = a3.Event();
            a4.Should().Be((((1, 0), 0), (0, (1, 1, 0), 0)));

            // Process B suffers an event
            Stamp b2 = b1.Event();
            b2.Should().Be(((0, 1), (0, 0, 1)));

            // Process B suffers an event
            Stamp b3 = b2.Event();
            b3.Should().Be(((0, 1), (0, 0, 2)));

            // Process B and C synchronize (join and fork)
            (Stamp c3, Stamp b4) = b3.Sync(c1);
            c3.Should().Be((((0, 1), 0), (1, 0, 1)));
            b4.Should().Be(((0, 1), (1, 0, 1)));

            // Process C retires and joins with process A
            Stamp a5 = a4.Join(c3);
            a5.Should().Be(((1, 0), (1, (0, 1, 0), 1)));

            // Process A suffers an event (inflates and simplifies to single integer event)
            Stamp a6 = a5.Event();
            a6.Should().Be(((1, 0), 2));
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

            (Stamp, Stamp) fork = seed.Fork();
            Process p = new Process("p", fork.Item1);
            Process q = new Process("q", fork.Item2);

            Message msgp1 = p.Send("p1");
            Stamp p1 = p.CurrentStamp;
            Console.WriteLine($"p1:{p1} after send");

            Message msgq1 = q.Send("q1");
            Stamp q1 = q.CurrentStamp;
            Console.WriteLine($"q1:{q1} after send");

            Stamp p2 = p.Receive(msgq1);
            Console.WriteLine($"p2:{p2} after receive");

            Stamp q2 = q.Receive(msgp1);
            Console.WriteLine($"q1:{q1} after receive");

            Stamp p3 = p.Increment();
            Console.WriteLine($"p3:{p3} after event");

            p1.Leq(p2).Should().BeTrue();
            p1.Leq(p3).Should().BeTrue();
            p2.Leq(p3).Should().BeTrue();

            q1.Leq(q2).Should().BeTrue();

            p1.Leq(q2).Should().BeTrue(); // send occurs before receive
            q1.Leq(p2).Should().BeTrue(); // send occurs before receive

            p1.Concurrent(q1).Should().BeTrue();
            p2.Concurrent(q2).Should().BeTrue();
            p3.Concurrent(q2).Should().BeTrue();

            // Stamp implements IComparable<Stamp> so a collection of stamps
            // can be partially ordered using Sort methods. Note this is a
            // partial order (not total order) as CompareTo returns 0
            // for equal and concurrent stamps. 

            var arr = new[] {q2, q1, p3, p2, p1};
            Array.Sort(arr);

            arr.Should().ContainInOrder(p1, p2, p3);
            arr.Should().ContainInOrder(q1, q2);
            arr.Should().ContainInOrder(p1, q2);
            arr.Should().ContainInOrder(q1, p2, p3);
        }

        [Test(Description = "Example from Detection of Mutual Inconsistency in Distributed Systems, Fig. 1")]
        public void DetectionOfMutualInconsistencyExample()
        {
            // Source http://zoo.cs.yale.edu/classes/cs426/2013/bib/parker83detection.pdf
            // Fig. 1. Partition graph G(f) for file stored redundantly at sites A, B, C, D.
            //
            //                   +------+
            //          +--------+ ABCD +-------+
            //          |        +------+       |
            //          |                       |
            //        +-v--+                  +-v--+
            //     *1 | AB +-------+  +-------+ CD +-------+
            //        +----+       |  |       +----+       |
            //          |         +v--v+                 +-v-+
            //          |      *2 | BC |                 | D |
            //          |         +----+                 +---+
            //          |            |                     |
            //        +-v-+          |        +-----+      |
            //     *3 | A |          +--------> BCD <------+
            //        +---+                   +-----+
            //          |                        |
            //          |     *4 +------+        |
            //          +--------> ABCD <--------+
            //                   +------+
            //
            // [*#] indicates modification to file

            // Fork seed stamp into four "sites"
            Stamp a, b, c, d;
            new Stamp().Fork(out a, out b, out c, out d);
            Console.WriteLine($"Site A: {a}");
            Console.WriteLine($"Site B: {b}");
            Console.WriteLine($"Site C: {c}");
            Console.WriteLine($"Site D: {d}");

            Stamp fileStampOnA = a.Peek();
            Console.WriteLine($"Initial file A stamp: {fileStampOnA}");

            Stamp fileStampOnB = b.Peek();
            Console.WriteLine($"Initial file B stamp: {fileStampOnB}");

            Stamp fileStampOnC = c.Peek();
            Console.WriteLine($"Initial file C stamp: {fileStampOnC}");

            Stamp fileStampOnD = d.Peek();
            Console.WriteLine($"Initial file D stamp: {fileStampOnD}");

            // Due to network partitions, clusters of sites are isolated from others at
            // various times.

            // A and B can communicate (forming {AB} partition), but are isolated from C and D
            // C and D can communicate (forming {CD} partition), but are isolated from A and B

            // File is modified at site A in {AB} partition
            a = a.Event();
            fileStampOnA = a.Peek(); // update file metadata
            Console.WriteLine($"File modified on A: {fileStampOnA}");

            // Site A notifies site B about the change to the file
            Console.WriteLine("Sync A->B");
            b.Concurrent(fileStampOnA).Should().BeFalse();
            b = b.Join(fileStampOnA);
            fileStampOnB = b.Peek(); // update file metadata
            Console.WriteLine($"File synced on B: {fileStampOnB}");

            // A and B become isolated from each other
            // C and D become isolated from each other

            // B and C resume communication, forming {BC} partition
            // Site B notifies site C about the previous change to the file
            Console.WriteLine("Sync B->C");
            c.Concurrent(fileStampOnB).Should().BeFalse();
            c = c.Join(fileStampOnB);
            fileStampOnC = c.Peek(); // update file metadata
            Console.WriteLine($"File synced on C: {fileStampOnC}");

            // File is modified at site C in {BC} partition
            c = c.Event();
            fileStampOnC = c.Peek(); // update file metadata
            Console.WriteLine($"File modified on C: {fileStampOnC}");

            // Site C shares the change with site B
            // There should not be a conflict (concurrent stamp) between B and C.
            Console.WriteLine("Sync C->B");
            b.Concurrent(fileStampOnC).Should().BeFalse();
            b = b.Join(fileStampOnC);
            fileStampOnB = b.Peek(); // update file metadata
            Console.WriteLine($"File synced on B: {fileStampOnB}");

            // File is modified at site A in {A} partition
            a = a.Event();
            fileStampOnA = a.Peek();
            Console.WriteLine($"File modified on A: {fileStampOnA}");

            // B, C, and D resume communication, forming {BCD} partition
            // Since they can now communicate with each other, B and C share information
            // about the modified file with D
            Console.WriteLine("Sync C->D");
            d.Concurrent(fileStampOnC).Should().BeFalse();
            d = d.Join(fileStampOnC);
            fileStampOnD = d.Peek(); // updates file metadata
            Console.WriteLine($"File synced on D: {fileStampOnD}");

            Console.WriteLine("Sync B->D");
            d.Concurrent(fileStampOnB).Should().BeFalse();
            d = d.Join(fileStampOnB);
            fileStampOnD = d.Peek(); // updates file metadata
            Console.WriteLine($"File synced on D: {fileStampOnD}");

            // A, B, C, and D again establish communication and share information
            // about the file changes
            //
            // There should be a conflict (concurrent stamps) between A and the
            // other sites: B, C, and D
            a.Concurrent(fileStampOnB).Should().BeTrue();
            a.Concurrent(fileStampOnC).Should().BeTrue();
            a.Concurrent(fileStampOnD).Should().BeTrue();

            // The concurrent stamps indicate there is a conflict which must
            // be resolved (manual or otherwise). Once the conflict is resolved
            // the updated file is stored and a new stamp must be generated that
            // dominates the others... this can be done by joining the stamps from
            // the (previously) concurrent stamps and inflating the stamp at A.
            a = a.Join(fileStampOnB);
            a = a.Join(fileStampOnC);
            a = a.Join(fileStampOnD);
            a = a.Event(); // inflate for conflict resolution
            fileStampOnA = a.Peek(); // update file metadata
            Console.WriteLine($"Resolve conflict on A: {fileStampOnA}");

            // Site A communicates conflict resolution to other sites
            Console.WriteLine("Sync A->B");
            b.Concurrent(fileStampOnA).Should().BeFalse();
            b = b.Join(fileStampOnA);
            fileStampOnB = b.Peek();
            Console.WriteLine($"File synced on B: {fileStampOnB}");

            Console.WriteLine("Sync A->C");
            c.Concurrent(fileStampOnA).Should().BeFalse();
            c = c.Join(fileStampOnA);
            fileStampOnC = c.Peek();
            Console.WriteLine($"File synced on C: {fileStampOnC}");

            Console.WriteLine("Sync A->D");
            d.Concurrent(fileStampOnA).Should().BeFalse();
            d = d.Join(fileStampOnA);
            fileStampOnD = d.Peek();
            Console.WriteLine($"File synced on D: {fileStampOnD}");
        }
    }

    class Message
    {
        public string Text { get; }
        public Stamp Stamp { get; }

        public Message(string text, Stamp stamp)
        {
            Text = text;
            Stamp = stamp;
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

        public Stamp CurrentStamp => _s.Peek();

        public Message Send(string text)
        {
            Stamp anonymous;
            _s = _s.Send(out anonymous);

            Message msg = new Message(text, anonymous);

            // e.g., _bus.Send(message);
            Console.WriteLine($"{_name} snd '{msg.Text}' with stamp:{msg.Stamp}");
            return msg;
        }

        public Stamp Receive(Message msg)
        {
            // e.g., _bus.Receive(message);
            _s = _s.Receive(msg.Stamp);
            Console.WriteLine($"{_name} rcv '{msg.Text}' with stamp:{msg.Stamp}");
            return CurrentStamp;
        }

        public Stamp Increment()
        {
            _s = _s.Event();
            return CurrentStamp;
        }
    }
}
