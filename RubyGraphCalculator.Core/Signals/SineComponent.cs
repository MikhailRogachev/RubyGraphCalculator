using RubyGraphCalculator.Core.Angles;

namespace RubyGraphCalculator.Core.Signals;

/// <summary>
/// One sine wave of a sum: amplitude · sin(2π · frequency · t + phase).
/// </summary>
/// <param name="Frequency">Frequency in Hz, from 40 to 30 000.</param>
/// <param name="Amplitude">Amplitude, from 0.1 to 5.</param>
/// <param name="Phase">Initial phase.</param>
/// <remarks>
/// The ranges are checked by <see cref="SignalGenerator.Sum"/>, not by the record itself.
/// </remarks>
public sealed record SineComponent(double Frequency, double Amplitude, Angle Phase);
