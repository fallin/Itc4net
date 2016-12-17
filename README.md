# Itc4net: interval tree clocks for .NET

Itc4net is a C# implementation of Interval Tree Clocks (ITC), a causality tracking mechanism. 

*Disclaimer: While this project is intended for production use, it has not yet been used in production. Towards this goal, there is an extensive set of unit tests, but it still requires real-world use and testing.*

### Overview

This project is a C#/.NET implementation of the ideas presented in the 2008 paper, [Interval Tree Clocks: A Logical Clock for Dynamic Systems](http://gsd.di.uminho.pt/members/cbm/ps/itc2008.pdf). An Interval Tree Clock (ITC) provides a means of causality tracking in a distributed system with a dynamic number of participants and offers a way to determine the partial ordering of events, i.e., a happened-before relation.

The term *causality* in distributed systems originates from the concept of causality in physics where "causal connections gives us the only ordering of events that all observers will agree on" ([The Speed of Light is NOT About Light](https://youtu.be/msVuCEs8Ydo?t=44s) | [PBS Digital Studios | Space Time](https://www.youtube.com/channel/UC7_gcs09iThXybpVgjHZ_7g)). In relativity, observers moving relative to each other may not agree on the elapsed time or distance between events. Similarly, in distributed systems, a causal history (or compressed representation) is necessary to determine the partial ordering of events or the detection of inconsistent data replicas because physical clocks are unreliable.

### Getting Started

Install using the NuGet Package Manager:

```
Install-Package Itc4net -Pre
```

... TODO ...

### Additional Resources

- [Interval Tree Clocks: A Logical Clock for Dynamic Systems](http://gsd.di.uminho.pt/members/cbm/ps/itc2008.pdf)
- [Logical time resources](https://gist.github.com/macintux/a79a254dd0bdd330702b): A convenient collection of links to logical time whitepapers and resources, including Lamport timestamps, version vectors, vector clocks, and more.
- [The trouble with timestamps](https://aphyr.com/posts/299-the-trouble-with-timestamps)
- [Provenance and causality in distributed systems](http://blog.jessitron.com/2016/09/provenance-and-causality-in-distributed.html): An excellent description of causality, why it's important and useful; a call to action!

### Implementation Details

- The 2008 ITC paper provides exceptional and detailed descriptions of how to implement the kernel operations (fork-event-join model). This project implements the timestamps, IDs, and events directly from the descriptions and formulas in the ITC paper; it is not a port of any existing ITC implementations.
- The core ITC classes are immutable. Accordingly, the core ITC kernel operations: fork, event, and join are pure functions.
- The ID and event classes use a [discriminated union technique](http://stackoverflow.com/a/3199453) (or as close as you can get in C#) to distinguish between internal (node) and external (leaf) tree nodes. A *Match* function eliminates the need to cast and provides concise code without the need for type checks and casting.