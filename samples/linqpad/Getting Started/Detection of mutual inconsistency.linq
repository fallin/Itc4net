<Query Kind="Program">
  <NuGetReference>FluentAssertions</NuGetReference>
  <NuGetReference Prerelease="true">Itc4net</NuGetReference>
  <Namespace>FluentAssertions</Namespace>
  <Namespace>Itc4net</Namespace>
</Query>

void Main()
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