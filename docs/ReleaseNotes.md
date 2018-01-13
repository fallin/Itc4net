# Itc4net Release Notes

Version 1.2.1

- Fixes bug in Stamp.CompareTo(Stamp) method. The original implementation returned 0 (zero) if the stamps were equal or concurrent; instead, it should return true if the stamps are equivalent or concurrent. The Stamp.Equals method is a structural comparison of the ID and event components, which is useful, but is not appropriate here.
- Added *Equivalent* extension method to Stamp class. A stamp where `(a ≤ b) Λ (b ≤ a)` is considered equivalent. That is, it returns true if two stamps represent the same causal past (ignoring the ID component).

Version 1.2.0

- Package now includes assemblies targeting netstandard2.0, net45, and net47.

  *Note, the net47 assembly is included to support using the Itc4net package with tuples (System.ValueTuple package) in LINQPad v5 on a Windows 10 machine running net47. See https://goo.gl/G42BLy for brief explanation of the issue.*

- Includes additional README documentation

Version 1.1.0

- Added StampConverter (TypeConverter) so timestamps serialize properly when using Json.NET

Version 1.0.0

- Initial release