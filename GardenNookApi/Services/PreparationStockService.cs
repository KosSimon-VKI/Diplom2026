using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GardenNookApi.Entities;
using Microsoft.EntityFrameworkCore;
using TransferModels.Orders;

namespace GardenNookApi.Services
{
    public interface IPreparationStockService
    {
        Task RefreshMenuAvailabilityAsync(CancellationToken cancellationToken = default);
        Task<StockConsumptionResult> TryConsumeForOrderAsync(OrderRequest request, CancellationToken cancellationToken = default);
        Task<StockConsumptionResult> TryConsumeForOrderAsync(int orderId, CancellationToken cancellationToken = default);
    }

    public sealed class PreparationStockService : IPreparationStockService
    {
        private const int UnitGramsId = 2;
        private const int UnitMillilitersId = 3;
        private const int UnitPiecesId = 4;
        private const int UnitKilogramsId = 5;
        private const int UnitLitersId = 6;
        private const int MilkCategoryId = 10;
        private const int CoffeeCategoryId = 2;
        private const decimal DecimalEpsilon = 0.000001m;

        private readonly AppDbContext _db;

        public PreparationStockService(AppDbContext db)
        {
            _db = db;
        }

        public async Task RefreshMenuAvailabilityAsync(CancellationToken cancellationToken = default)
        {
            var dishes = await _db.Dishes.ToListAsync(cancellationToken);
            var drinks = await _db.Drinks.ToListAsync(cancellationToken);
            var toppings = await _db.ToppingsAndSyrups.ToListAsync(cancellationToken);

            var technicalCardIds = dishes
                .Where(x => x.TechnicalCardId.HasValue)
                .Select(x => x.TechnicalCardId!.Value)
                .Concat(drinks.Where(x => x.TechnicalCardId.HasValue).Select(x => x.TechnicalCardId!.Value))
                .Concat(toppings.Where(x => x.TechnicalCardId.HasValue).Select(x => x.TechnicalCardId!.Value))
                .Distinct()
                .ToList();

            var availabilityByCard = await BuildAvailabilityByTechnicalCardAsync(technicalCardIds, cancellationToken);

            var hasChanges = false;

            foreach (var dish in dishes)
            {
                var available = ResolveAvailability(dish.TechnicalCardId, availabilityByCard);
                if (dish.IsAvailable != available)
                {
                    dish.IsAvailable = available;
                    hasChanges = true;
                }
            }

            foreach (var drink in drinks)
            {
                var available = ResolveAvailability(drink.TechnicalCardId, availabilityByCard);
                if (drink.IsAvailable != available)
                {
                    drink.IsAvailable = available;
                    hasChanges = true;
                }
            }

            foreach (var topping in toppings)
            {
                var available = ResolveAvailability(topping.TechnicalCardId, availabilityByCard);
                if (topping.IsAvailable != available)
                {
                    topping.IsAvailable = available;
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<StockConsumptionResult> TryConsumeForOrderAsync(OrderRequest request, CancellationToken cancellationToken = default)
        {
            var requiredBySemiFinished = await BuildRequiredSemiFinishedForOrderAsync(request, cancellationToken);

            return await TryConsumeRequirementsAsync(
                requiredBySemiFinished,
                new Dictionary<int, decimal>(),
                cancellationToken);
        }

        public async Task<StockConsumptionResult> TryConsumeForOrderAsync(int orderId, CancellationToken cancellationToken = default)
        {
            var requirements = await BuildRequiredForSavedOrderAsync(orderId, cancellationToken);

            return await TryConsumeRequirementsAsync(
                requirements.RequiredBySemiFinished,
                requirements.RequiredByIngredients,
                cancellationToken);
        }

        private async Task<StockConsumptionResult> TryConsumeRequirementsAsync(
            IReadOnlyDictionary<int, decimal> requiredBySemiFinished,
            IReadOnlyDictionary<int, decimal> requiredByIngredients,
            CancellationToken cancellationToken)
        {
            if (requiredBySemiFinished.Count == 0 && requiredByIngredients.Count == 0)
            {
                return StockConsumptionResult.Success();
            }

            var semiFinishedIds = requiredBySemiFinished.Keys.ToList();
            var ingredientIds = requiredByIngredients.Keys.ToList();

            var preparations = semiFinishedIds.Count == 0
                ? []
                : await _db.Preparations
                    .Where(x => x.SemiFinishedId.HasValue && semiFinishedIds.Contains(x.SemiFinishedId.Value))
                    .OrderBy(x => x.ProductionDate ?? DateOnly.MinValue)
                    .ThenBy(x => x.Id)
                    .ToListAsync(cancellationToken);

            var preparationsBySemiFinished = preparations
                .Where(x => x.SemiFinishedId.HasValue)
                .GroupBy(x => x.SemiFinishedId!.Value)
                .ToDictionary(x => x.Key, x => x.ToList());

            var semiNames = semiFinishedIds.Count == 0
                ? new Dictionary<int, string>()
                : await _db.SemiFinisheds
                    .Where(x => semiFinishedIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.Name ?? $"SemiFinished #{x.Id}", cancellationToken);

            var ingredientsById = ingredientIds.Count == 0
                ? new Dictionary<int, Ingredient>()
                : await _db.Ingredients
                    .Where(x => ingredientIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x, cancellationToken);

            var shortages = new List<StockConsumptionItem>();

            foreach (var requiredPair in requiredBySemiFinished)
            {
                var semiFinishedId = requiredPair.Key;
                var required = requiredPair.Value;

                preparationsBySemiFinished.TryGetValue(semiFinishedId, out var rows);

                var available = (rows ?? [])
                    .Sum(x => ToNonNegative(x.StockGrams));

                if (available + DecimalEpsilon < required)
                {
                    shortages.Add(new StockConsumptionItem
                    {
                        SemiFinishedId = semiFinishedId,
                        SemiFinishedName = semiNames.TryGetValue(semiFinishedId, out var name) ? name : $"SemiFinished #{semiFinishedId}",
                        Required = RoundTo6(required),
                        Available = RoundTo6(available)
                    });
                }
            }

            foreach (var requiredPair in requiredByIngredients)
            {
                var ingredientId = requiredPair.Key;
                var required = requiredPair.Value;

                if (!ingredientsById.TryGetValue(ingredientId, out var ingredient))
                {
                    shortages.Add(new StockConsumptionItem
                    {
                        SemiFinishedId = ingredientId,
                        SemiFinishedName = $"Ingredient #{ingredientId}",
                        Required = RoundTo6(required),
                        Available = 0m
                    });
                    continue;
                }

                var available = ConvertToBaseUnits(ToNonNegative(ingredient.Stock), ingredient.UnitOfMeasureId);
                if (available + DecimalEpsilon < required)
                {
                    shortages.Add(new StockConsumptionItem
                    {
                        SemiFinishedId = ingredientId,
                        SemiFinishedName = ingredient.Name ?? $"Ingredient #{ingredientId}",
                        Required = RoundTo6(required),
                        Available = RoundTo6(available)
                    });
                }
            }

            if (shortages.Count > 0)
            {
                return StockConsumptionResult.Fail(shortages);
            }

            foreach (var requiredPair in requiredBySemiFinished.OrderBy(x => x.Key))
            {
                var semiFinishedId = requiredPair.Key;
                var remaining = requiredPair.Value;

                if (!preparationsBySemiFinished.TryGetValue(semiFinishedId, out var rows))
                {
                    var name = semiNames.TryGetValue(semiFinishedId, out var value)
                        ? value
                        : $"SemiFinished #{semiFinishedId}";

                    return StockConsumptionResult.Fail([
                        new StockConsumptionItem
                        {
                            SemiFinishedId = semiFinishedId,
                            SemiFinishedName = name,
                            Required = RoundTo6(remaining),
                            Available = 0m
                        }
                    ]);
                }

                foreach (var preparation in rows)
                {
                    var stock = ToNonNegative(preparation.StockGrams);
                    if (stock <= DecimalEpsilon)
                    {
                        continue;
                    }

                    var toTake = Math.Min(stock, remaining);
                    preparation.StockGrams = RoundTo2(stock - toTake);
                    remaining -= toTake;

                    if (remaining <= DecimalEpsilon)
                    {
                        break;
                    }
                }

                if (remaining > DecimalEpsilon)
                {
                    var available = rows.Sum(x => ToNonNegative(x.StockGrams));
                    var name = semiNames.TryGetValue(semiFinishedId, out var value)
                        ? value
                        : $"SemiFinished #{semiFinishedId}";

                    return StockConsumptionResult.Fail([
                        new StockConsumptionItem
                        {
                            SemiFinishedId = semiFinishedId,
                            SemiFinishedName = name,
                            Required = RoundTo6(requiredPair.Value),
                            Available = RoundTo6(available)
                        }
                    ]);
                }
            }

            foreach (var requiredPair in requiredByIngredients.OrderBy(x => x.Key))
            {
                var ingredientId = requiredPair.Key;
                var required = requiredPair.Value;

                if (!ingredientsById.TryGetValue(ingredientId, out var ingredient))
                {
                    return StockConsumptionResult.Fail([
                        new StockConsumptionItem
                        {
                            SemiFinishedId = ingredientId,
                            SemiFinishedName = $"Ingredient #{ingredientId}",
                            Required = RoundTo6(required),
                            Available = 0m
                        }
                    ]);
                }

                var available = ConvertToBaseUnits(ToNonNegative(ingredient.Stock), ingredient.UnitOfMeasureId);
                if (available + DecimalEpsilon < required)
                {
                    return StockConsumptionResult.Fail([
                        new StockConsumptionItem
                        {
                            SemiFinishedId = ingredientId,
                            SemiFinishedName = ingredient.Name ?? $"Ingredient #{ingredientId}",
                            Required = RoundTo6(required),
                            Available = RoundTo6(available)
                        }
                    ]);
                }

                var remaining = Math.Max(0m, available - required);
                ingredient.Stock = RoundTo2(ConvertFromBaseUnits(remaining, ingredient.UnitOfMeasureId));
            }

            return StockConsumptionResult.Success();
        }
        private async Task<Dictionary<int, decimal>> BuildRequiredSemiFinishedForOrderAsync(
            OrderRequest request,
            CancellationToken cancellationToken)
        {
            var result = new Dictionary<int, decimal>();

            var dishes = request.Dishes ?? [];
            var drinks = request.Drinks ?? [];
            var toppings = request.Toppings ?? [];

            var dishIds = dishes.Select(x => x.DishId).Distinct().ToList();
            var drinkIds = drinks.Select(x => x.DrinkId).Distinct().ToList();

            var topIdsFromDishes = dishes
                .SelectMany(x => x.Toppings ?? [])
                .Select(x => x.ToppingId);

            var topIdsFromDrinks = drinks
                .SelectMany(x => x.Toppings ?? [])
                .Select(x => x.ToppingId);

            var topIdsStandalone = toppings.Select(x => x.ToppingId);

            var toppingIds = topIdsFromDishes
                .Concat(topIdsFromDrinks)
                .Concat(topIdsStandalone)
                .Distinct()
                .ToList();

            var dishCardById = dishIds.Count == 0
                ? new Dictionary<int, int?>()
                : await _db.Dishes
                    .Where(x => dishIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.TechnicalCardId })
                    .ToDictionaryAsync(x => x.Id, x => x.TechnicalCardId, cancellationToken);

            var drinkCardById = drinkIds.Count == 0
                ? new Dictionary<int, int?>()
                : await _db.Drinks
                    .Where(x => drinkIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.TechnicalCardId })
                    .ToDictionaryAsync(x => x.Id, x => x.TechnicalCardId, cancellationToken);

            var toppingCardById = toppingIds.Count == 0
                ? new Dictionary<int, int?>()
                : await _db.ToppingsAndSyrups
                    .Where(x => toppingIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.TechnicalCardId })
                    .ToDictionaryAsync(x => x.Id, x => x.TechnicalCardId, cancellationToken);

            var technicalCardIds = dishCardById.Values
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Concat(drinkCardById.Values.Where(x => x.HasValue).Select(x => x!.Value))
                .Concat(toppingCardById.Values.Where(x => x.HasValue).Select(x => x!.Value))
                .Distinct()
                .ToList();

            var requirementsByCard = await LoadRequirementsByTechnicalCardAsync(technicalCardIds, cancellationToken);

            foreach (var dish in dishes)
            {
                if (dish.Quantity <= 0)
                {
                    continue;
                }

                if (dishCardById.TryGetValue(dish.DishId, out var cardId) && cardId.HasValue)
                {
                    AddCardRequirements(result, requirementsByCard, cardId.Value, dish.Quantity);
                }

                foreach (var top in dish.Toppings ?? [])
                {
                    if (top.Quantity <= 0)
                    {
                        continue;
                    }

                    if (toppingCardById.TryGetValue(top.ToppingId, out var topCardId) && topCardId.HasValue)
                    {
                        var totalQty = dish.Quantity * top.Quantity;
                        AddCardRequirements(result, requirementsByCard, topCardId.Value, totalQty);
                    }
                }
            }

            foreach (var drink in drinks)
            {
                if (drink.Quantity <= 0)
                {
                    continue;
                }

                if (drinkCardById.TryGetValue(drink.DrinkId, out var cardId) && cardId.HasValue)
                {
                    AddCardRequirements(result, requirementsByCard, cardId.Value, drink.Quantity);
                }

                foreach (var top in drink.Toppings ?? [])
                {
                    if (top.Quantity <= 0)
                    {
                        continue;
                    }

                    if (toppingCardById.TryGetValue(top.ToppingId, out var topCardId) && topCardId.HasValue)
                    {
                        var totalQty = drink.Quantity * top.Quantity;
                        AddCardRequirements(result, requirementsByCard, topCardId.Value, totalQty);
                    }
                }
            }

            foreach (var top in toppings)
            {
                if (top.Quantity <= 0)
                {
                    continue;
                }

                if (toppingCardById.TryGetValue(top.ToppingId, out var topCardId) && topCardId.HasValue)
                {
                    AddCardRequirements(result, requirementsByCard, topCardId.Value, top.Quantity);
                }
            }

            return result;
        }

