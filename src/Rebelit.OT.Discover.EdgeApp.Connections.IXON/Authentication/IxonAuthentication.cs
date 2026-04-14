using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Authentication;

public class IxonAuthentication
{
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

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

        var body = new
        {
            expiresIn = 900 // Token validity duration in seconds (15 minutes) 
        };

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

    /// <summary>
    /// Asynchronously retrieves a valid bearer token for the specified user and application, refreshing the token if
    /// necessary.
    /// </summary>
    /// value must be provided.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a valid bearer token as a string.</returns>
    public async Task<string> GetValidTokenAsync(string email, string password, 
        string applicationId, string? otpCode = null)
    {
        if (_cachedToken != null && DateTime.UtcNow < _tokenExpiry)
        {
            return _cachedToken;
        }

        await _refreshLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            if (_cachedToken != null && DateTime.UtcNow < _tokenExpiry)
            {
                return _cachedToken;
            }

            _cachedToken = await BearerTokenGenerator(email, password, applicationId, otpCode);
            // Set expiry slightly before actual expiry (e.g., 30 seconds buffer)
            _tokenExpiry = DateTime.UtcNow.AddSeconds(870); // 870 = 900 - 30
            
            return _cachedToken;
        }
        finally
        {
            _refreshLock.Release();
        }
    }
}
