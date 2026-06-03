using System.Text.RegularExpressions;
using Rebelit.OT.Discover.EdgeApp.API.Utilities;

namespace Rebelit.OT.Discover.EdgeApp.Tests;

[TestFixture]
public partial class SlugGeneratorTests
{
    [GeneratedRegex("^[a-z][a-z0-9_-]{0,63}$")]
    private static partial Regex SlugRegex();

    [Test]
    public void CreateFromNameAndAddress_WithNumericNodeId_AppendsNodeIdWithoutEquals()
    {
        var result = SlugGenerator.CreateFromNameAndAddress("Test", "ns=2;i=2502");

        Assert.That(result, Is.EqualTo("test_i2502"));
    }

    [Test]
    public void CreateFromNameAndAddress_WithStringNodeId_AppendsNodeIdWithoutEquals()
    {
        var result = SlugGenerator.CreateFromNameAndAddress("Test", "ns=2;s=BladeMotor17Temperature.pMinProcessValue");

        Assert.That(result, Is.EqualTo("test_sblademotor17temperature_pminprocessvalue"));
    }

    [Test]
    public void CreateFromNameAndAddress_ReplacesUnsupportedCharacters_AndMatchesExpectedPattern()
    {
        var result = SlugGenerator.CreateFromNameAndAddress("My Test.Name", "ns=2;s=A/B:C");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("mytestname_sa_b_c"));
            Assert.That(SlugRegex().IsMatch(result), Is.True);
        });
    }

    [Test]
    public void CreateFromNameAndAddress_PrefixesWithA_WhenFirstCharacterIsNotLetter()
    {
        var result = SlugGenerator.CreateFromNameAndAddress("123", "ns=2;i=2502");

        Assert.That(result, Is.EqualTo("a123_i2502"));
    }

    [Test]
    public void CreateFromNameAndAddress_TrimsToMaxLength64()
    {
        var longName = new string('x', 70);

        var result = SlugGenerator.CreateFromNameAndAddress(longName, "ns=2;i=2502");

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Length.EqualTo(64));
            Assert.That(SlugRegex().IsMatch(result), Is.True);
        });
    }

    [Test]
    public void CreateFromNameAndAddress_WithNullNameAndAddress_ReturnsValidFallback()
    {
        var result = SlugGenerator.CreateFromNameAndAddress(null, null);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("a"));
            Assert.That(SlugRegex().IsMatch(result), Is.True);
        });
    }
}
