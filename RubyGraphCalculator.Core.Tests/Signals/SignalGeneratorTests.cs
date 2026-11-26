using RubyGraphCalculator.Core.Angles;
using RubyGraphCalculator.Core.Signals;

namespace RubyGraphCalculator.Core.Tests.Signals;

[TestFixture]
public sealed class SignalGeneratorTests
{
    private const double RelativeTolerance = 1e-12;
    private const double TimeTolerance = 1e-12;

    // Valid default values for the parameters a test does not vary.
    private const double ValidFrequency = 440.0;
    private const double ValidAmplitude = 1.0;
    private const int ValidSampleRate = 10000;
    private const double ValidDuration = 0.1;

    // frequency, amplitude, phase (degrees), sampleRate, duration.
    // Frequencies are integers or binary fractions (123.25), so frequency · n is exact in the reference.
    // 30 000 Hz at 10 000 Hz is above Nyquist: the signal must still be generated (aliasing is allowed).
    private static readonly object[] SignalCases =
    [
        new object[] { 440.0, 1.0, 0.0, 10000, 0.1 },
        new object[] { 1000.0, 2.5, 45.0, 44100, 0.5 },
        new object[] { 123.25, 0.7, 123.456, 48000, 0.25 },
        new object[] { 40.0, 0.1, 90.0, 1000, 1.0 },
        new object[] { 30000.0, 5.0, 270.0, 192000, 0.1 },
        new object[] { 30000.0, 1.0, 300.0, 10000, 0.1 },
    ];

    /// <summary>
    /// Reference value amplitude · sin(2π · f · tₙ + phase) with tₙ = n / sampleRate.
    /// </summary>
    /// <remarks>
    /// Computing 2π · f · tₙ directly loses precision for large arguments: for 30 000 Hz and 20 s the
    /// argument is about 3.8·10⁶ rad, where one double ulp is about 5·10⁻¹⁰ — far above the 1e-12 tolerance.
    /// The number of cycles f · n / sampleRate is therefore reduced to its fractional part first.
    /// For integer or binary-fraction frequencies f · n is exact, and so is the remainder.
    /// The result is mathematically identical to amplitude · Math.Sin(2π·f·tₙ + phase).
    /// </remarks>
    private static double Reference(double frequency, double amplitude, Angle phase, int sampleRate, int n)
    {
        var cycles = frequency * n % sampleRate / sampleRate;
        return amplitude * Math.Sin(2.0 * Math.PI * cycles + phase.Radians);
    }

    // The epsilon prevents losing a sample to rounding: 0.7 * 44100 = 30869.999999999996 in double.
    private static int ExpectedSampleCount(int sampleRate, double duration) =>
        (int)Math.Floor(duration * sampleRate + 1e-9);

    // Values

    [TestCaseSource(nameof(SignalCases))]
    public void Sin_EveryValue_MatchesReference(
        double frequency, double amplitude, double phaseDegrees, int sampleRate, double duration)
    {
        var phase = Angle.FromDegrees(phaseDegrees);

        var signal = SignalGenerator.Sin(frequency, amplitude, phase, sampleRate, duration);

        Assert.Multiple(() =>
        {
            for (var n = 0; n < signal.Values.Length; n++)
            {
                Assert.That(
                    signal.Values[n],
                    Is.EqualTo(Reference(frequency, amplitude, phase, sampleRate, n))
                        .Within(RelativeTolerance * amplitude),
                    $"Values[{n}]");
            }
        });
    }

    [TestCaseSource(nameof(SignalCases))]
    public void Sin_NoValueExceedsAmplitude(
        double frequency, double amplitude, double phaseDegrees, int sampleRate, double duration)
    {
        var signal = SignalGenerator.Sin(
            frequency, amplitude, Angle.FromDegrees(phaseDegrees), sampleRate, duration);

        // Trig.Sin may return 1.0000000000000002 at π/2 (last-bit rounding), so allow a tiny excess.
        Assert.That(signal.Values.Max(Math.Abs), Is.LessThanOrEqualTo(amplitude * (1 + RelativeTolerance)));
    }

