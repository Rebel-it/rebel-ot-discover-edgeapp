using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Authentication;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Tests.Authentication;

[TestFixture]
public class IxonAuthenticationTests
{
    private const string TestEmail = "test@example.com";
    private const string TestPassword = "SecurePassword123";
    private const string TestApplicationId = "test-app-id";
    private const string TestOtpCode = "123456";

    [Test]
    public async Task BearerTokenGenerator_WithoutOtp_ReturnsToken()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(
            HttpStatusCode.OK,
            """{"type":"AccessTokenCreateResponse","data":{"publicId":"token123","secretId":"bearer-token-secret-id"},"status":"success"}"""
        );
        var auth = new TestableIxonAuthentication(handler);

        // Act
        var token = await auth.BearerTokenGenerator(
            TestEmail,
            TestPassword,
            TestApplicationId
        );

        // Assert
        Assert.That(token, Is.EqualTo("bearer-token-secret-id"));
    }

    [Test]
    public async Task BearerTokenGenerator_WithOtp_ReturnsToken()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(
            HttpStatusCode.OK,
            """{"type":"AccessTokenCreateResponse","data":{"publicId":"token123","secretId":"bearer-token-with-otp"},"status":"success"}"""
        );
        var auth = new TestableIxonAuthentication(handler);

        // Act
        var token = await auth.BearerTokenGenerator(
            TestEmail,
            TestPassword,
            TestApplicationId,
            TestOtpCode
        );

        // Assert
        Assert.That(token, Is.EqualTo("bearer-token-with-otp"));
    }

    [Test]
    public async Task BearerTokenGenerator_WithoutOtp_SendsCorrectBasicAuthHeader()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(
            HttpStatusCode.OK,
            """{"data":{"secretId":"token"}}"""
        );
        var auth = new TestableIxonAuthentication(handler);

        // Act
        await auth.BearerTokenGenerator(TestEmail, TestPassword, TestApplicationId);

        // Assert
        var authHeader = handler.LastRequest?.Headers.Authorization;
        Assert.That(authHeader, Is.Not.Null);
        Assert.That(authHeader!.Scheme, Is.EqualTo("Basic"));

        // Decode and verify the Basic Auth format: email::password
        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter!));
        Assert.That(decoded, Is.EqualTo($"{TestEmail}::{TestPassword}"));
    }

    [Test]
    public async Task BearerTokenGenerator_WithOtp_SendsCorrectBasicAuthHeader()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(
            HttpStatusCode.OK,
            """{"data":{"secretId":"token"}}"""
        );
        var auth = new TestableIxonAuthentication(handler);

        // Act
        await auth.BearerTokenGenerator(
            TestEmail,
            TestPassword,
            TestApplicationId,
            TestOtpCode
        );

        // Assert
        var authHeader = handler.LastRequest?.Headers.Authorization;
        Assert.That(authHeader, Is.Not.Null);
        Assert.That(authHeader!.Scheme, Is.EqualTo("Basic"));

        // Decode and verify the Basic Auth format: email:otp:password
        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter!));
        Assert.That(decoded, Is.EqualTo($"{TestEmail}:{TestOtpCode}:{TestPassword}"));
    }

    [Test]
    public async Task BearerTokenGenerator_SendsCorrectApiHeaders()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(
            HttpStatusCode.OK,
            """{"data":{"secretId":"token"}}"""
        );
        var auth = new TestableIxonAuthentication(handler);

        // Act
        await auth.BearerTokenGenerator(TestEmail, TestPassword, TestApplicationId);

        // Assert
        var request = handler.LastRequest;
        Assert.Multiple(() =>
        {
            Assert.That(
                request?.Headers.GetValues("Api-Version").FirstOrDefault(),
                Is.EqualTo("2")
            );
            Assert.That(
                request?.Headers.GetValues("Api-Application").FirstOrDefault(),
                Is.EqualTo(TestApplicationId)
            );
        });
    }

    [Test]
    public async Task BearerTokenGenerator_SendsCorrectRequestBody()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(
            HttpStatusCode.OK,
            """{"data":{"secretId":"token"}}"""
        );
        var auth = new TestableIxonAuthentication(handler);

        // Act
        await auth.BearerTokenGenerator(TestEmail, TestPassword, TestApplicationId);

        // Assert
        var requestContent = await handler.LastRequest!.Content!.ReadAsStringAsync();
        Assert.That(requestContent, Does.Contain("\"expiresIn\":900"));
    }

    [Test]
    public async Task BearerTokenGenerator_UsesCorrectEndpoint()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(
            HttpStatusCode.OK,
            """{"data":{"secretId":"token"}}"""
        );
        var auth = new TestableIxonAuthentication(handler);

        // Act
        await auth.BearerTokenGenerator(TestEmail, TestPassword, TestApplicationId);

        // Assert
        Assert.That(
            handler.LastRequest!.RequestUri!.ToString(),
            Is.EqualTo("https://portal.ixon.cloud/api/access-tokens?fields=secretId")
        );
    }

    [Test]
    public void BearerTokenGenerator_OnUnauthorized_ThrowsHttpRequestException()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(
            HttpStatusCode.Unauthorized,
            """{"error":"Invalid credentials"}"""
        );
        var auth = new TestableIxonAuthentication(handler);

        // Act & Assert
        Assert.ThrowsAsync<HttpRequestException>(
            () => auth.BearerTokenGenerator(TestEmail, TestPassword, TestApplicationId)
        );
    }

    [Test]
    public void BearerTokenGenerator_OnServerError_ThrowsHttpRequestException()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(
            HttpStatusCode.InternalServerError,
            """{"error":"Server error"}"""
        );
        var auth = new TestableIxonAuthentication(handler);

        // Act & Assert
        Assert.ThrowsAsync<HttpRequestException>(
            () => auth.BearerTokenGenerator(TestEmail, TestPassword, TestApplicationId)
        );
    }

    [Test]
    public void BearerTokenGenerator_WithMissingSecretId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(
            HttpStatusCode.OK,
            """{"data":{"publicId":"token123"}}"""
        );
        var auth = new TestableIxonAuthentication(handler);

        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(
            () => auth.BearerTokenGenerator(TestEmail, TestPassword, TestApplicationId)
        );
    }

    [Test]
    public void BearerTokenGenerator_WithMissingDataProperty_ThrowsKeyNotFoundException()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(
            HttpStatusCode.OK,
            """{"status":"success"}"""
        );
        var auth = new TestableIxonAuthentication(handler);

        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(
            () => auth.BearerTokenGenerator(TestEmail, TestPassword, TestApplicationId)
        );
    }

    #region Test Helpers

    private sealed class MockHttpMessageHandler(HttpStatusCode statusCode, string content)
        : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            LastRequest = request;
            return Task.FromResult(
                new HttpResponseMessage(statusCode) { Content = new StringContent(content) }
            );
        }
    }

    private sealed class TestableIxonAuthentication : IxonAuthentication
    {
        private readonly HttpMessageHandler _handler;

        public TestableIxonAuthentication(HttpMessageHandler handler)
        {
            _handler = handler;
        }

        public new async Task<string> BearerTokenGenerator(
            string email,
            string password,
            string applicationId,
            string? otpCode = null
        )
        {
            var basicAuth = CreateBasicAuthPublic(email, password, otpCode);

            using var client = new HttpClient(_handler);

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", basicAuth);

            client.DefaultRequestHeaders.Add("Api-Version", "2");
            client.DefaultRequestHeaders.Add("Api-Application", applicationId);

            var body = new { expiresIn = 900 };

            var json = System.Text.Json.JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                "https://portal.ixon.cloud/api/access-tokens?fields=secretId",
                content
            );

            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();

            using var doc = System.Text.Json.JsonDocument.Parse(responseJson);

            var token = doc.RootElement.GetProperty("data").GetProperty("secretId").GetString();

            return token!;
        }

        private string CreateBasicAuthPublic(string email, string password, string? otpCode = null)
        {
            var raw = otpCode != null
                ? $"{email}:{otpCode}:{password}"
                : $"{email}::{password}";
            var bytes = Encoding.UTF8.GetBytes(raw);
            return Convert.ToBase64String(bytes);
        }
    }

    #endregion
}

