using GardenNookWeb.Models;
using GardenNookWeb.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using TransferModels.Clients;

namespace GardenNookWeb.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly HttpClient _http;

        public AuthorizationController(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("api");
        }

        [HttpGet]
        public IActionResult AuthorizationView()
        {
            return View(new AuthorizationModel());
        }

        [HttpPost]
        public async Task<IActionResult> AuthorizationView(AuthorizationModel model)
        {
            if (PhoneNumberNormalizer.TryNormalizeRussian(model.PhoneNumber, out var normalizedPhone))
            {
                model.PhoneNumber = normalizedPhone;
            }

            ModelState.Clear();
            TryValidateModel(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _http.PostAsJsonAsync("api/auth/client", new ClientRequest
            {
                PhoneNumber = model.PhoneNumber,
                Password = model.Password
            });

            if (!response.IsSuccessStatusCode)
            {
                model.Error = "Ошибка сервера";
                return View(model);
            }

            var result = await response.Content.ReadFromJsonAsync<ClientResponse>();
            if (result?.Client == null)
            {
                model.Error = "Неверный телефон или пароль";
                return View(model);
            }

            return RedirectToAction("MenuView", "Main");
        }
    }
}