    [TestCase(0.0)]
    [TestCase(30.0)]
    [TestCase(90.0)]
    [TestCase(123.456)]
    [TestCase(270.0)]
    [TestCase(-45.0)]
    public void Sin_FirstValue_EqualsAmplitudeTimesSinOfPhase(double phaseDegrees)
    {
        const double amplitude = 2.5;
        var phase = Angle.FromDegrees(phaseDegrees);

        var signal = SignalGenerator.Sin(ValidFrequency, amplitude, phase, ValidSampleRate, ValidDuration);

        Assert.That(
            signal.Values[0],
            Is.EqualTo(amplitude * Math.Sin(phase.Radians)).Within(RelativeTolerance * amplitude));
    }

    [Test]
    public void Sin_ZeroPhase_FirstValueIsZero()
    {
        var signal = SignalGenerator.Sin(
            ValidFrequency, ValidAmplitude, Angle.FromDegrees(0.0), ValidSampleRate, ValidDuration);

        Assert.That(signal.Values[0], Is.EqualTo(0.0).Within(RelativeTolerance * ValidAmplitude));
    }

    // Sample count and time axis

    [TestCase(1000, 0.1, 100)]
    [TestCase(10000, 0.1, 1000)]
    [TestCase(44100, 1.0, 44100)]
    [TestCase(48000, 0.25, 12000)]
    [TestCase(10000, 0.12345, 1234)]
    [TestCase(1000, 20.0, 20000)]
    [TestCase(44100, 0.7, 30870)]
    [TestCase(1000, 1.001, 1001)]
    public void Sin_SampleCount_IsFloorOfDurationTimesSampleRate(int sampleRate, double duration, int expected)
    {
        var signal = SignalGenerator.Sin(
            ValidFrequency, ValidAmplitude, Angle.FromDegrees(0.0), sampleRate, duration);

        Assert.Multiple(() =>
        {
            Assert.That(ExpectedSampleCount(sampleRate, duration), Is.EqualTo(expected), "test data");
            Assert.That(signal.Time, Has.Length.EqualTo(expected));
            Assert.That(signal.Values, Has.Length.EqualTo(expected));
        });
    }

    [TestCase(1000, 0.1)]
    [TestCase(44100, 0.5)]
    [TestCase(192000, 0.1)]
    public void Sin_Time_StartsAtZeroWithStepOfOneOverSampleRate(int sampleRate, double duration)
    {
        var signal = SignalGenerator.Sin(
            ValidFrequency, ValidAmplitude, Angle.FromDegrees(0.0), sampleRate, duration);
        var step = 1.0 / sampleRate;

        Assert.Multiple(() =>
        {
            Assert.That(signal.Time[0], Is.EqualTo(0.0).Within(TimeTolerance));

            for (var n = 0; n < signal.Time.Length; n++)
            {
                Assert.That(signal.Time[n], Is.EqualTo((double)n / sampleRate).Within(TimeTolerance), $"Time[{n}]");
            }

            for (var n = 1; n < signal.Time.Length; n++)
            {
                Assert.That(signal.Time[n] - signal.Time[n - 1], Is.EqualTo(step).Within(TimeTolerance), $"step {n}");
            }
        });
    }

    [TestCase(1000, 0.1)]
    [TestCase(44100, 0.5)]
    public void Sin_LastTime_IsOneStepBeforeDuration(int sampleRate, double duration)
    {
        var signal = SignalGenerator.Sin(
            ValidFrequency, ValidAmplitude, Angle.FromDegrees(0.0), sampleRate, duration);

        Assert.That(signal.Time[^1], Is.EqualTo(duration - 1.0 / sampleRate).Within(TimeTolerance));
    }

    // Long signal: catches phase error accumulated over millions of samples

