using Microsoft.AspNetCore.Mvc;
using TransferModels.Clients;
using TransferModels.Staff;
using TransferModels;
using GardenNookApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using GardenNookApi.Utils;

namespace GardenNookApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class UserController : Controller
    {
        private readonly AppDbContext database;

        public UserController(AppDbContext db)
        {
            database = db;
        }


        [HttpPost("staff")]
        public async Task<IActionResult> Staff(StaffRequest request)
        {
            var current = await database.Staff
                .AsNoTracking()
                .Include(s => s.Role)
                .FirstOrDefaultAsync(s => s.Login == request.Login && s.Password == request.Password);

            if (current == null)
                return Ok(new StaffResponse { User = null });

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, current.Id.ToString()),
                new Claim(ClaimTypes.Name, current.FullName ?? ""),
                new Claim("login", current.Login ?? ""),
                new Claim(ClaimTypes.Role, current.Role?.Name ?? "Staff") // или "Admin"/"Cashier"
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return Ok(new StaffResponse
            {
                User = new StaffDto
                {
                    FullName = current.FullName,
                    Login = current.Login,
                    Password = current.Password,
                    Role = current.Role.Name
                }
            });
        }

        [HttpPost("client")]
        public async Task<IActionResult> Client(ClientRequest request)
        {
            if (!PhoneNumberNormalizer.TryNormalizeRussian(request.PhoneNumber, out var normalizedPhone))
            {
                return Ok(new ClientResponse { Client = null });
            }

            var phoneVariants = PhoneNumberNormalizer.BuildVariants(normalizedPhone);

            var client = await database.Clients
                .AsNoTracking()
                .Include(q => q.ClientCategory)
                .FirstOrDefaultAsync(c =>
                    c.PhoneNumber != null &&
                    phoneVariants.Contains(c.PhoneNumber) &&
                    c.Password == request.Password);

            if (client == null)
            {
                return Ok(new ClientResponse { Client = null });
            }    

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, client.Id.ToString()),
                new Claim(ClaimTypes.Name, client.FullName ?? ""),
                new Claim("phone", client.PhoneNumber ?? ""),
                new Claim(ClaimTypes.Role, "Client"),
                new Claim("category", client.ClientCategory?.Name ?? "Новый")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = true }
            );

            return Ok(new ClientResponse
            {
                Client = new ClientDto
                {
                    FullName = client.FullName,
                    PhoneNumber = client.PhoneNumber,
                    Password = client.Password
                }
            });
        }

        [HttpPost("registration")]
        public IActionResult Registrartion(ClientRequest request)
        {
            if (!PhoneNumberNormalizer.TryNormalizeRussian(request.PhoneNumber, out var normalizedPhone))
            {
                return Ok(new ClientResponse { Client = null });
            }

            var phoneVariants = PhoneNumberNormalizer.BuildVariants(normalizedPhone);

            if (database.Clients.Any(c => c.PhoneNumber != null && phoneVariants.Contains(c.PhoneNumber)))
            {
                return Ok(new ClientResponse { Client = null });
            }
            Client newClient = new Client
            {
                FullName = request.FullName,
                PhoneNumber = normalizedPhone,
                Password = request.Password,
                ClientCategoryId = 1,
                OrderCount = 0
            };
            database.Clients.Add(newClient);
            database.SaveChanges();
            return Ok(new ClientResponse
            {
                Client = new ClientDto
                {
                    FullName = newClient.FullName,
                    PhoneNumber = newClient.PhoneNumber,
                    Password = newClient.Password
                }
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }
    }
}