        private async Task<SavedOrderRequirements> BuildRequiredForSavedOrderAsync(
            int orderId,
            CancellationToken cancellationToken)
        {
            var orderExists = await _db.Orders
                .AsNoTracking()
                .AnyAsync(x => x.Id == orderId, cancellationToken);

            if (!orderExists)
            {
                throw new InvalidOperationException($"Order Id={orderId} was not found.");
            }

            var requiredBySemiFinished = new Dictionary<int, decimal>();
            var requiredByIngredients = new Dictionary<int, decimal>();

            var dishItems = await _db.OrderDishItems
                .AsNoTracking()
                .Where(x =>
                    x.OrderId == orderId &&
                    x.DishId.HasValue &&
                    x.Quantity.HasValue &&
                    x.Quantity.Value > 0m)
                .Select(x => new SavedDishItem(
                    x.Id,
                    x.DishId!.Value,
                    x.Quantity!.Value))
                .ToListAsync(cancellationToken);

            var drinkItems = await _db.OrderDrinkItems
                .AsNoTracking()
                .Where(x =>
                    x.OrderId == orderId &&
                    x.DrinkId.HasValue &&
                    x.Quantity.HasValue &&
                    x.Quantity.Value > 0m)
                .Select(x => new SavedDrinkItem(
                    x.Id,
                    x.DrinkId!.Value,
                    x.Quantity!.Value))
                .ToListAsync(cancellationToken);

            var standaloneToppings = await _db.OrderToppingItems
                .AsNoTracking()
                .Where(x => x.OrderId == orderId && x.Quantity > 0)
                .Select(x => new SavedStandaloneTopping(
                    x.ToppingId,
                    x.Quantity))
                .ToListAsync(cancellationToken);

            if (dishItems.Count == 0 && drinkItems.Count == 0 && standaloneToppings.Count == 0)
            {
                return new SavedOrderRequirements(requiredBySemiFinished, requiredByIngredients);
            }

            var dishItemIds = dishItems.Select(x => x.Id).ToList();
            var drinkItemIds = drinkItems.Select(x => x.Id).ToList();

            var dishToppings = dishItemIds.Count == 0
                ? []
                : await _db.DishToppings
                    .AsNoTracking()
                    .Where(x =>
                        x.OrderDishItemId.HasValue &&
                        dishItemIds.Contains(x.OrderDishItemId.Value) &&
                        x.ToppingId.HasValue &&
                        x.Quantity.HasValue &&
                        x.Quantity.Value > 0m)
                    .Select(x => new SavedLinkedTopping(
                        x.OrderDishItemId!.Value,
                        x.ToppingId!.Value,
                        x.Quantity!.Value))
                    .ToListAsync(cancellationToken);

            var drinkToppings = drinkItemIds.Count == 0
                ? []
                : await _db.DrinkToppings
                    .AsNoTracking()
                    .Where(x =>
                        x.OrderDrinkItemId.HasValue &&
                        drinkItemIds.Contains(x.OrderDrinkItemId.Value) &&
                        x.ToppingId.HasValue &&
                        x.Quantity.HasValue &&
                        x.Quantity.Value > 0m)
                    .Select(x => new SavedLinkedTopping(
                        x.OrderDrinkItemId!.Value,
                        x.ToppingId!.Value,
                        x.Quantity!.Value))
                    .ToListAsync(cancellationToken);

            var modifiersByDrinkItem = drinkItemIds.Count == 0
                ? new Dictionary<int, SavedDrinkModifier>()
                : await _db.OrderDrinkItemModifiers
                    .AsNoTracking()
                    .Where(x => drinkItemIds.Contains(x.OrderDrinkItemId))
                    .ToDictionaryAsync(
                        x => x.OrderDrinkItemId,
                        x => new SavedDrinkModifier(x.MilkIngredientId, x.CoffeeIngredientId),
                        cancellationToken);

            var dishIds = dishItems.Select(x => x.DishId).Distinct().ToList();
            var drinkIds = drinkItems.Select(x => x.DrinkId).Distinct().ToList();

            var toppingIds = dishToppings
                .Select(x => x.ToppingId)
                .Concat(drinkToppings.Select(x => x.ToppingId))
                .Concat(standaloneToppings.Select(x => x.ToppingId))
                .Distinct()
                .ToList();

            var dishCardById = dishIds.Count == 0
                ? new Dictionary<int, int?>()
                : await _db.Dishes
                    .AsNoTracking()
                    .Where(x => dishIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.TechnicalCardId })
                    .ToDictionaryAsync(x => x.Id, x => x.TechnicalCardId, cancellationToken);

