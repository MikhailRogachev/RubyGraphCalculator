# Project: Simulator – Learning Trigonometric Functions and Spectrum – RubyGraphCalculator

## Project Structure
- Four separate subprojects: `RubyGraphCalculator.UI`, `RubyGraphCalculator.Core`, `RubyGraphCalculator.UI.Tests`, `RubyGraphCalculator.Core.Tests`
- `RubyGraphCalculator.UI` – user interface
- `RubyGraphCalculator.Core` – base classes, mathematical calculations
- `RubyGraphCalculator.UI.Tests` – NUnit tests for ViewModels
- `RubyGraphCalculator.Core.Tests` – NUnit tests for base classes and mathematical calculations

## Technologies
- WPF
- `TargetFramework`: `net10.0` for `RubyGraphCalculator.Core` and `RubyGraphCalculator.Core.Tests`   (Core has no WPF dependency)
- `TargetFramework`: `net10.0-windows` for `RubyGraphCalculator.UI` and `RubyGraphCalculator.UI.Tests`
- C#
- MVVM
- Do not change the .NET version, `TargetFramework`, or NuGet packages without an explicit request
- Package versions are managed exclusively in `Directory.Packages.props`

## Dependencies
- Project references: UI → Core, Core.Tests → Core, UI.Tests → UI.
  Core never references UI.

## Domain
- The simulator supports angles in degrees and radians. Degrees are the default unit in the UI; the user can switch to radians
- The angle unit must always be explicit. Never pass an angle as a bare `double`
- Sample rate (Hz) is set by the user. Default: 10000 Hz
- If a signal frequency exceeds the Nyquist frequency (sample rate / 2), the UI shows a warning.
  The signal is still generated and the spectrum shows aliasing (for learning purposes)

## Code Style
- Use `var` for variable type declarations
- Add `sealed` to the declaration when creating a class
- Remove `sealed` from the class declaration if a derived class is added

## Commands
- Build: `dotnet build`
- Tests: `dotnet test`
- Formatting: `dotnet format`

## Rules
- `RubyGraphCalculator.Core` must perform mathematical calculations independently without relying on third-party NuGet packages
- `RubyGraphCalculator.UI.Tests` may use Moq
- `RubyGraphCalculator.Core.Tests` must not use Moq
- Do not modify or delete existing tests just to make them pass. If a test fails, fix the implementation code. If you believe the bug is in the test itself, ask first
- Never commit or push directly to `main`. Create a separate branch for each task
- Run `dotnet test` before committing: all tests must pass
- Merge changes into `main` only via pull request: `gh pr create`
- Do not change `global.json` without an explicit request
- Do not change the version in `Directory.Build.props` without an explicit request
- Do not modify `.github/workflows/` without an explicit request