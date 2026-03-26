using System.Security.Claims;
using GardenNookApi.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransferModels.Profile;

namespace GardenNookApi.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private const string CancelledStatusNameRu = "Отменен";
        private const string CancelledStatusTokenRu = "отмен";
        private const string CancelledStatusTokenEn = "cancel";
        private const string CompositionTypeDish = "Блюдо";
        private const string CompositionTypeDrink = "Напиток";
        private const string CompositionTypeTopping = "Добавка";

        private readonly AppDbContext _db;

        public ProfileController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<ProfileResponse>> GetProfile()
        {
            var clientId = TryGetClientId();
            if (clientId == null)
                return Unauthorized("Некорректный идентификатор клиента в cookie авторизации");

            var client = await _db.Clients
                .AsNoTracking()
                .Where(c => c.Id == clientId.Value)
                .Select(c => new ProfileClientDto
                {
                    FullName = c.FullName ?? string.Empty,
                    Category = c.ClientCategory != null
                        ? (c.ClientCategory.Name ?? "Без категории")
                        : "Без категории"
                })
                .FirstOrDefaultAsync();

            if (client == null)
                return Unauthorized("Клиент не найден");

            var ordersData = await _db.Orders
                .AsNoTracking()
                .Where(o => o.ClientId == clientId.Value)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    OrderId = o.Id,
                    CreatedAt = o.CreatedAt,
                    Status = o.Status != null ? o.Status.Name : null,
                    OrderType = o.OrderType != null ? (o.OrderType.Name ?? string.Empty) : string.Empty,
                    TotalPrice = o.TotalPrice ?? 0m,
                    TotalCalories = o.TotalCalories ?? 0m,
                    Comment = o.Comment
                })
                .ToListAsync();

            var orders = ordersData
                .Select(o => new ProfileOrderDto
                {
                    OrderId = o.OrderId,
                    CreatedAt = o.CreatedAt,
                    Status = o.Status ?? string.Empty,
                    OrderType = o.OrderType,
                    TotalPrice = o.TotalPrice,
                    TotalCalories = o.TotalCalories,
                    Comment = o.Comment,
                    CanCancel = !IsCancelledStatusName(o.Status),
                    Items = new List<ProfileOrderCompositionItemDto>()
                })
                .ToList();

            var orderCompositionByOrderId = await LoadOrderCompositionAsync(orders.Select(o => o.OrderId).ToList());
            foreach (var order in orders)
            {
                if (orderCompositionByOrderId.TryGetValue(order.OrderId, out var items))
                    order.Items = items;
            }

            return Ok(new ProfileResponse
            {
                Client = client,
                Orders = orders
            });
        }

        [HttpPost("orders/{orderId:int}/cancel")]
        public async Task<ActionResult<ProfileCancelOrderResponse>> CancelOrder(int orderId)
        {
            var clientId = TryGetClientId();
            if (clientId == null)
                return Unauthorized("Некорректный идентификатор клиента в cookie авторизации");

            var cancelledStatus = await _db.OrderStatuses
                .AsNoTracking()
                .Where(s => s.Name != null &&
                            (EF.Functions.Like(s.Name.ToLower(), $"%{CancelledStatusTokenRu}%") ||
                             EF.Functions.Like(s.Name.ToLower(), $"%{CancelledStatusTokenEn}%")))
                .OrderBy(s => s.Id)
                .Select(s => new { s.Id, s.Name })
                .FirstOrDefaultAsync();

            if (cancelledStatus == null)
                return StatusCode(500, "Статус отмены заказа не найден");

            var order = await _db.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.ClientId == clientId.Value);

            if (order == null)
                return NotFound("Заказ не найден");

            if (order.StatusId == cancelledStatus.Id)
                return BadRequest("Заказ уже отменен");

            order.StatusId = cancelledStatus.Id;
            await _db.SaveChangesAsync();

            return Ok(new ProfileCancelOrderResponse
            {
                OrderId = order.Id,
                Status = cancelledStatus.Name ?? CancelledStatusNameRu
            });
        }

        private int? TryGetClientId()
        {
            var rawClientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(rawClientId, out var clientId) ? clientId : null;
        }

        private static bool IsCancelledStatusName(string? statusName)
        {
            if (string.IsNullOrWhiteSpace(statusName))
                return false;

            var normalized = statusName.Trim().ToLowerInvariant();
            return normalized.Contains(CancelledStatusTokenRu) || normalized.Contains(CancelledStatusTokenEn);
        }

        private async Task<Dictionary<int, List<ProfileOrderCompositionItemDto>>> LoadOrderCompositionAsync(IReadOnlyCollection<int> orderIds)
        {
            var orderIdList = orderIds
                .Distinct()
                .ToList();

            if (orderIdList.Count == 0)
                return new Dictionary<int, List<ProfileOrderCompositionItemDto>>();

            var compositionByOrderId = orderIdList.ToDictionary(id => id, _ => new List<ProfileOrderCompositionItemDto>());

            var dishItems = await _db.OrderDishItems
                .AsNoTracking()
                .Where(i => i.OrderId.HasValue && orderIdList.Contains(i.OrderId.Value))
                .OrderBy(i => i.Id)
                .Select(i => new OrderCompositionItemSource
                {
                    ItemId = i.Id,
                    OrderId = i.OrderId!.Value,
                    Name = i.Dish != null ? i.Dish.Name : null,
                    Quantity = i.Quantity ?? 0m,
                    TotalPrice = i.FinalPrice ?? 0m
                })
                .ToListAsync();

            var dishAddons = new List<OrderCompositionAddonSource>();
            var dishItemIds = dishItems.Select(i => i.ItemId).ToList();
            if (dishItemIds.Count > 0)
            {
                dishAddons = await _db.DishToppings
                    .AsNoTracking()
                    .Where(t => t.OrderDishItemId.HasValue && dishItemIds.Contains(t.OrderDishItemId.Value))
                    .OrderBy(t => t.Id)
                    .Select(t => new OrderCompositionAddonSource
                    {
                        ParentItemId = t.OrderDishItemId!.Value,
                        Name = t.Topping != null ? t.Topping.Name : null,
                        Quantity = t.Quantity ?? 0m,
                        TotalPrice = t.FinalPrice ?? 0m
                    })
                    .ToListAsync();
            }

            var dishAddonsByItemId = dishAddons
                .GroupBy(a => a.ParentItemId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(a => new ProfileOrderCompositionAddonDto
                    {
                        Name = a.Name ?? "Без названия",
                        Quantity = a.Quantity,
                        TotalPrice = a.TotalPrice
                    }).ToList()
                );

            foreach (var dishItem in dishItems)
            {
                var addons = dishAddonsByItemId.TryGetValue(dishItem.ItemId, out var itemAddons)
                    ? itemAddons
                    : new List<ProfileOrderCompositionAddonDto>();

                compositionByOrderId[dishItem.OrderId].Add(new ProfileOrderCompositionItemDto
                {
                    Type = CompositionTypeDish,
                    Name = dishItem.Name ?? "Без названия",
                    Quantity = dishItem.Quantity,
                    TotalPrice = dishItem.TotalPrice,
                    Addons = addons
                });
            }

            var drinkItems = await _db.OrderDrinkItems
                .AsNoTracking()
                .Where(i => i.OrderId.HasValue && orderIdList.Contains(i.OrderId.Value))
                .OrderBy(i => i.Id)
                .Select(i => new OrderCompositionItemSource
                {
                    ItemId = i.Id,
                    OrderId = i.OrderId!.Value,
                    Name = i.Drink != null ? i.Drink.Name : null,
                    Quantity = i.Quantity ?? 0m,
                    TotalPrice = i.FinalPrice ?? 0m
                })
                .ToListAsync();

            var drinkAddons = new List<OrderCompositionAddonSource>();
            var drinkItemIds = drinkItems.Select(i => i.ItemId).ToList();
            if (drinkItemIds.Count > 0)
            {
                drinkAddons = await _db.DrinkToppings
                    .AsNoTracking()
                    .Where(t => t.OrderDrinkItemId.HasValue && drinkItemIds.Contains(t.OrderDrinkItemId.Value))
                    .OrderBy(t => t.Id)
                    .Select(t => new OrderCompositionAddonSource
                    {
                        ParentItemId = t.OrderDrinkItemId!.Value,
                        Name = t.Topping != null ? t.Topping.Name : null,
                        Quantity = t.Quantity ?? 0m,
                        TotalPrice = t.FinalPrice ?? 0m
                    })
                    .ToListAsync();
            }

            var drinkAddonsByItemId = drinkAddons
                .GroupBy(a => a.ParentItemId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(a => new ProfileOrderCompositionAddonDto
                    {
                        Name = a.Name ?? "Без названия",
                        Quantity = a.Quantity,
                        TotalPrice = a.TotalPrice
                    }).ToList()
                );

            foreach (var drinkItem in drinkItems)
            {
                var addons = drinkAddonsByItemId.TryGetValue(drinkItem.ItemId, out var itemAddons)
                    ? itemAddons
                    : new List<ProfileOrderCompositionAddonDto>();

                compositionByOrderId[drinkItem.OrderId].Add(new ProfileOrderCompositionItemDto
                {
                    Type = CompositionTypeDrink,
                    Name = drinkItem.Name ?? "Без названия",
                    Quantity = drinkItem.Quantity,
                    TotalPrice = drinkItem.TotalPrice,
                    Addons = addons
                });
            }

            var standaloneToppings = await _db.OrderToppingItems
                .AsNoTracking()
                .Where(i => orderIdList.Contains(i.OrderId))
                .OrderBy(i => i.Id)
                .Select(i => new
                {
                    i.OrderId,
                    Name = i.Topping != null ? i.Topping.Name : null,
                    Quantity = (decimal)i.Quantity,
                    i.TotalPrice
                })
                .ToListAsync();

            foreach (var topping in standaloneToppings)
            {
                compositionByOrderId[topping.OrderId].Add(new ProfileOrderCompositionItemDto
                {
                    Type = CompositionTypeTopping,
                    Name = topping.Name ?? "Без названия",
                    Quantity = topping.Quantity,
                    TotalPrice = topping.TotalPrice,
                    Addons = new List<ProfileOrderCompositionAddonDto>()
                });
            }

            return compositionByOrderId;
        }

        private sealed class OrderCompositionItemSource
        {
            public int ItemId { get; set; }
            public int OrderId { get; set; }
            public string? Name { get; set; }
            public decimal Quantity { get; set; }
            public decimal TotalPrice { get; set; }
        }

        private sealed class OrderCompositionAddonSource
        {
            public int ParentItemId { get; set; }
            public string? Name { get; set; }
            public decimal Quantity { get; set; }
            public decimal TotalPrice { get; set; }
        }
    }
}