            var drinkCardById = drinkIds.Count == 0
                ? new Dictionary<int, int?>()
                : await _db.Drinks
                    .AsNoTracking()
                    .Where(x => drinkIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.TechnicalCardId })
                    .ToDictionaryAsync(x => x.Id, x => x.TechnicalCardId, cancellationToken);

            var toppingCardById = toppingIds.Count == 0
                ? new Dictionary<int, int?>()
                : await _db.ToppingsAndSyrups
                    .AsNoTracking()
                    .Where(x => toppingIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.TechnicalCardId })
                    .ToDictionaryAsync(x => x.Id, x => x.TechnicalCardId, cancellationToken);

            var technicalCardIds = dishCardById.Values
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Concat(drinkCardById.Values.Where(x => x.HasValue).Select(x => x!.Value))
                .Concat(toppingCardById.Values.Where(x => x.HasValue).Select(x => x!.Value))
                .Distinct()
                .ToList();

            var requirementsByCard = await LoadRequirementsByTechnicalCardAsync(technicalCardIds, cancellationToken);

            var dishToppingsByItem = dishToppings
                .GroupBy(x => x.OwnerItemId)
                .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var dishItem in dishItems)
            {
                if (dishCardById.TryGetValue(dishItem.DishId, out var cardId) && cardId.HasValue)
                {
                    AddCardRequirements(requiredBySemiFinished, requirementsByCard, cardId.Value, dishItem.Quantity);
                }

                if (!dishToppingsByItem.TryGetValue(dishItem.Id, out var linkedToppings))
                {
                    continue;
                }

                foreach (var top in linkedToppings)
                {
                    if (toppingCardById.TryGetValue(top.ToppingId, out var topCardId) && topCardId.HasValue)
                    {
                        AddCardRequirements(requiredBySemiFinished, requirementsByCard, topCardId.Value, top.Quantity);
                    }
                }
            }

            var drinkToppingsByItem = drinkToppings
                .GroupBy(x => x.OwnerItemId)
                .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var drinkItem in drinkItems)
            {
                if (drinkCardById.TryGetValue(drinkItem.DrinkId, out var cardId) && cardId.HasValue)
                {
                    AddCardRequirements(requiredBySemiFinished, requirementsByCard, cardId.Value, drinkItem.Quantity);
                }

                if (!drinkToppingsByItem.TryGetValue(drinkItem.Id, out var linkedToppings))
                {
                    continue;
                }

                foreach (var top in linkedToppings)
                {
                    if (toppingCardById.TryGetValue(top.ToppingId, out var topCardId) && topCardId.HasValue)
                    {
                        AddCardRequirements(requiredBySemiFinished, requirementsByCard, topCardId.Value, top.Quantity);
                    }
                }
            }

            foreach (var standaloneTop in standaloneToppings)
            {
                if (toppingCardById.TryGetValue(standaloneTop.ToppingId, out var topCardId) && topCardId.HasValue)
                {
                    AddCardRequirements(requiredBySemiFinished, requirementsByCard, topCardId.Value, standaloneTop.Quantity);
                }
            }

            var drinkTechnicalCardIds = drinkItems
                .Select(x => drinkCardById.TryGetValue(x.DrinkId, out var cardId) ? cardId : null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

            if (drinkTechnicalCardIds.Count == 0)
            {
                return new SavedOrderRequirements(requiredBySemiFinished, requiredByIngredients);
            }

            var ingredientRowsByCard = await LoadIngredientRequirementsByTechnicalCardAsync(drinkTechnicalCardIds, cancellationToken);

            var baseIngredientIds = ingredientRowsByCard.Values
                .SelectMany(x => x)
                .Select(x => x.IngredientId);

            var modifierIngredientIds = modifiersByDrinkItem.Values
                .SelectMany(x => new int?[] { x.MilkIngredientId, x.CoffeeIngredientId })
                .Where(x => x.HasValue)
                .Select(x => x!.Value);

            var allIngredientIds = baseIngredientIds
                .Concat(modifierIngredientIds)
                .Distinct()
                .ToList();

            var ingredientMetadataById = allIngredientIds.Count == 0
                ? new Dictionary<int, IngredientMetadata>()
                : await _db.Ingredients
                    .AsNoTracking()
                    .Where(x => allIngredientIds.Contains(x.Id))
                    .Select(x => new IngredientMetadata(
                        x.Id,
                        x.Name ?? $"Ingredient #{x.Id}",
                        x.CategoryId))
                    .ToDictionaryAsync(x => x.Id, x => x, cancellationToken);

            foreach (var drinkItem in drinkItems)
            {
                if (!drinkCardById.TryGetValue(drinkItem.DrinkId, out var drinkCardId) || !drinkCardId.HasValue)
                {
                    continue;
                }

                if (!ingredientRowsByCard.TryGetValue(drinkCardId.Value, out var baseRows) || baseRows.Count == 0)
                {
                    continue;
                }

                modifiersByDrinkItem.TryGetValue(drinkItem.Id, out var modifier);
                var effectiveRows = ApplyDrinkIngredientModifiers(baseRows, modifier, ingredientMetadataById);

                foreach (var row in effectiveRows)
                {
                    AddIngredientRequirement(requiredByIngredients, row.IngredientId, row.RequiredBase * drinkItem.Quantity);
                }
            }

            return new SavedOrderRequirements(requiredBySemiFinished, requiredByIngredients);
        }
        private async Task<Dictionary<int, bool>> BuildAvailabilityByTechnicalCardAsync(
            IReadOnlyCollection<int> technicalCardIds,
            CancellationToken cancellationToken)
        {
            var requirementsByCard = await LoadRequirementsByTechnicalCardAsync(technicalCardIds, cancellationToken);
            var semiFinishedIds = requirementsByCard.Values
                .SelectMany(x => x.Keys)
                .Distinct()
                .ToList();

            var stockBySemiFinished = await _db.Preparations
                .Where(x => x.SemiFinishedId.HasValue && semiFinishedIds.Contains(x.SemiFinishedId.Value))
                .GroupBy(x => x.SemiFinishedId!.Value)
                .Select(g => new
                {
                    SemiFinishedId = g.Key,
                    Available = g.Sum(x => (decimal?)x.StockGrams) ?? 0m
                })
                .ToDictionaryAsync(x => x.SemiFinishedId, x => ToNonNegative(x.Available), cancellationToken);

            var result = new Dictionary<int, bool>();

            foreach (var technicalCardId in technicalCardIds)
            {
                if (!requirementsByCard.TryGetValue(technicalCardId, out var requirements) || requirements.Count == 0)
                {
                    result[technicalCardId] = true;
                    continue;
                }

                var available = true;

                foreach (var pair in requirements)
                {
                    stockBySemiFinished.TryGetValue(pair.Key, out var stock);
                    if (stock + DecimalEpsilon < pair.Value)
                    {
                        available = false;
                        break;
                    }
                }

                result[technicalCardId] = available;
            }

            return result;
        }

        private async Task<Dictionary<int, Dictionary<int, decimal>>> LoadRequirementsByTechnicalCardAsync(
            IReadOnlyCollection<int> technicalCardIds,
            CancellationToken cancellationToken)
        {
            if (technicalCardIds.Count == 0)
            {
                return new Dictionary<int, Dictionary<int, decimal>>();
            }

            var rows = await _db.TechnicalCardSemiFinishedCompositions
                .Where(x =>
                    x.TechnicalCardId.HasValue &&
                    x.SemiFinishedId.HasValue &&
                    technicalCardIds.Contains(x.TechnicalCardId.Value))
                .Select(x => new
                {
                    TechnicalCardId = x.TechnicalCardId!.Value,
                    SemiFinishedId = x.SemiFinishedId!.Value,
                    Required = ConvertToBaseUnits(GetRequiredWeight(x), x.UnitOfMeasureId)
                })
                .ToListAsync(cancellationToken);

            var grouped = rows
                .Where(x => x.Required > DecimalEpsilon)
                .GroupBy(x => new { x.TechnicalCardId, x.SemiFinishedId })
                .Select(g => new
                {
                    g.Key.TechnicalCardId,
                    g.Key.SemiFinishedId,
                    Required = g.Sum(x => x.Required)
                })
                .GroupBy(x => x.TechnicalCardId)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(x => x.SemiFinishedId, x => RoundTo6(x.Required)));

            return grouped;
        }

        private async Task<Dictionary<int, List<IngredientRequirementRow>>> LoadIngredientRequirementsByTechnicalCardAsync(
            IReadOnlyCollection<int> technicalCardIds,
            CancellationToken cancellationToken)
        {
            if (technicalCardIds.Count == 0)
            {
                return new Dictionary<int, List<IngredientRequirementRow>>();
            }

            var rows = await _db.TechnicalCardIngredientCompositions
                .AsNoTracking()
                .Where(x =>
                    x.TechnicalCardId.HasValue &&
                    x.IngredientId.HasValue &&
                    technicalCardIds.Contains(x.TechnicalCardId.Value))
                .Select(x => new
                {
                    TechnicalCardId = x.TechnicalCardId!.Value,
                    IngredientId = x.IngredientId!.Value,
                    Required = ConvertToBaseUnits(GetRequiredWeight(x), x.UnitOfMeasureId)
                })
                .ToListAsync(cancellationToken);

            return rows
                .Where(x => x.Required > DecimalEpsilon)
                .GroupBy(x => x.TechnicalCardId)
                .ToDictionary(
                    g => g.Key,
                    g => g
                        .Select(x => new IngredientRequirementRow(x.IngredientId, RoundTo6(x.Required)))
                        .ToList());
        }

        private static IReadOnlyList<IngredientRequirementRow> ApplyDrinkIngredientModifiers(
            IReadOnlyList<IngredientRequirementRow> baseRows,
            SavedDrinkModifier? modifier,
            IReadOnlyDictionary<int, IngredientMetadata> ingredientMetadataById)
        {
            if (baseRows.Count == 0 ||
                modifier == null ||
                (!modifier.MilkIngredientId.HasValue && !modifier.CoffeeIngredientId.HasValue))
            {
                return baseRows;
            }

            int? milkReplacementIngredientId = null;
            if (modifier.MilkIngredientId.HasValue)
            {
                milkReplacementIngredientId = modifier.MilkIngredientId.Value;

                if (!ingredientMetadataById.TryGetValue(milkReplacementIngredientId.Value, out var milkIngredient))
                {
                    throw new InvalidOperationException($"Milk replacement ingredient Id={milkReplacementIngredientId.Value} was not found.");
                }

                if (milkIngredient.CategoryId != MilkCategoryId)
                {
                    throw new InvalidOperationException($"Milk replacement ingredient Id={milkReplacementIngredientId.Value} has invalid category.");
                }
            }

            int? coffeeReplacementIngredientId = null;
            if (modifier.CoffeeIngredientId.HasValue)
            {
                coffeeReplacementIngredientId = modifier.CoffeeIngredientId.Value;

                if (!ingredientMetadataById.TryGetValue(coffeeReplacementIngredientId.Value, out var coffeeIngredient))
                {
                    throw new InvalidOperationException($"Coffee replacement ingredient Id={coffeeReplacementIngredientId.Value} was not found.");
                }

                if (coffeeIngredient.CategoryId != CoffeeCategoryId)
                {
                    throw new InvalidOperationException($"Coffee replacement ingredient Id={coffeeReplacementIngredientId.Value} has invalid category.");
                }
            }

            if (!milkReplacementIngredientId.HasValue && !coffeeReplacementIngredientId.HasValue)
            {
                return baseRows;
            }

            var result = new List<IngredientRequirementRow>(baseRows.Count);

            foreach (var baseRow in baseRows)
            {
                var ingredientId = baseRow.IngredientId;

                if (ingredientMetadataById.TryGetValue(baseRow.IngredientId, out var baseIngredientMetadata))
                {
                    if (milkReplacementIngredientId.HasValue && baseIngredientMetadata.CategoryId == MilkCategoryId)
                    {
                        ingredientId = milkReplacementIngredientId.Value;
                    }
                    else if (coffeeReplacementIngredientId.HasValue && baseIngredientMetadata.CategoryId == CoffeeCategoryId)
                    {
                        ingredientId = coffeeReplacementIngredientId.Value;
                    }
                }

                result.Add(new IngredientRequirementRow(ingredientId, baseRow.RequiredBase));
            }

            return result;
        }

        private static void AddCardRequirements(
            IDictionary<int, decimal> target,
            IReadOnlyDictionary<int, Dictionary<int, decimal>> requirementsByCard,
            int technicalCardId,
            decimal multiplier)
        {
            if (multiplier <= 0 || !requirementsByCard.TryGetValue(technicalCardId, out var requirements))
            {
                return;
            }

            foreach (var pair in requirements)
            {
                if (target.TryGetValue(pair.Key, out var current))
                {
                    target[pair.Key] = current + (pair.Value * multiplier);
                }
                else
                {
                    target[pair.Key] = pair.Value * multiplier;
                }
            }
        }

        private static void AddIngredientRequirement(
            IDictionary<int, decimal> target,
            int ingredientId,
            decimal requiredBase)
        {
            if (ingredientId <= 0 || requiredBase <= DecimalEpsilon)
            {
                return;
            }

            if (target.TryGetValue(ingredientId, out var current))
            {
                target[ingredientId] = current + requiredBase;
            }
            else
            {
                target[ingredientId] = requiredBase;
            }
        }

        private static decimal GetRequiredWeight(TechnicalCardSemiFinishedComposition row)
        {
            if (row.OutputWeight.HasValue && row.OutputWeight.Value > 0)
            {
                return row.OutputWeight.Value;
            }

            if (row.NetWeight.HasValue && row.NetWeight.Value > 0)
            {
                return row.NetWeight.Value;
            }

            if (row.GrossWeight.HasValue && row.GrossWeight.Value > 0)
            {
                return row.GrossWeight.Value;
            }

            return 0m;
        }

        private static decimal GetRequiredWeight(TechnicalCardIngredientComposition row)
        {
            if (row.OutputWeight.HasValue && row.OutputWeight.Value > 0)
            {
                return row.OutputWeight.Value;
            }

            if (row.NetWeight.HasValue && row.NetWeight.Value > 0)
            {
                return row.NetWeight.Value;
            }

            if (row.GrossWeight.HasValue && row.GrossWeight.Value > 0)
            {
                return row.GrossWeight.Value;
            }

            return 0m;
        }

        private static decimal ConvertToBaseUnits(decimal value, int? unitOfMeasureId)
        {
            if (value <= 0)
            {
                return 0m;
            }

            return unitOfMeasureId switch
            {
                UnitKilogramsId => value * 1000m,
                UnitLitersId => value * 1000m,
                UnitGramsId => value,
                UnitMillilitersId => value,
                UnitPiecesId => value,
                _ => value
            };
        }

        private static decimal ConvertFromBaseUnits(decimal value, int? unitOfMeasureId)
        {
            if (value <= 0)
            {
                return 0m;
            }

            return unitOfMeasureId switch
            {
                UnitKilogramsId => value / 1000m,
                UnitLitersId => value / 1000m,
                UnitGramsId => value,
                UnitMillilitersId => value,
                UnitPiecesId => value,
                _ => value
            };
        }

        private static bool ResolveAvailability(
            int? technicalCardId,
            IReadOnlyDictionary<int, bool> availabilityByCard)
        {
            if (!technicalCardId.HasValue)
            {
                return true;
            }

            return availabilityByCard.TryGetValue(technicalCardId.Value, out var available)
                ? available
                : true;
        }

        private static decimal ToNonNegative(decimal? value)
            => value.HasValue && value.Value > 0 ? value.Value : 0m;

        private static decimal RoundTo2(decimal value)
            => Math.Round(value, 2, MidpointRounding.AwayFromZero);

        private static decimal RoundTo6(decimal value)
            => Math.Round(value, 6, MidpointRounding.AwayFromZero);

        private sealed record SavedOrderRequirements(
            Dictionary<int, decimal> RequiredBySemiFinished,
            Dictionary<int, decimal> RequiredByIngredients);

        private sealed record SavedDishItem(int Id, int DishId, decimal Quantity);

        private sealed record SavedDrinkItem(int Id, int DrinkId, decimal Quantity);

        private sealed record SavedLinkedTopping(int OwnerItemId, int ToppingId, decimal Quantity);

        private sealed record SavedStandaloneTopping(int ToppingId, decimal Quantity);

        private sealed record SavedDrinkModifier(int? MilkIngredientId, int? CoffeeIngredientId);

        private sealed record IngredientMetadata(int Id, string Name, int? CategoryId);

        private sealed record IngredientRequirementRow(int IngredientId, decimal RequiredBase);
    }

    public sealed class StockConsumptionResult
    {
        public bool IsSuccess { get; init; }
        public IReadOnlyList<StockConsumptionItem> Items { get; init; } = [];

        public static StockConsumptionResult Success() =>
            new() { IsSuccess = true };

        public static StockConsumptionResult Fail(IReadOnlyList<StockConsumptionItem> items) =>
            new()
            {
                IsSuccess = false,
                Items = items
            };
    }

    public sealed class StockConsumptionItem
    {
        public int SemiFinishedId { get; init; }
        public string SemiFinishedName { get; init; } = string.Empty;
        public decimal Required { get; init; }
        public decimal Available { get; init; }
    }
}
