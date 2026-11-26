using RubyGraphCalculator.Core.Signals;

namespace RubyGraphCalculator.Core.Tests.Signals;

[TestFixture]
public sealed class SignalTests
{
    [Test]
    public void Constructor_StoresTimeAndValues()
    {
        var time = new[] { 0.0, 0.5, 1.0 };
        var values = new[] { 1.0, 2.0, 3.0 };

        var signal = new Signal(time, values);

        Assert.Multiple(() =>
        {
            Assert.That(signal.Time, Is.SameAs(time));
            Assert.That(signal.Values, Is.SameAs(values));
        });
    }

    [Test]
    public void Constructor_EmptyArrays_IsAllowed()
    {
        var signal = new Signal([], []);

        Assert.Multiple(() =>
        {
            Assert.That(signal.Time, Is.Empty);
            Assert.That(signal.Values, Is.Empty);
        });
    }

    [Test]
    public void Deconstruct_ReturnsTimeAndValues()
    {
        var time = new[] { 0.0, 1.0 };
        var values = new[] { 3.0, 4.0 };

        var (t, v) = new Signal(time, values);

        Assert.Multiple(() =>
        {
            Assert.That(t, Is.SameAs(time));
            Assert.That(v, Is.SameAs(values));
        });
    }

    [Test]
    public void Constructor_NullTime_ThrowsArgumentNullException()
    {
        Assert.That(() => new Signal(null!, [1.0]), Throws.ArgumentNullException);
    }

    [Test]
    public void Constructor_NullValues_ThrowsArgumentNullException()
    {
        Assert.That(() => new Signal([0.0], null!), Throws.ArgumentNullException);
    }

    [Test]
    public void Constructor_DifferentLengths_ThrowsArgumentException()
    {
        Assert.That(() => new Signal([0.0, 1.0], [1.0]), Throws.ArgumentException);
    }
}
