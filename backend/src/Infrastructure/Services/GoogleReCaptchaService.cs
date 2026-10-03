using Crm.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;
using System.Net.Http.Json;

namespace Crm.Infrastructure.Services;

public class GoogleReCaptchaService : IReCaptchaService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public GoogleReCaptchaService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<bool> Validate(string secretCode)
    {
        var enabled = _configuration.GetValue<bool>("ReCaptcha:Enabled", false);
        if (!enabled)
        {
            return true;
        }

        var privateKey = _configuration["ReCaptcha:PrivateKey"];
        if (string.IsNullOrEmpty(privateKey))
        {
            return true;
        }

        var url = $"https://www.google.com/recaptcha/api/siteverify?secret={privateKey}&response={secretCode}";
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ReCaptchaResponse>(url);
            return response?.Success ?? false;
        }
        catch
        {
            return false;
        }
    }

    private class ReCaptchaResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("error-codes")]
        public List<string>? ErrorCodes { get; set; }
    }
}
