using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Tests.Models;

[TestFixture]
public class VariableTests
{
    [Test]
    public void Signed_ByDefault_IsNull()
    {
        var variable = new Variable
        {
            PublicId = "v",
            Address = "a",
            Name = "n",
            Slug = "s",
            Type = "t",
            Width = "w",
        };

        Assert.That(variable.Signed, Is.Null);
    }

    [Test]
    public void AllProperties_WhenAssigned_ReturnAssignedValues()
    {
        var source = new Source { PublicId = "src-1" };

        var variable = new Variable
        {
            PublicId = "var-1",
            Address = "40001",
            Name = "Temperature",
            Slug = "temperature",
            Type = "int16",
            Width = "1",
            Source = source,
            Signed = true,
        };

        Assert.Multiple(() =>
        {
            Assert.That(variable.PublicId, Is.EqualTo("var-1"));
            Assert.That(variable.Address, Is.EqualTo("40001"));
            Assert.That(variable.Name, Is.EqualTo("Temperature"));
            Assert.That(variable.Slug, Is.EqualTo("temperature"));
            Assert.That(variable.Type, Is.EqualTo("int16"));
            Assert.That(variable.Width, Is.EqualTo("1"));
            Assert.That(variable.Source, Is.SameAs(source));
            Assert.That(variable.Signed, Is.True);
        });
    }
}
