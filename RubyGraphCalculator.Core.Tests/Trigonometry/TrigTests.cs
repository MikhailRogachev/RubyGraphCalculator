using RubyGraphCalculator.Core.Angles;
using RubyGraphCalculator.Core.Trigonometry;

namespace RubyGraphCalculator.Core.Tests.Trigonometry;

[TestFixture]
public sealed class TrigTests
{
    private const double Tolerance = 1e-12;
    private const double RelativeTolerance = 1e-12;

    // Comparison with Math.Sin / Math.Cos of angle.Radians:
    // one point inside each quadrant, quadrant boundaries, a non-round value, a point near 360°.
    private static readonly double[] ReferenceDegrees =
    [
        45.0, 135.0, 225.0, 315.0,
        0.0, 90.0, 180.0, 270.0,
        123.456,
        359.0,
    ];

    // Several x in each quadrant, used for identities and reduction formulas.
    private static readonly double[] QuadrantDegrees =
    [
        10.0, 30.0, 60.0, 80.0,
        100.0, 135.0, 170.0,
        190.0, 225.0, 260.0,
        280.0, 315.0, 350.0,
    ];

    private static IEnumerable<double> DenseDegrees()
    {
        // Every 0.1° from 0° to 360°, computed from an integer step to avoid accumulated error.
        for (var i = 0; i <= 3600; i++)
        {
            yield return i / 10.0;
        }
    }

    // Reference comparison

    [TestCaseSource(nameof(ReferenceDegrees))]
    public void Sin_MatchesMathSinOfRadians(double degrees)
    {
        var angle = Angle.FromDegrees(degrees);

        Assert.That(Trig.Sin(angle), Is.EqualTo(Math.Sin(angle.Radians)).Within(Tolerance));
    }

    [TestCaseSource(nameof(ReferenceDegrees))]
    public void Cos_MatchesMathCosOfRadians(double degrees)
    {
        var angle = Angle.FromDegrees(degrees);

        Assert.That(Trig.Cos(angle), Is.EqualTo(Math.Cos(angle.Radians)).Within(Tolerance));
    }

    // Exact values

    [TestCase(30.0, 0.5)]
    [TestCase(90.0, 1.0)]
    [TestCase(270.0, -1.0)]
    public void Sin_ExactValue(double degrees, double expected)
    {
        Assert.That(Trig.Sin(Angle.FromDegrees(degrees)), Is.EqualTo(expected).Within(Tolerance));
    }

    [TestCase(0.0, 1.0)]
    [TestCase(60.0, 0.5)]
    [TestCase(180.0, -1.0)]
    public void Cos_ExactValue(double degrees, double expected)
    {
        Assert.That(Trig.Cos(Angle.FromDegrees(degrees)), Is.EqualTo(expected).Within(Tolerance));
    }

    // Properties

    [TestCase(0.0)]
    [TestCase(123.456)]
    [TestCase(359.0)]
    [TestCaseSource(nameof(QuadrantDegrees))]
    public void SinSquaredPlusCosSquared_IsOne(double degrees)
    {
        var angle = Angle.FromDegrees(degrees);
        var sin = Trig.Sin(angle);
        var cos = Trig.Cos(angle);

        Assert.That(sin * sin + cos * cos, Is.EqualTo(1.0).Within(Tolerance));
    }

    [TestCase(123.456)]
    [TestCaseSource(nameof(QuadrantDegrees))]
    public void Sin_IsOdd(double degrees)
    {
        var sinX = Trig.Sin(Angle.FromDegrees(degrees));
        var sinMinusX = Trig.Sin(Angle.FromDegrees(-degrees));

        Assert.That(sinMinusX, Is.EqualTo(-sinX).Within(Tolerance));
    }

    // Reduction formulas

    [TestCaseSource(nameof(QuadrantDegrees))]
    public void Sin_OfPiMinusX_EqualsSinX(double degrees)
    {
        var sinX = Trig.Sin(Angle.FromDegrees(degrees));
        var reduced = Trig.Sin(Angle.FromDegrees(180.0 - degrees));

        Assert.That(reduced, Is.EqualTo(sinX).Within(Tolerance));
    }

    [TestCaseSource(nameof(QuadrantDegrees))]
    public void Sin_OfPiPlusX_EqualsMinusSinX(double degrees)
    {
        var sinX = Trig.Sin(Angle.FromDegrees(degrees));
        var reduced = Trig.Sin(Angle.FromDegrees(180.0 + degrees));

        Assert.That(reduced, Is.EqualTo(-sinX).Within(Tolerance));
    }

    [TestCaseSource(nameof(QuadrantDegrees))]
    public void Sin_OfTwoPiMinusX_EqualsMinusSinX(double degrees)
    {
        var sinX = Trig.Sin(Angle.FromDegrees(degrees));
        var reduced = Trig.Sin(Angle.FromDegrees(360.0 - degrees));

        Assert.That(reduced, Is.EqualTo(-sinX).Within(Tolerance));
    }

    // Dense check: every 0.1° from 0° to 360°

    [Test]
    public void Sin_DenseGrid_MatchesMathSin()
    {
        Assert.Multiple(() =>
        {
            foreach (var degrees in DenseDegrees())
            {
                var angle = Angle.FromDegrees(degrees);

                Assert.That(
                    Trig.Sin(angle),
                    Is.EqualTo(Math.Sin(angle.Radians)).Within(Tolerance),
                    $"sin({degrees}°)");
            }
        });
    }

    [Test]
    public void Cos_DenseGrid_MatchesMathCos()
    {
        Assert.Multiple(() =>
        {
            foreach (var degrees in DenseDegrees())
            {
                var angle = Angle.FromDegrees(degrees);

                Assert.That(
                    Trig.Cos(angle),
                    Is.EqualTo(Math.Cos(angle.Radians)).Within(Tolerance),
                    $"cos({degrees}°)");
            }
        });
    }

    // Very small angles: sin x ≈ x, so an absolute tolerance of 1e-12 would accept 0.
    // A relative tolerance is used instead.

    [TestCase(1e-10)]
    [TestCase(1e-13)]
    public void Sin_VerySmallAngle_MatchesWithRelativeTolerance(double radians)
    {
        var angle = Angle.FromRadians(radians);
        var expected = Math.Sin(angle.Radians);

        Assert.That(Trig.Sin(angle), Is.EqualTo(expected).Within(Math.Abs(expected) * RelativeTolerance));
    }

    // Sign near quadrant boundaries

    [Test]
    public void Sin_JustBelow180_IsPositive()
    {
        Assert.That(Trig.Sin(Angle.FromDegrees(179.999)), Is.GreaterThan(0.0));
    }

    [Test]
    public void Sin_JustAbove180_IsNegative()
    {
        Assert.That(Trig.Sin(Angle.FromDegrees(180.001)), Is.LessThan(0.0));
    }

    [Test]
    public void Cos_JustBelow90_IsPositive()
    {
        Assert.That(Trig.Cos(Angle.FromDegrees(89.999)), Is.GreaterThan(0.0));
    }

    [Test]
    public void Cos_JustAbove90_IsNegative()
    {
        Assert.That(Trig.Cos(Angle.FromDegrees(90.001)), Is.LessThan(0.0));
    }
}
