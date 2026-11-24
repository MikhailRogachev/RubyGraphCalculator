# RubyGraphCalculator.UI.Tests — ViewModel tests
- Allowed packages: NUnit, NUnit3TestAdapter, Microsoft.NET.Test.Sdk, Moq. Other packages — only on request
- One test class per ViewModel: `MainViewModel` → `MainViewModelTests`
- Use Assert.That(...) only, not ClassicAssert
- Test ViewModels only, not WPF windows (no UI automation)
- Mock interfaces with Moq (Core interfaces, Serilog.ILogger), not concrete classes