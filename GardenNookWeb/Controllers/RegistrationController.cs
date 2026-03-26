using GardenNookWeb.Models;
using GardenNookWeb.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using TransferModels.Clients;

namespace GardenNookWeb.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly HttpClient _http;

        public RegistrationController(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("api");
        }

        [HttpGet]
        public IActionResult RegistrationView()
        {
            return View(new RegistrationModel());
        }

        [HttpPost]
        public async Task<IActionResult> RegistrationView(RegistrationModel model)
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

            var response = await _http.PostAsJsonAsync(
                "api/auth/registration",
                new ClientRequest
                {
                    PhoneNumber = model.PhoneNumber,
                    Password = model.Password,
                    FullName = model.FullName
                });

            if (!response.IsSuccessStatusCode)
            {
                model.Error = "Ошибка сервера";
                return View(model);
            }

            var result = await response.Content.ReadFromJsonAsync<ClientResponse>();

            if (result?.Client == null)
            {
                model.Error = "Пользователь с таким телефоном уже существует";
                return View(model);
            }

            string name = result.Client.FullName ?? "Пользователь";
            model.SuccsesMessage = $"Пользователь {name} c номером телефона {model.PhoneNumber} успешно зарегистрирован!";
            model.FullName = string.Empty;
            model.PhoneNumber = string.Empty;
            model.Password = string.Empty;
            return View(model);
        }
    }
}
