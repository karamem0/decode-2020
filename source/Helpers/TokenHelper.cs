using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.IdentityModel.JsonWebTokens;
using MySchedule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MySchedule.Helpers
{

    public static class TokenHelper
    {

        public async static Task<string> GetAccessTokenAsync(HttpClient httpClient, HttpContext httpContext)
        {
            if (httpContext.Session.Keys.Contains("AccessToken"))
            {
                var cachedToken = Encoding.UTF8.GetString(httpContext.Session.Get("AccessToken"));
                var jwtToken = new JsonWebToken(cachedToken);
                var expireIn = jwtToken.GetPayloadValue<double>("exp");
                var expireOn = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(expireIn);
                if (expireOn > DateTime.UtcNow)
                {
                    return cachedToken;
                }
            }
            var authService = new AuthService(httpClient);
            var requestAuthority = new Uri(httpContext.Request.GetDisplayUrl()).GetLeftPart(UriPartial.Authority);
            var requestCookies = string.Join(";", httpContext.Request.Cookies.Select(value => $"{value.Key}={value.Value}"));
            await authService.RefreshTokenAsync(requestAuthority, requestCookies);
            var refreshToken = await authService.GetRefreshTokenAsync(requestAuthority, requestCookies);
            var accessToken = await authService.GetAccessTokenAsync(refreshToken);
            httpContext.Session.Set("AccessToken", Encoding.UTF8.GetBytes(accessToken));
            return accessToken;
        }

    }

}
