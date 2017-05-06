<Query Kind="Program">
  <NuGetReference>FluentAssertions</NuGetReference>
  <NuGetReference Prerelease="true">Itc4net</NuGetReference>
  <Namespace>FluentAssertions</Namespace>
  <Namespace>Itc4net</Namespace>
</Query>

void Main()
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