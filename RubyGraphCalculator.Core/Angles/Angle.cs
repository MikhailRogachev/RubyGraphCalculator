namespace RubyGraphCalculator.Core.Angles;

/// <summary>
/// An angle with an explicit unit. Stored internally in radians and normalized to [0, 2π).
/// </summary>
public sealed class Angle
{
    private const double FullTurnDegrees = 360.0;
    private const double FullTurnRadians = 2.0 * Math.PI;
    private const double RadiansPerDegree = Math.PI / 180.0;
    private const double DegreesPerRadian = 180.0 / Math.PI;

    private Angle(double radians)
    {
        Radians = radians;
    }

    /// <summary>The angle in radians, in the range [0, 2π).</summary>
    public double Radians { get; }

    /// <summary>The angle in degrees, in the range [0°, 360°).</summary>
    public double Degrees => WrapToFullTurn(Radians * DegreesPerRadian, FullTurnDegrees);

    /// <summary>
    /// Creates an angle from degrees. Full turns are removed in degrees first, then the value
    /// is converted to radians, so whole-degree angles are normalized exactly.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="degrees"/> is NaN or infinity.</exception>
    public static Angle FromDegrees(double degrees)
    {
        EnsureFinite(degrees, nameof(degrees));

        var normalizedDegrees = WrapToFullTurn(degrees, FullTurnDegrees);
        var radians = WrapToFullTurn(normalizedDegrees * RadiansPerDegree, FullTurnRadians);
        return new Angle(radians);
    }

    /// <summary>Creates an angle from radians, removing full turns.</summary>
    /// <exception cref="ArgumentException"><paramref name="radians"/> is NaN or infinity.</exception>
    public static Angle FromRadians(double radians)
    {
        EnsureFinite(radians, nameof(radians));

        return new Angle(WrapToFullTurn(radians, FullTurnRadians));
    }

    private static void EnsureFinite(double value, string paramName)
    {
        if (!double.IsFinite(value))
        {
            throw new ArgumentException($"Angle must be a finite number, but was {value}.", paramName);
        }
    }

    /// <summary>
    /// Maps a finite value into [0, fullTurn). The result is never equal to fullTurn and never -0.
    /// </summary>
    private static double WrapToFullTurn(double value, double fullTurn)
    {
        // The % remainder of two doubles is exact; its sign follows the dividend.
        var remainder = value % fullTurn;

        if (remainder < 0)
        {
            remainder += fullTurn;
        }

        // A tiny negative remainder (e.g. -1e-15) plus fullTurn rounds to exactly fullTurn.
        // The true value is just below a full turn, so 0 is its closest representable equivalent.
        // The same applies when a unit conversion rounds a value just below fullTurn up to it.
        // Comparing with 0 also turns -0.0 into +0.0.
        if (remainder >= fullTurn || remainder == 0)
        {
            return 0.0;
        }

        return remainder;
    }
}
