using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MySchedule.Models
{

    public class AuthService
    {

        private readonly HttpClient httpClient;

        public AuthService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task RefreshTokenAsync(string authority, string cookie)
        {
            var requestUrl = $"{authority}/.auth/refresh";
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            requestMessage.Headers.Add("Cookie", cookie);
            var responseMessage = await this.httpClient.SendAsync(requestMessage);
            if (!responseMessage.IsSuccessStatusCode)
            {
                var statusCode = (int)responseMessage.StatusCode;
                var reasonPhrase = responseMessage.ReasonPhrase;
                throw new InvalidOperationException($"{statusCode} {reasonPhrase}");
            }
        }

        public async Task<string> GetRefreshTokenAsync(string authority, string cookie)
        {
            var requestUrl = $"{authority}/.auth/me";
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            requestMessage.Headers.Add("Cookie", cookie);
            var responseMessage = await this.httpClient.SendAsync(requestMessage);
            if (!responseMessage.IsSuccessStatusCode)
            {
                var statusCode = (int)responseMessage.StatusCode;
                var reasonPhrase = responseMessage.ReasonPhrase;
                throw new InvalidOperationException($"{statusCode} {reasonPhrase}");
            }
            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<AuthToken[]>(responseContent);
            return responseJson[0].RefreshToken;
        }

        public async Task<string> GetAccessTokenAsync(string refreshToken)
        {
            var appConfig = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            var clientApplicationBuilder = ConfidentialClientApplicationBuilder
                .Create(appConfig["ClientId"])
                .WithHttpClientFactory(new MsalHttpClientFactory(this.httpClient))
                .WithTenantId(appConfig["TenantId"])
                .WithClientSecret(appConfig["ClientSecret"]);
            var clientApplication = clientApplicationBuilder.Build() as IByRefreshToken;
            var authenticationResult = await clientApplication.AcquireTokenByRefreshToken(
                new[] { "https://graph.microsoft.com/Calendars.Read" },
                refreshToken
            ).ExecuteAsync();
            return authenticationResult.AccessToken;
        }

        public class AuthToken
        {

            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }

            [JsonPropertyName("expires_on")]
            public DateTime ExpiresOn { get; set; }

            [JsonPropertyName("id_token")]
            public string IdToken { get; set; }

            [JsonPropertyName("refresh_token")]
            public string RefreshToken { get; set; }

            [JsonPropertyName("user_id")]
            public string UserId { get; set; }

        }

        public class MsalHttpClientFactory : IMsalHttpClientFactory
        {

            private readonly HttpClient httpClient;

            public MsalHttpClientFactory(HttpClient httpClient)
            {
                this.httpClient = httpClient;
            }

            public HttpClient GetHttpClient()
            {
                return this.httpClient;
            }

        }

    }

}
