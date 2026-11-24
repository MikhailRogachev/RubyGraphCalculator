# RubyGraphCalculator.Core — Mathematical Library
- Contains interfaces and classes for calculations: sin, cos, signal generation, FFT, and spectrograms
  All calculations are implemented here; `RubyGraphCalculator.UI` only invokes them
- External NuGet packages are prohibited. All algorithms must be custom-built
- No dependencies on WPF, UI, or logging
- Angles are represented by the `Angle` type (`Angle.FromDegrees`, `Angle.FromRadians`, properties `Degrees` and `Radians`).
  Internally `Angle` stores radians, because all trigonometric algorithms work in radians.
  Trigonometric functions accept `Angle`, not `double`
- `Angle` is normalized on creation to the range [0°, 360°) by removing full turns.
  Negative angles are normalized too: 450° → 90°, -90° → 270°, -450° → 270°, 720° → 0°.
  Radians are normalized the same way, to [0, 2π). The result is never equal to 360° (or 2π)
- `Angle.FromDegrees` and `Angle.FromRadians` throw `ArgumentException` for NaN and infinity
- FFT accepts signals of any length. Signals whose length is not a power of two are zero-padded to the next power of two; FFT is implemented with the Cooley-Tukey algorithm
- FFT returns amplitudes for frequencies from 0 to sample rate / 2, together with the frequency of each point
- Amplitudes are normalized: a sine wave with amplitude A produces a peak of height A
- Core does not throw for frequencies above Nyquist. It exposes the Nyquist frequency so the UI can warn
- Input data errors are handled via exceptions (`ArgumentException`, etc.) `RubyGraphCalculator.UI` is responsible for logging them
- Every new feature must include tests in `RubyGraphCalculator.Core.Tests`