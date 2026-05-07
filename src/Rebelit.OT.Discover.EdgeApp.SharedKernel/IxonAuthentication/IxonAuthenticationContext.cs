namespace Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

public class IxonAuthenticationContext : IIxonAuthenticationContext
{
    /// <summary>
    ///     Property can be null because the action filter will return a bad request
    ///     if the headers are missing.
    /// </summary>
    public IxonHeaders IxonHeaders { get; set; } = null!;
}
