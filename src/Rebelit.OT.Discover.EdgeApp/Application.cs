using Microsoft.Extensions.Logging;

namespace Rebelit.OT.Discover.EdgeApp;

public class Application(IScraper scraper, ILogger<Application> logger)
{
    public async Task Run(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("Starting scraping process...");
                await scraper.ExecuteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during scraping: {Message}", ex.Message);
            }
            finally
            {
                logger.LogInformation("Scraping process completed. Waiting for the next cycle...");
                await Task.Delay(TimeSpan.FromMinutes(15), cancellationToken);
            }
        }
    }
}
