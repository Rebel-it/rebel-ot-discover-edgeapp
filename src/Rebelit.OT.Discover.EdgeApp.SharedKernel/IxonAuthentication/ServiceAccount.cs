namespace Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

public record ServiceAccount
{
    public required string ApiApplicationId { get; set; }
    public required string AccessToken { get; set; }
}