<Query Kind="Program">
  <NuGetReference>FluentAssertions</NuGetReference>
  <NuGetReference Prerelease="true">Itc4net</NuGetReference>
  <Namespace>FluentAssertions</Namespace>
  <Namespace>Itc4net</Namespace>
</Query>

void Main()
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

    var arr = new[] { q2, q1, p3, p2, p1 };
    Array.Sort(arr);

    arr.Should().ContainInOrder(p1, p2, p3);
    arr.Should().ContainInOrder(q1, q2);
    arr.Should().ContainInOrder(p1, q2);
    arr.Should().ContainInOrder(q1, p2, p3);
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