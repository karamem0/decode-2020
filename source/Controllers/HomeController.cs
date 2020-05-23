using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MySchedule.Helpers;
using MySchedule.Models;

namespace MySchedule.Controllers
{
    public class HomeController : Controller
    {

        private readonly HttpClient httpClient;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            this.httpClient = httpClientFactory.CreateClient();
        }

        [ActionName("Index")]
        public async Task<IActionResult> IndexAsync()
        {
            var accessToken = await TokenHelper.GetAccessTokenAsync(this.httpClient, this.HttpContext);
            var graphService = new GraphService(accessToken);
            var events = await graphService.GetEventsAsync();
            return this.View(events.ToArray());
        }

    }

}
