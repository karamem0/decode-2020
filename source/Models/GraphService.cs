using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MySchedule.Models
{

    public class GraphService
    {

        private readonly GraphServiceClient graphServiceClient;

        public GraphService(string accessToken)
        {
            this.graphServiceClient = new GraphServiceClient(new AuthenticationProvider(accessToken));
        }

        public async Task<IEnumerable<Event>> GetEventsAsync()
        {
            return await this.graphServiceClient.Me.CalendarView.Request(new[]
            {
                new QueryOption("startDateTime", DateTime.Today.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")),
                new QueryOption("endDateTime", DateTime.Today.AddYears(1).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")),
            }).GetAsync();
        }

        public class AuthenticationProvider : IAuthenticationProvider
        {

            private readonly string accessToken;

            public AuthenticationProvider(string accessToken)
            {
                this.accessToken = accessToken;
            }

            public Task AuthenticateRequestAsync(HttpRequestMessage request)
            {
                return Task.Run(() =>
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.accessToken);
                });
            }

        }

    }

}