    [TestCase(440.0, 0.0)]
    [TestCase(30000.0, 30.0)]
    public void Sin_LongSignal_LastValueMatchesReference(double frequency, double phaseDegrees)
    {
        const int sampleRate = 192000;
        const double duration = 20.0;
        const double amplitude = 1.0;
        var phase = Angle.FromDegrees(phaseDegrees);

        var signal = SignalGenerator.Sin(frequency, amplitude, phase, sampleRate, duration);
        var last = signal.Values.Length - 1;

        Assert.Multiple(() =>
        {
            Assert.That(signal.Values, Has.Length.EqualTo(3_840_000));
            Assert.That(signal.Time[last], Is.EqualTo(duration - 1.0 / sampleRate).Within(TimeTolerance));
            Assert.That(
                signal.Values[last],
                Is.EqualTo(Reference(frequency, amplitude, phase, sampleRate, last))
                    .Within(RelativeTolerance * amplitude));
        });
    }

    // Boundary values are valid

    [TestCase(40.0)]
    [TestCase(30000.0)]
    public void Sin_FrequencyAtBoundary_IsValid(double frequency)
    {
        AssertGeneratesValidSignal(frequency, ValidAmplitude, ValidSampleRate, ValidDuration);
    }

    [TestCase(0.1)]
    [TestCase(5.0)]
    public void Sin_AmplitudeAtBoundary_IsValid(double amplitude)
    {
        AssertGeneratesValidSignal(ValidFrequency, amplitude, ValidSampleRate, ValidDuration);
    }

    [TestCase(1000)]
    [TestCase(192000)]
    public void Sin_SampleRateAtBoundary_IsValid(int sampleRate)
    {
        AssertGeneratesValidSignal(ValidFrequency, ValidAmplitude, sampleRate, ValidDuration);
    }

    [TestCase(0.1)]
    [TestCase(20.0)]
    public void Sin_DurationAtBoundary_IsValid(double duration)
    {
        AssertGeneratesValidSignal(ValidFrequency, ValidAmplitude, 1000, duration);
    }

    private static void AssertGeneratesValidSignal(double frequency, double amplitude, int sampleRate, double duration)
    {
        var phase = Angle.FromDegrees(0.0);

        var signal = SignalGenerator.Sin(frequency, amplitude, phase, sampleRate, duration);
        var last = signal.Values.Length - 1;

        Assert.Multiple(() =>
        {
            Assert.That(signal.Values, Has.Length.EqualTo(ExpectedSampleCount(sampleRate, duration)));
            Assert.That(
                signal.Values[last],
                Is.EqualTo(Reference(frequency, amplitude, phase, sampleRate, last))
                    .Within(RelativeTolerance * amplitude));
        });
    }

    // Invalid input

    [TestCase(39.999)]
    [TestCase(30000.001)]
    [TestCase(0.0)]
    [TestCase(-440.0)]
    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    public void Sin_InvalidFrequency_ThrowsArgumentException(double frequency)
    {
        Assert.That(
            () => SignalGenerator.Sin(frequency, ValidAmplitude, Angle.FromDegrees(0.0), ValidSampleRate, ValidDuration),
            Throws.InstanceOf<ArgumentException>());
    }

    [TestCase(0.0999)]
    [TestCase(5.001)]
    [TestCase(0.0)]
    [TestCase(-1.0)]
    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    public void Sin_InvalidAmplitude_ThrowsArgumentException(double amplitude)
    {
        Assert.That(
            () => SignalGenerator.Sin(ValidFrequency, amplitude, Angle.FromDegrees(0.0), ValidSampleRate, ValidDuration),
            Throws.InstanceOf<ArgumentException>());
    }

    [TestCase(999)]
    [TestCase(192001)]
    [TestCase(0)]
    [TestCase(-10000)]
    public void Sin_InvalidSampleRate_ThrowsArgumentException(int sampleRate)
    {
        Assert.That(
            () => SignalGenerator.Sin(ValidFrequency, ValidAmplitude, Angle.FromDegrees(0.0), sampleRate, ValidDuration),
            Throws.InstanceOf<ArgumentException>());
    }

    [TestCase(0.0999)]
    [TestCase(20.001)]
    [TestCase(0.0)]
    [TestCase(-1.0)]
    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    public void Sin_InvalidDuration_ThrowsArgumentException(double duration)
    {
        Assert.That(
            () => SignalGenerator.Sin(ValidFrequency, ValidAmplitude, Angle.FromDegrees(0.0), ValidSampleRate, duration),
            Throws.InstanceOf<ArgumentException>());
    }
}
