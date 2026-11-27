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
- `FromDegrees` normalizes in degrees first, then converts to radians, so whole-degree angles
  are normalized exactly (e.g. -36000° → 0°, not 359.999...°)
- `Angle.FromDegrees` and `Angle.FromRadians` throw `ArgumentException` for NaN and infinity
- `Trig.Sin` reduces the angle from [0, 2π) to [0, π/2] using symmetry, then evaluates a Taylor series
- `Trig.Cos` is implemented via `Trig.Sin`: cos x = sin(x + π/2)
- Core never calls `Math.Sin`, `Math.Cos` or other `Math` trigonometric functions.
  Tests may use them as a reference
- `SignalGenerator.Sin(frequency, amplitude, phase, sampleRate, duration)` returns `Signal(Time, Values)`
- Valid ranges (outside → `ArgumentException`, also for NaN and infinity):
  frequency 40–30 000 Hz, amplitude 0.1–5, sampleRate 1 000–192 000 Hz, duration 0.1–20 s
- Number of samples: N = floor(duration × sampleRate + 1e-9). The small epsilon prevents
  losing a sample to rounding (0.7 s × 44 100 Hz = 30 869.999…, but N must be 30 870)
- Times: tₙ = n / sampleRate, starting at 0
- The phase of each sample is computed directly from n, never accumulated.
  Reduce to the fractional number of cycles first: (frequency · n mod sampleRate) / sampleRate,
  then multiply by 2π — computing 2π·f·t directly loses precision for long signals
- A sum of sine waves: `SignalGenerator.Sum(IReadOnlyList<SineComponent> components, int sampleRate, double duration)`
  returns `Signal(Time, Values)`. `SineComponent` is `record SineComponent(double Frequency, double Amplitude, Angle Phase)`
  in `Signals/`. Sample rate and duration are passed once: all components share one time grid
- `Sum` accepts 1 to 4 components; 0, more than 4 or null → `ArgumentException`.
  Each component uses the same ranges as `Sin`. Components with equal frequencies are allowed
- `Sum` is computed in one pass: for each sample n, add up all components. Never generate each component
  as a separate `Signal` and add the arrays (4 components at 20 s × 192 kHz would allocate ~250 MB).
  The phase of each component is computed as in `Sin` (fractional cycles, never accumulated)
- `Sin` is implemented via `Sum` with one component. Do not duplicate the generation logic in `Sin`
- FFT accepts signals of any length. Signals whose length is not a power of two are zero-padded to the next power of two; FFT is implemented with the Cooley-Tukey algorithm
- FFT returns amplitudes for frequencies from 0 to sample rate / 2, together with the frequency of each point
- Amplitudes are normalized: a sine wave with amplitude A produces a peak of height A
- Core does not throw for frequencies above Nyquist. It exposes the Nyquist frequency so the UI can warn
- Input data errors are handled via exceptions (`ArgumentException`, etc.) `RubyGraphCalculator.UI` is responsible for logging them
- Every new feature must include tests in `RubyGraphCalculator.Core.Tests`

## Folder structure
- Folders by feature: `Angles/`, `Trigonometry/`, `Signals/`, `Spectrum/`.
  New code goes into the matching folder; ask before creating a new top-level folder
- Namespace matches the folder: `RubyGraphCalculator.Core.Angles`
