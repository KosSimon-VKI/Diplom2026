using GardenNookWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using TransferModels.Menu;

namespace GardenNookWeb.Controllers
{
    public class MainController : Controller
    {
        private const string ApiAuthCookieName = "GardenNook.Auth";
        private readonly HttpClient _http;

        public MainController(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("api");
        }

        [HttpGet("/Menu")]
        public async Task<IActionResult> MenuView()
        {
            try
            {
                if (!Request.Cookies.TryGetValue(ApiAuthCookieName, out var authCookie) || string.IsNullOrWhiteSpace(authCookie))
                    return RedirectToAction("AuthorizationView", "Authorization");

                using var request = new HttpRequestMessage(HttpMethod.Get, "api/menu");
                request.Headers.Add("Cookie", $"{ApiAuthCookieName}={authCookie}");

                using var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode == 401)
                        return RedirectToAction("AuthorizationView", "Authorization");

                    return View(new MenuResponse());
                }

                var menu = await response.Content.ReadFromJsonAsync<MenuResponse>();
                return View(menu ?? new MenuResponse());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                return View(new MenuResponse());
            }
        }

        [HttpGet("/Cart")]
        public IActionResult Cart()
        {
            return View("CartView");
        }

        [HttpGet("/Profile")]
        public IActionResult Profile()
        {
            if (!Request.Cookies.TryGetValue(ApiAuthCookieName, out var authCookie) || string.IsNullOrWhiteSpace(authCookie))
                return RedirectToAction("AuthorizationView", "Authorization");

            return View("ProfileView");
        }
    }
}
