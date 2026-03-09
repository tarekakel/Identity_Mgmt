using System.Net.Http.Json;
using System.Text.Json;
using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.Auth;
using AmlScreening.Application.Interfaces;
using AmlScreening.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AmlScreening.Infrastructure.Services;

/// <summary>
/// Proxies auth operations to the Identity Management API.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IdentityApiOptions _options;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IHttpClientFactory httpClientFactory,
        IOptions<IdentityApiOptions> options,
        ILogger<AuthService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        return await CallAuthEndpointAsync<LoginRequest>(
            "api/Auth/login",
            request,
            cancellationToken);
    }

    public async Task<ApiResponse<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        return await CallAuthEndpointAsync<RefreshTokenRequest>(
            "api/Auth/refresh",
            request,
            cancellationToken);
    }

    public async Task RevokeTokenAsync(string refreshToken, string? ipAddress, string? accessToken = null, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("IdentityApi");
        var body = new RevokeTokenRequest { RefreshToken = refreshToken };

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/Auth/revoke");
        request.Content = JsonContent.Create(body);
        if (!string.IsNullOrEmpty(accessToken))
        {
            var token = accessToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? accessToken["Bearer ".Length..]
                : accessToken;
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        try
        {
            var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                _logger.LogWarning("Identity API revoke returned {StatusCode}", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call Identity API revoke");
        }
    }

    private async Task<ApiResponse<LoginResponse>> CallAuthEndpointAsync<TRequest>(
        string path,
        TRequest requestBody,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("IdentityApi");

        try
        {
            var response = await client.PostAsJsonAsync(path, requestBody, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrEmpty(json))
                return ApiResponse<LoginResponse>.Fail("No response from authentication service.");

            var parsed = JsonSerializer.Deserialize<IdentityAuthResponseDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (parsed == null)
                return ApiResponse<LoginResponse>.Fail("Invalid response from authentication service.");

            if (response.IsSuccessStatusCode && parsed.Success && parsed.Data != null)
                return ApiResponse<LoginResponse>.Ok(parsed.Data);

            var message = parsed.Message ?? (parsed.Errors?.Count > 0 ? string.Join("; ", parsed.Errors) : "Authentication failed.");
            return ApiResponse<LoginResponse>.Fail(message, parsed.Errors ?? new List<string>());
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Identity API request failed");
            return ApiResponse<LoginResponse>.Fail("Authentication service is temporarily unavailable.");
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Identity API request timed out");
            return ApiResponse<LoginResponse>.Fail("Authentication service request timed out.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling Identity API");
            return ApiResponse<LoginResponse>.Fail("An error occurred during authentication.");
        }
    }
}
