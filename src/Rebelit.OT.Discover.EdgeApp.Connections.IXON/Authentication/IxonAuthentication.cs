using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Authentication;

public class IxonAuthentication
{
    private const int ExpiresIn = 14400; // 4 hours in seconds

    /// <summary>
    /// Asynchronously obtains a bearer token for API authentication using the specified user credentials and
    /// application identifier.
    /// </summary>
    /// must be provided.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the bearer token as a string, which
    /// can be used for authenticated API requests.</returns>
    public async Task<string> BearerTokenGenerator(string email, string password, string applicationId, string? otpCode = null)
    {
        var basicAuth = CreateBasicAuth(email, password, otpCode);

        using var client = new HttpClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", basicAuth);

        client.DefaultRequestHeaders.Add("Api-Version", "2");
        client.DefaultRequestHeaders.Add("Api-Application", applicationId);

        var body = new { expiresIn = ExpiresIn };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(
            "https://portal.ixon.cloud/api/access-tokens?fields=secretId",
            content);

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseJson);

        // The actual bearer token is in "secretId"
        var token = doc.RootElement
                       .GetProperty("data")
                       .GetProperty("secretId")
                       .GetString();

        return token!;
    }

    public async Task<string> CompanyIdAsync(string bearerToken, string applicationId)
    {
        using var client = new HttpClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", bearerToken);

        client.DefaultRequestHeaders.Add("Api-Version", "2");
        client.DefaultRequestHeaders.Add("Api-Application", applicationId);

        var response = await client.GetAsync(
            "https://portal.ixon.cloud/api/companies?fields=publicId");

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseJson);

        var companyId = doc.RootElement
            .GetProperty("data")
            .EnumerateArray()
            .First()
            .GetProperty("publicId")
            .GetString();

        return companyId!;
    }


    /// <summary>
    /// Creates a Base64-encoded Basic Authentication credential string using the specified email, password, and
    /// optional one-time password (OTP) code.
    /// </summary>
    /// <returns>A Base64-encoded string representing the Basic Authentication credentials, formatted as "email:otpCode:password"
    /// if an OTP code is provided, or "email::password" if not.</returns>
    private string CreateBasicAuth(string email, string password, string? otpCode = null)
    {
        var raw = otpCode != null ? $"{email}:{otpCode}:{password}" : $"{email}::{password}";
        var bytes = Encoding.UTF8.GetBytes(raw);
        return Convert.ToBase64String(bytes);
    }
}
