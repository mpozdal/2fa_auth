using System.Net.Http.Json;
using System.Text.Json;
using AuthService.Application.Contracts;
using AuthService.Application.Interfaces;
using TwoFactorService.Application.Contracts;

namespace AuthService.Infrastructure.HttpClients
{
    public class TwoFactorApiClient(HttpClient httpClient) : ITwoFactorApiClient
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task<ServiceResult> VerifyLoginCodeAsync(string email, string code)
        {
            var requestBody = new Verify2FARequest(email, code);

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
                "api/2fa//internal/verify-login",
                requestBody
            );

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = await response.Content.ReadFromJsonAsync<ServiceResult>(options);
                return result ?? ServiceResult.Fail("Empty 2FA response");
            }

            try
            {
                var errorResult = await response.Content.ReadFromJsonAsync<ServiceResult>();
                return errorResult ?? ServiceResult.Fail($"2FA service failed with status: {response.StatusCode}");
            }
            catch
            {
                return ServiceResult.Fail($"2FA service failed with status: {response.StatusCode}");
            }
        }

    }
}
