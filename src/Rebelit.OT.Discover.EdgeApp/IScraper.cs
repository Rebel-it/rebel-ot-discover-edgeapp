namespace Rebelit.OT.Discover.EdgeApp;

public interface IScraper
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}
