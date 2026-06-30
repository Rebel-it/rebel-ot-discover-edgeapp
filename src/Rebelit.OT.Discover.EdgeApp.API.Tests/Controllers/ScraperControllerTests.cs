using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.API;
using Rebelit.OT.Discover.EdgeApp.API.Controllers;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.Tests.Controllers;

[TestFixture]
public class ScraperControllerTests
{
    [Test]
    public async Task RunVariableScrapeAsync_WhenScrapeReturnsVariables_ReturnsOkWithCount()
    {
        // Arrange
        IReadOnlyList<Variable> variables =
        [
            new() { Name = "Var1", Address = "ns=2;s=Var1", Slug = "var1", Type = "bool" },
            new() { Name = "Var2", Address = "ns=2;s=Var2", Slug = "var2", Type = "int" },
            new() { Name = "Var3", Address = "ns=2;s=Var3", Slug = "var3", Type = "float" },
        ];

        var scraper = new FakeScraper(variables);
        var sut = new ScraperController(scraper);
        using var cts = new CancellationTokenSource();

        // Act
        var result = await sut.RunVariableScrapeAsync(cts.Token);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(scraper.CallCount, Is.EqualTo(1));
            Assert.That(scraper.LastCancellationToken, Is.EqualTo(cts.Token));

            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.Not.Null);

            var countProperty = okResult.Value!.GetType().GetProperty("count");
            Assert.That(countProperty, Is.Not.Null);

            var countValue = countProperty!.GetValue(okResult.Value);
            Assert.That(countValue, Is.EqualTo(3));
        });
    }

    [Test]
    public async Task RunVariableScrapeAsync_WhenNoVariablesFound_ReturnsOkWithZeroCount()
    {
        // Arrange
        IReadOnlyList<Variable> variables = [];
        var scraper = new FakeScraper(variables);
        var sut = new ScraperController(scraper);

        // Act
        var result = await sut.RunVariableScrapeAsync(CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(scraper.CallCount, Is.EqualTo(1));

            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.Not.Null);

            var countProperty = okResult.Value!.GetType().GetProperty("count");
            Assert.That(countProperty, Is.Not.Null);

            var countValue = countProperty!.GetValue(okResult.Value);
            Assert.That(countValue, Is.EqualTo(0));
        });
    }

    private sealed class FakeScraper(IReadOnlyList<Variable> variablesToReturn) : IScraper
    {
        public int CallCount { get; private set; }
        public CancellationToken LastCancellationToken { get; private set; }

        public Task<IReadOnlyList<Variable>> ScrapeVariablesAsync(CancellationToken cancellationToken)
        {
            CallCount++;
            LastCancellationToken = cancellationToken;
            return Task.FromResult(variablesToReturn);
        }
    }
}
