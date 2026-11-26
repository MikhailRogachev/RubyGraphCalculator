namespace RubyGraphCalculator.Core.Signals;

/// <summary>
/// A sampled signal: <see cref="Values"/>[n] is the signal value at time <see cref="Time"/>[n] (seconds).
/// </summary>
/// <remarks>
/// Record equality compares the arrays by reference, not by content.
/// </remarks>
public sealed record Signal(double[] Time, double[] Values)
{
    /// <summary>Sample times in seconds.</summary>
    public double[] Time { get; } = Time ?? throw new ArgumentNullException(nameof(Time));

    /// <summary>Signal values; one value per element of <see cref="Time"/>.</summary>
    public double[] Values { get; } = ValidateValues(Time, Values);

    private static double[] ValidateValues(double[]? time, double[] values)
    {
        ArgumentNullException.ThrowIfNull(values, nameof(Values));

        if (time is not null && time.Length != values.Length)
        {
            throw new ArgumentException(
                $"Time and Values must have the same length, but were {time.Length} and {values.Length}.",
                nameof(Values));
        }

        return values;
    }
}
