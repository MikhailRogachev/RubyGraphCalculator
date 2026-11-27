using RubyGraphCalculator.Core.Angles;
using RubyGraphCalculator.Core.Trigonometry;

namespace RubyGraphCalculator.Core.Signals;

/// <summary>Generates sampled test signals.</summary>
public static class SignalGenerator
{
    private const double MinFrequency = 40.0;
    private const double MaxFrequency = 30_000.0;
    private const double MinAmplitude = 0.1;
    private const double MaxAmplitude = 5.0;
    private const int MinSampleRate = 1_000;
    private const int MaxSampleRate = 192_000;
    private const double MinDuration = 0.1;
    private const double MaxDuration = 20.0;
    private const int MinComponents = 1;
    private const int MaxComponents = 4;

    // duration · sampleRate can land just below a whole number (0.7 · 44 100 = 30 869.999…);
    // without this epsilon floor() would lose the last sample.
    private const double SampleCountEpsilon = 1e-9;

    private const double TwoPi = 2.0 * Math.PI;

    /// <summary>
    /// Generates amplitude · sin(2π · frequency · t + phase), sampled at t = n / sampleRate
    /// for n = 0 … N - 1, where N = floor(duration · sampleRate + 1e-9).
    /// </summary>
    /// <param name="frequency">Frequency in Hz, from 40 to 30 000.</param>
    /// <param name="amplitude">Amplitude, from 0.1 to 5.</param>
    /// <param name="phase">Initial phase.</param>
    /// <param name="sampleRate">Sample rate in Hz, from 1 000 to 192 000.</param>
    /// <param name="duration">Duration in seconds, from 0.1 to 20.</param>
    /// <exception cref="ArgumentException">A value is outside its range, NaN or infinity.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="phase"/> is null.</exception>
    /// <remarks>
    /// Frequencies above the Nyquist frequency (sampleRate / 2) are allowed: the signal is generated
    /// as is and shows aliasing.
    /// </remarks>
    public static Signal Sin(double frequency, double amplitude, Angle phase, int sampleRate, double duration) =>
        Sum([new SineComponent(frequency, amplitude, phase)], sampleRate, duration);

    /// <summary>
    /// Generates the sum of 1 to 4 sine waves, all sampled on one time grid t = n / sampleRate
    /// for n = 0 … N - 1, where N = floor(duration · sampleRate + 1e-9).
    /// </summary>
    /// <param name="components">1 to 4 components; each uses the same ranges as <see cref="Sin"/>.</param>
    /// <param name="sampleRate">Sample rate in Hz, from 1 000 to 192 000.</param>
    /// <param name="duration">Duration in seconds, from 0.1 to 20.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="components"/> is null, empty, has more than 4 elements or a null element,
    /// or a value is outside its range, NaN or infinity.
    /// </exception>
    public static Signal Sum(IReadOnlyList<SineComponent> components, int sampleRate, double duration)
    {
        ArgumentNullException.ThrowIfNull(components);

        if (components.Count < MinComponents || components.Count > MaxComponents)
        {
            throw new ArgumentException(
                $"Between {MinComponents} and {MaxComponents} components are required, but there were {components.Count}.",
                nameof(components));
        }

        // Copied into arrays once so the hot loop below reads plain doubles.
        var frequencies = new double[components.Count];
        var amplitudes = new double[components.Count];
        var phases = new double[components.Count];

        for (var k = 0; k < components.Count; k++)
        {
            var component = components[k]
                ?? throw new ArgumentException($"Component {k} is null.", nameof(components));

            EnsureInRange(component.Frequency, MinFrequency, MaxFrequency, $"{nameof(components)}[{k}].{nameof(SineComponent.Frequency)}");
            EnsureInRange(component.Amplitude, MinAmplitude, MaxAmplitude, $"{nameof(components)}[{k}].{nameof(SineComponent.Amplitude)}");

            if (component.Phase is null)
            {
                throw new ArgumentNullException($"{nameof(components)}[{k}].{nameof(SineComponent.Phase)}");
            }

            frequencies[k] = component.Frequency;
            amplitudes[k] = component.Amplitude;
            phases[k] = component.Phase.Radians;
        }

        EnsureInRange(sampleRate, MinSampleRate, MaxSampleRate, nameof(sampleRate));
        EnsureInRange(duration, MinDuration, MaxDuration, nameof(duration));

        var count = (int)Math.Floor(duration * sampleRate + SampleCountEpsilon);
        var time = new double[count];
        var values = new double[count];

        // One pass: each sample adds up all components, so no per-component arrays are allocated.
        for (var n = 0; n < count; n++)
        {
            time[n] = (double)n / sampleRate;

            var sum = 0.0;

            for (var k = 0; k < frequencies.Length; k++)
            {
                // The phase is computed directly from n, never accumulated. The number of cycles
                // f · n / sampleRate is reduced to its fractional part before multiplying by 2π:
                // 2π · f · t itself reaches millions of radians for long signals and loses precision.
                var fractionalCycles = frequencies[k] * n % sampleRate / sampleRate;
                var angle = Angle.FromRadians(TwoPi * fractionalCycles + phases[k]);

                sum += amplitudes[k] * Trig.Sin(angle);
            }

            values[n] = sum;
        }

        return new Signal(time, values);
    }

    private static void EnsureInRange(double value, double min, double max, string paramName)
    {
        // Written as a negated range check so that NaN (for which every comparison is false) is rejected too.
        if (!(value >= min && value <= max))
        {
            throw new ArgumentOutOfRangeException(
                paramName, value, $"Value must be between {min} and {max}, but was {value}.");
        }
    }
}
