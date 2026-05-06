namespace Rebelit.OT.Discover.EdgeApp.Dto;

public record ServiceAccountDto
{
    public required string ApiApplicationId { get; set; }
    public required string AccessToken { get; set; }
}