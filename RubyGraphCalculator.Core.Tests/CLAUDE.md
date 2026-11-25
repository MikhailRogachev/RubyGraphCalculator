# RubyGraphCalculator.Core.Tests — math tests
- Allowed packages: NUnit, NUnit3TestAdapter, Microsoft.NET.Test.Sdk. Other packages (including Moq) — only on request
- One test class per Core class: `SignalGenerator` → `SignalGeneratorTests`
- Use Assert.That(...) only, not ClassicAssert
- Compare floating-point results with a tolerance: Is.EqualTo(expected).Within(1e-9)
- Check reference values (e.g. against Math.Sin), known results (FFT of a pure sine peaks at its frequency)   and mathematical properties (Parseval's theorem).

- Folder structure mirrors `RubyGraphCalculator.Core`: tests for `Angles/Angle.cs` go to `Angles/AngleTests.cs`,
  namespace `RubyGraphCalculator.Core.Tests.Angles`
