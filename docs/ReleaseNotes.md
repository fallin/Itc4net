# Itc4net Release Notes

Version 1.2.0

- Package now includes assemblies targeting netstandard2.0, net45, and net47.

  *Note, the net47 assembly is included to support using the Itc4net package with tuples (System.ValueTuple package) in LINQPad v5 on a Windows 10 machine running net47. See https://goo.gl/G42BLy for brief explanation of the issue.*

- Includes additional README documentation

Version 1.1.0

- Added StampConverter (TypeConverter) so timestamps serialize properly when using Json.NET

Version 1.0.0

- Initial release