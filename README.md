# Itc4net: interval tree clocks for .NET

Itc4net is a C# implementation of Interval Tree Clocks (ITC), a causality tracking mechanism. 

*Disclaimer: While this project is intended for production use, it has not been used in production (yet). Towards this goal, there is an extensive set of unit tests, but it still requires real-world use and testing.*

### Implementation Details

- This project implements the 2008 paper, [Interval Tree Clocks: A Logical Clock for Dynamic Systems](http://gsd.di.uminho.pt/members/cbm/ps/itc2008.pdf) by Paulo Sérgio Almeida, Carlos Baquero, and Victor Fonte. The ITC paper is very readable and provides comprehensive descriptions of the ITC operations.
- This project is not a port of any existing ITC implementations.
- All core ITC classes are immutable. Accordingly, the core ITC kernel operations: fork, event, and join are pure functions. (The stamp text parser & scanner are not immutable)
- The id and event classes use a [discriminated union technique](http://stackoverflow.com/a/3199453) (or as close as you can get in C#) to distinguish between internal (node) and external (leaf) tree nodes. A *Match* function eliminates the need to cast and provides concise code without the need for type checks and casting.