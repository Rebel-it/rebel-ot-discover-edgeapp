using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Authentication;

public class IxonAuthentication
{
    /// <summary>
    /// Asynchronously obtains a bearer token for API authentication using the specified user credentials and
    /// application identifier.
    /// </summary>
    /// <remarks>The returned bearer token is valid for a limited duration (typically 15 minutes). Ensure that
    /// the provided credentials and application ID are correct and that multi-factor authentication requirements are
    /// met if applicable.</remarks>
    /// <param name="email">The email address associated with the user account. Cannot be null or empty.</param>
    /// <param name="password">The password for the user account. Cannot be null or empty.</param>
    /// <param name="applicationId">The unique identifier of the application requesting the token. Cannot be null or empty.</param>
    /// <param name="otpCode">An optional one-time password (OTP) code for multi-factor authentication. If required by the account, this value
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
            expiresIn = 900 
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

}
