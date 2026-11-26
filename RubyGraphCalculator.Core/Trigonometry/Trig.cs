using RubyGraphCalculator.Core.Angles;

namespace RubyGraphCalculator.Core.Trigonometry;

/// <summary>Trigonometric functions implemented without System.Math.Sin / System.Math.Cos.</summary>
public static class Trig
{
    private const double HalfPi = Math.PI / 2.0;
    private const double ThreeHalvesPi = 3.0 * Math.PI / 2.0;
    private const double TwoPi = 2.0 * Math.PI;

    // On [0, π/2] the Taylor terms fall below double precision well before this many terms
    // (the 13th term for x = π/2 is about 1e-25). The limit only guards against an endless loop.
    private const int MaxTaylorTerms = 30;

    /// <summary>Returns the sine of <paramref name="angle"/>.</summary>
    public static double Sin(Angle angle)
    {
        ArgumentNullException.ThrowIfNull(angle);

        // Angle.Radians is already normalized to [0, 2π).
        var x = angle.Radians;

        // Reduce to [0, π/2] using the symmetries of the sine:
        //   sin(π - x) = sin x,  sin(π + x) = -sin x,  sin(2π - x) = -sin x.
        if (x <= HalfPi)
        {
            return SinTaylor(x);
        }

        if (x <= Math.PI)
        {
            return SinTaylor(Math.PI - x);
        }

        if (x <= ThreeHalvesPi)
        {
            return -SinTaylor(x - Math.PI);
        }

        return -SinTaylor(TwoPi - x);
    }

    /// <summary>Returns the cosine of <paramref name="angle"/>.</summary>
    public static double Cos(Angle angle)
    {
        ArgumentNullException.ThrowIfNull(angle);

        // cos x = sin(x + π/2). FromRadians normalizes the shifted value back to [0, 2π).
        return Sin(Angle.FromRadians(angle.Radians + HalfPi));
    }

    /// <summary>
    /// Evaluates the Taylor series sin x = x - x³/3! + x⁵/5! - ... for x in [0, π/2].
    /// </summary>
    private static double SinTaylor(double x)
    {
        var xSquared = x * x;
        var term = x;
        var sum = x;

        for (var n = 1; n < MaxTaylorTerms; n++)
        {
            // Next term from the previous one: t(n) = -t(n-1) · x² / ((2n)(2n + 1)).
            term *= -xSquared / ((2.0 * n) * (2.0 * n + 1.0));

            var next = sum + term;
            if (next == sum)
            {
                break;
            }

            sum = next;
        }

        return sum;
    }
}
