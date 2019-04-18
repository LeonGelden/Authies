using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace MvcClientWeb.Controllers
{
    [Authorize]
    public class ResourceApi1Controller : Controller
    {
        private readonly HttpClient _httpClient;

        public ResourceApi1Controller()
        {
            _httpClient = new HttpClient();
        }

        // GET: ResourceApi1
        public ActionResult Index()
        {
            // try and retrieve the access token from the claims
            var identity = User.Identity as ClaimsIdentity;
            var accessTokenClaim = identity.Claims.FirstOrDefault(cl => cl.Type.Equals("access_token"));

            if (string.IsNullOrWhiteSpace(accessTokenClaim?.Value))
            {
                
            }
            else
            {
                _httpClient.BaseAddress = new Uri("https://localhost:5001");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessTokenClaim.Value);

                HttpResponseMessage message = _httpClient.GetAsync("/api/values").Result;

                if (message.IsSuccessStatusCode)
                {
                    var inter = message.Content.ReadAsStringAsync();

                    List<string> result = JsonConvert.DeserializeObject<List<string>>(inter.Result);
                }
            }

            return View();
        }
    }
}