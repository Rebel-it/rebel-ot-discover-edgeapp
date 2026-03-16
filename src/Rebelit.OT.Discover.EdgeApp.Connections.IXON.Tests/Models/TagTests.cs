using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Tests.Models;

[TestFixture]
public class TagTests
{
    [Test]
    public void Aggregators_ByDefault_IsEmpty()
    {
        var tag = new Tag();

        Assert.That(tag.Aggregators, Is.Empty);
    }

    [Test]
    public void AllProperties_WhenAssigned_ReturnAssignedValues()
    {
        var source = new Source { PublicId = "src-1" };
        var variable = new Variable
        {
            PublicId = "var-1",
            Address = "40001",
            Name = "Temp",
            Slug = "temp",
            Type = "int16",
            Width = "1",
        };
        var logTrigger = new object();
        var onChangeExpiry = new object();

        var tag = new Tag
        {
            Source = source,
            Variable = variable,
            PublicId = "tag-1",
            TagId = 42,
            Slug = "tag-slug",
            LogEvent = "always",
            LoggingInterval = "1s",
            RetentionPolicy = "30d",
            Name = "my-tag",
            Aggregators = ["avg"],
            EdgeAggregator = "mean",
            LogTrigger = logTrigger,
            OnChangeExpiry = onChangeExpiry,
        };

        Assert.Multiple(() =>
        {
            Assert.That(tag.Source, Is.SameAs(source));
            Assert.That(tag.Variable, Is.SameAs(variable));
            Assert.That(tag.PublicId, Is.EqualTo("tag-1"));
            Assert.That(tag.TagId, Is.EqualTo(42));
            Assert.That(tag.Slug, Is.EqualTo("tag-slug"));
            Assert.That(tag.LogEvent, Is.EqualTo("always"));
            Assert.That(tag.LoggingInterval, Is.EqualTo("1s"));
            Assert.That(tag.RetentionPolicy, Is.EqualTo("30d"));
            Assert.That(tag.Name, Is.EqualTo("my-tag"));
            Assert.That(tag.Aggregators, Has.Count.EqualTo(1));
            Assert.That(tag.EdgeAggregator, Is.EqualTo("mean"));
            Assert.That(tag.LogTrigger, Is.SameAs(logTrigger));
            Assert.That(tag.OnChangeExpiry, Is.SameAs(onChangeExpiry));
        });
    }
}
