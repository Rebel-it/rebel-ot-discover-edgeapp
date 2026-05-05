namespace Rebelit.OT.Discover.EdgeApp.Dto;

public record CredentialsDto
{
    public required string ApiApplicationId { get; set; }
    public required string AccessToken { get; set; }
}