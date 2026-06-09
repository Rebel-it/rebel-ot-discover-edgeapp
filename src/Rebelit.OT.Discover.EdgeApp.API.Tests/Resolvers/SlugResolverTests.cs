using Rebelit.OT.Discover.EdgeApp.API.Resolvers;

namespace Rebelit.OT.Discover.EdgeApp.Tests.Resolvers;

[TestFixture]
public class SlugResolverTests
{
    [Test]
    public void Resolve_WhenInputIsNull_ReturnsOpcua()
    {
        var result = SlugResolver.Resolve(null!);

        Assert.That(result, Is.EqualTo("opcua"));
    }

    [Test]
    public void Resolve_WhenInputIsEmpty_ReturnsOpcua()
    {
        var result = SlugResolver.Resolve(string.Empty);

        Assert.That(result, Is.EqualTo("opcua"));
    }

    [Test]
    public void Resolve_WhenInputContainsSpacesUnderscoresAndPeriods_ReplacesWithHyphensAndLowercases()
    {
        var result = SlugResolver.Resolve("My_Source.Name Value");

        Assert.That(result, Is.EqualTo("my-source-name-value"));
    }

    [Test]
    public void Resolve_WhenInputContainsUnsupportedCharacters_RemovesThem()
    {
        var result = SlugResolver.Resolve("Temp@#%Signal!");

        Assert.That(result, Is.EqualTo("tempsignal"));
    }

    [Test]
    public void Resolve_WhenInputCreatesLeadingAndTrailingHyphens_TrimsHyphens()
    {
        var result = SlugResolver.Resolve("__My Source.__");

        Assert.That(result, Is.EqualTo("my-source"));
    }

    [Test]
    public void Resolve_WhenInputContainsNoValidCharacters_ReturnsOpcua()
    {
        var result = SlugResolver.Resolve("@@@...");

        Assert.That(result, Is.EqualTo("opcua"));
    }
}
