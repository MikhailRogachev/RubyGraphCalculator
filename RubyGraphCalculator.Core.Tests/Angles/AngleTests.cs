using RubyGraphCalculator.Core.Angles;

namespace RubyGraphCalculator.Core.Tests.Angles;

[TestFixture]
public sealed class AngleTests
{
    private const double Tolerance = 1e-9;
    private const double RoundTripTolerance = 1e-12;
    private const double TwoPi = 2 * Math.PI;

    // Reference values: degrees <-> radians

    [TestCase(0.0, 0.0)]
    [TestCase(90.0, Math.PI / 2)]
    [TestCase(180.0, Math.PI)]
    [TestCase(270.0, 3 * Math.PI / 2)]
    public void FromDegrees_ReferenceValue_ReturnsExpectedRadians(double degrees, double expectedRadians)
    {
        var angle = Angle.FromDegrees(degrees);

        Assert.That(angle.Radians, Is.EqualTo(expectedRadians).Within(Tolerance));
    }

    [TestCase(0.0, 0.0)]
    [TestCase(Math.PI / 2, 90.0)]
    [TestCase(Math.PI, 180.0)]
    [TestCase(3 * Math.PI / 2, 270.0)]
    public void FromRadians_ReferenceValue_ReturnsExpectedDegrees(double radians, double expectedDegrees)
    {
        var angle = Angle.FromRadians(radians);

        Assert.That(angle.Degrees, Is.EqualTo(expectedDegrees).Within(Tolerance));
    }

    // Round trip for 0 <= x < 360

    [TestCase(0.0)]
    [TestCase(1e-10)]
    [TestCase(0.5)]
    [TestCase(1.0)]
    [TestCase(30.0)]
    [TestCase(45.0)]
    [TestCase(60.0)]
    [TestCase(90.0)]
    [TestCase(123.456)]
    [TestCase(180.0)]
    [TestCase(270.0)]
    [TestCase(359.0)]
    [TestCase(359.999999)]
    public void FromDegrees_InRange_RoundTripsToSameDegrees(double degrees)
    {
        var angle = Angle.FromDegrees(degrees);

        Assert.That(angle.Degrees, Is.EqualTo(degrees).Within(RoundTripTolerance));
    }

    // Round trip for 0 <= x < 2π

    [TestCase(0.0)]
    [TestCase(1e-10)]
    [TestCase(0.5)]
    [TestCase(1.0)]
    [TestCase(Math.PI / 2)]
    [TestCase(Math.PI)]
    [TestCase(4.0)]
    [TestCase(TwoPi - 1e-9)]
    public void FromRadians_InRange_RoundTripsToSameRadians(double radians)
    {
        var angle = Angle.FromRadians(radians);

        Assert.That(angle.Radians, Is.EqualTo(radians).Within(RoundTripTolerance));
    }

    // Normalization of degrees

    [TestCase(450.0, 90.0)]
    [TestCase(720.0, 0.0)]
    [TestCase(360.0, 0.0)]
    public void FromDegrees_FullTurnsOrMore_IsNormalized(double degrees, double expectedDegrees)
    {
        var angle = Angle.FromDegrees(degrees);

        Assert.That(angle.Degrees, Is.EqualTo(expectedDegrees).Within(Tolerance));
    }

    [TestCase(-90.0, 270.0)]
    [TestCase(-450.0, 270.0)]
    [TestCase(-360.0, 0.0)]
    [TestCase(-36000.0, 0.0)]
    public void FromDegrees_Negative_IsNormalized(double degrees, double expectedDegrees)
    {
        var angle = Angle.FromDegrees(degrees);

        Assert.That(angle.Degrees, Is.EqualTo(expectedDegrees).Within(Tolerance));
    }

    [Test]
    public void FromDegrees_TinyNegative_StaysBelowFullTurn()
    {
        var angle = Angle.FromDegrees(-1e-15);

        Assert.Multiple(() =>
        {
            Assert.That(angle.Degrees, Is.LessThan(360.0));
            Assert.That(angle.Degrees, Is.GreaterThanOrEqualTo(0.0));
            Assert.That(angle.Radians, Is.LessThan(TwoPi));
            Assert.That(angle.Radians, Is.GreaterThanOrEqualTo(0.0));
        });
    }

    // Normalization of radians

    [Test]
    public void FromRadians_MoreThanFullTurn_IsNormalized()
    {
        var angle = Angle.FromRadians(TwoPi + 0.1);

        Assert.That(angle.Radians, Is.EqualTo(0.1).Within(Tolerance));
    }

    [TestCase(TwoPi, 0.0)]
    [TestCase(2 * TwoPi, 0.0)]
    [TestCase(-Math.PI / 2, 3 * Math.PI / 2)]
    [TestCase(-(TwoPi + Math.PI / 2), 3 * Math.PI / 2)]
    public void FromRadians_OutOfRange_IsNormalized(double radians, double expectedRadians)
    {
        var angle = Angle.FromRadians(radians);

        Assert.That(angle.Radians, Is.EqualTo(expectedRadians).Within(Tolerance));
    }

    [Test]
    public void FromRadians_TinyNegative_StaysBelowFullTurn()
    {
        var angle = Angle.FromRadians(-1e-17);

        Assert.Multiple(() =>
        {
            Assert.That(angle.Radians, Is.LessThan(TwoPi));
            Assert.That(angle.Radians, Is.GreaterThanOrEqualTo(0.0));
            Assert.That(angle.Degrees, Is.LessThan(360.0));
        });
    }

    // Invalid input

    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    public void FromDegrees_NotFinite_ThrowsArgumentException(double degrees)
    {
        Assert.That(() => Angle.FromDegrees(degrees), Throws.InstanceOf<ArgumentException>());
    }

    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    public void FromRadians_NotFinite_ThrowsArgumentException(double radians)
    {
        Assert.That(() => Angle.FromRadians(radians), Throws.InstanceOf<ArgumentException>());
    }
}
