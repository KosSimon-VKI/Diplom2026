using GardenNookApi.Entities;
using GardenNookApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TransferModels.Kitchen;

namespace GardenNookApi.Controllers
{
    [ApiController]
    [Route("api/kitchen")]
    [Authorize]
    public class KitchenController : ControllerBase
    {
        private const string ActiveStatusTokenRu = "процесс";
        private const string ActiveStatusTokenEn = "process";
        private const string ToppingCategoryDishTokenRu = "к блюд";

        private readonly AppDbContext _db;
        private readonly KitchenPickupFilterOptions _pickupFilterOptions;

        public KitchenController(AppDbContext db, IOptions<KitchenPickupFilterOptions> pickupFilterOptions)
        {
            _db = db;
            _pickupFilterOptions = pickupFilterOptions?.Value ?? new KitchenPickupFilterOptions();
        }

        [HttpGet("orders")]
        public async Task<ActionResult<KitchenOrdersResponse>> GetOrders()
        {
            var orderSources = await _db.Orders
                .AsNoTracking()
                .Where(o =>
                    o.Status != null &&
                    o.Status.Name != null &&
                    (EF.Functions.Like(o.Status.Name.ToLower(), $"%{ActiveStatusTokenRu}%") ||
                     EF.Functions.Like(o.Status.Name.ToLower(), $"%{ActiveStatusTokenEn}%")))
                .OrderBy(o => o.CreatedAt)
                .ThenBy(o => o.Id)
                .Select(o => new
                {
                    o.Id,
                    o.Comment,
                    o.CreatedAt,
                    o.PickupAt,
                    OrderType = o.OrderType != null ? o.OrderType.Name : null
                })
                .ToListAsync();

            if (orderSources.Count == 0)
            {
                return Ok(new KitchenOrdersResponse());
            }

            var now = DateTime.Now;
            var pickupWindow = TimeSpan.FromMinutes(Math.Max(0, _pickupFilterOptions.WindowMinutes));

            var filteredOrderSources = orderSources
                .Where(o =>
                    !o.PickupAt.HasValue ||
                    IsWithinPickupWindow(o.PickupAt.Value, now, pickupWindow))
                .ToList();

            if (filteredOrderSources.Count == 0)
            {
                return Ok(new KitchenOrdersResponse());
            }

            var overduePickupSources = filteredOrderSources
                .Where(o => o.PickupAt <= now)
                .OrderByDescending(o => o.PickupAt)
                .ThenBy(o => o.Id)
                .ToList();

            var futurePickupSources = filteredOrderSources
                .Where(o => o.PickupAt > now)
                .OrderBy(o => o.PickupAt)
                .ThenBy(o => o.Id)
                .ToList();

            var noPickupSources = filteredOrderSources
                .Where(o => !o.PickupAt.HasValue)
                .OrderBy(o => o.CreatedAt)
                .ThenBy(o => o.Id)
                .ToList();

            var orderedOrderSources = overduePickupSources
                .Concat(futurePickupSources)
                .Concat(noPickupSources)
                .ToList();

            var orderIds = orderedOrderSources
                .Select(o => o.Id)
                .ToList();

            var ordersById = orderedOrderSources.ToDictionary(
                o => o.Id,
                o => new KitchenOrderDto
                {
                    OrderId = o.Id,
                    Comment = o.Comment ?? string.Empty,
                    CreatedAt = o.CreatedAt,
                    PickupAt = o.PickupAt,
                    OrderType = o.OrderType ?? string.Empty
                });

            var dishSources = await _db.OrderDishItems
                .AsNoTracking()
                .Where(i => i.OrderId.HasValue && orderIds.Contains(i.OrderId.Value))
                .OrderBy(i => i.Id)
                .Select(i => new DishSource
                {
                    ItemId = i.Id,
                    OrderId = i.OrderId!.Value,
                    Name = i.Dish != null ? i.Dish.Name : null,
                    Quantity = i.Quantity ?? 0m
                })
                .ToListAsync();

            var dishItemIds = dishSources
                .Select(i => i.ItemId)
                .ToList();

            var dishToppingSources = dishItemIds.Count == 0
                ? new List<DishToppingSource>()
                : await _db.DishToppings
                    .AsNoTracking()
                    .Where(t => t.OrderDishItemId.HasValue && dishItemIds.Contains(t.OrderDishItemId.Value))
                    .OrderBy(t => t.Id)
                    .Select(t => new DishToppingSource
                    {
                        OrderDishItemId = t.OrderDishItemId!.Value,
                        Name = t.Topping != null ? t.Topping.Name : null,
                        Quantity = t.Quantity ?? 0m
                    })
                    .ToListAsync();

            var dishToppingsByItemId = dishToppingSources
                .GroupBy(t => t.OrderDishItemId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(t => new KitchenOrderDishToppingDto
                    {
                        Name = t.Name ?? "Без названия",
                        Quantity = t.Quantity
                    }).ToList());

            foreach (var dishSource in dishSources)
            {
                if (!ordersById.TryGetValue(dishSource.OrderId, out var order))
                    continue;

                var dish = new KitchenOrderDishDto
                {
                    Name = dishSource.Name ?? "Без названия",
                    Quantity = dishSource.Quantity
                };

                if (dishToppingsByItemId.TryGetValue(dishSource.ItemId, out var dishToppings))
                {
                    dish.Toppings = dishToppings;
                }

                order.Dishes.Add(dish);
            }

            var standaloneToppings = await _db.OrderToppingItems
                .AsNoTracking()
                .Where(i =>
                    orderIds.Contains(i.OrderId) &&
                    i.Topping != null &&
                    i.Topping.Category != null &&
                    i.Topping.Category.Name != null &&
                    EF.Functions.Like(i.Topping.Category.Name.ToLower(), $"%{ToppingCategoryDishTokenRu}%"))
                .OrderBy(i => i.Id)
                .Select(i => new
                {
                    i.OrderId,
                    Name = i.Topping != null ? i.Topping.Name : null,
                    Quantity = (decimal)i.Quantity
                })
                .ToListAsync();

            foreach (var topping in standaloneToppings)
            {
                if (!ordersById.TryGetValue(topping.OrderId, out var order))
                    continue;

                order.Toppings.Add(new KitchenOrderStandaloneToppingDto
                {
                    Name = topping.Name ?? "Без названия",
                    Quantity = topping.Quantity
                });
            }

            var resultOrders = orderedOrderSources
                .Select(o => ordersById[o.Id])
                .Where(o => o.Dishes.Count > 0 || o.Toppings.Count > 0)
                .ToList();

            return Ok(new KitchenOrdersResponse
            {
                Orders = resultOrders
            });
        }

        private sealed class DishSource
        {
            public int ItemId { get; set; }
            public int OrderId { get; set; }
            public string? Name { get; set; }
            public decimal Quantity { get; set; }
        }

        private sealed class DishToppingSource
        {
            public int OrderDishItemId { get; set; }
            public string? Name { get; set; }
            public decimal Quantity { get; set; }
        }

        private static bool IsWithinPickupWindow(DateTime pickupAt, DateTime now, TimeSpan pickupWindow)
        {
            var delta = pickupAt - now;
            if (delta < TimeSpan.Zero)
            {
                delta = delta.Negate();
            }

            return delta <= pickupWindow;
        }
    }
}
