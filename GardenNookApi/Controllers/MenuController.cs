using GardenNookApi.Entities;
using GardenNookApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TransferModels.Menu;

namespace GardenNookApi.Controllers
{
    [ApiController]
    [Route("api/menu")]
    [Authorize]
    public class MenuController : Controller
    {
        private const int UnitGramsId = 2;
        private const int UnitMillilitersId = 3;
        private const int UnitPiecesId = 4;
        private const int UnitKilogramsId = 5;
        private const int UnitLitersId = 6;
        private const string InactiveCategoryName = "Неактивные";
        private const int InactiveDishCategoryId = 12;
        private static readonly string InactiveCategoryNameLower = InactiveCategoryName.ToLower();
        private static readonly int[] MilkModifierIngredientIds = [106, 107, 108, 110, 113, 115, 118];
        private static readonly int[] CoffeeModifierIngredientIds = [6, 8];

        private readonly AppDbContext database;
        private readonly IPreparationStockService stockService;

        public MenuController(AppDbContext db, IPreparationStockService stockService)
        {
            database = db;
            this.stockService = stockService;
        }

        [HttpGet]
        public async Task<IActionResult> GetFullMenu()
        {
            await stockService.RefreshMenuAvailabilityAsync();

            // Блюда
            var dishRows = await database.Dishes
                .AsNoTracking()
                .Include(q => q.Category)
                .Where(d =>
                    d.CategoryId != InactiveDishCategoryId &&
                    (d.Category == null ||
                     d.Category.Name == null ||
                     d.Category.Name.Trim().ToLower() != InactiveCategoryNameLower))
                .OrderByDescending(d => d.IsAvailable)
                .ThenBy(d => d.Name)
                .Select(d => new
                {
                    d.Id,
                    d.Name,
                    d.PriceRub,
                    CategoryName = d.Category != null ? d.Category.Name : null,
                    d.CaloriesKcal,
                    d.ProteinsG,
                    d.FatsG,
                    d.CarbsG,
                    d.ImageUrl,
                    d.IsAvailable,
                    d.TechnicalCardId
                })
                .ToListAsync();

            var dishCardIds = dishRows
                .Where(d => d.TechnicalCardId.HasValue)
                .Select(d => d.TechnicalCardId!.Value)
                .Distinct()
                .ToList();

            var dishWeightByCard = new Dictionary<int, decimal>();
            if (dishCardIds.Count > 0)
            {
                var ingredientRows = await database.TechnicalCardIngredientCompositions
                    .AsNoTracking()
                    .Where(x => x.TechnicalCardId.HasValue && dishCardIds.Contains(x.TechnicalCardId.Value))
                    .Select(x => new
                    {
                        TechnicalCardId = x.TechnicalCardId!.Value,
                        x.OutputWeight,
                        x.NetWeight,
                        x.GrossWeight,
                        x.UnitOfMeasureId
                    })
                    .ToListAsync();

                foreach (var row in ingredientRows)
                {
                    AddDishWeight(
                        dishWeightByCard,
                        row.TechnicalCardId,
                        ConvertToGrams(PickWeight(row.OutputWeight, row.NetWeight, row.GrossWeight), row.UnitOfMeasureId));
                }

                var semiFinishedRows = await database.TechnicalCardSemiFinishedCompositions
                    .AsNoTracking()
                    .Where(x => x.TechnicalCardId.HasValue && dishCardIds.Contains(x.TechnicalCardId.Value))
                    .Select(x => new
                    {
                        TechnicalCardId = x.TechnicalCardId!.Value,
                        x.OutputWeight,
                        x.NetWeight,
                        x.GrossWeight,
                        x.UnitOfMeasureId
                    })
                    .ToListAsync();

                foreach (var row in semiFinishedRows)
                {
                    AddDishWeight(
                        dishWeightByCard,
                        row.TechnicalCardId,
                        ConvertToGrams(PickWeight(row.OutputWeight, row.NetWeight, row.GrossWeight), row.UnitOfMeasureId));
                }
            }

            var dishes = dishRows
                .Select(d =>
                {
                    var categoryName = d.CategoryName ?? string.Empty;

                    return new DishDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        WeightLabel = BuildDishWeightLabel(d.TechnicalCardId, dishWeightByCard),
                        Price = Convert.ToInt32(d.PriceRub),
                        Category = categoryName,
                        Calories = Convert.ToInt32(d.CaloriesKcal),
                        Proteins = Convert.ToInt32(d.ProteinsG),
                        Fats = Convert.ToInt32(d.FatsG),
                        Carbs = Convert.ToInt32(d.CarbsG),
                        ImageUrl = string.IsNullOrWhiteSpace(d.ImageUrl)
                            ? "/Images/placeholder.png"
                            : "/Images/" + d.ImageUrl,
                        IsAvailable = d.IsAvailable
                    };
                })
                .ToList();

            // Напитки
            var drinkRows = await database.Drinks
                .AsNoTracking()
                .Include(q => q.Category)
                .Include(q => q.UnitOfMeasure)
                .Where(d =>
                    d.Category == null ||
                    d.Category.Name == null ||
                    d.Category.Name.Trim().ToLower() != InactiveCategoryNameLower)
                .OrderByDescending(d => d.IsAvailable)
                .ThenBy(d => d.Name)
                .Select(d => new
                {
                    Id = d.Id,
                    Name = d.Name,
                    d.Quantity,
                    UnitName = d.UnitOfMeasure != null ? d.UnitOfMeasure.Name : null,
                    Price = Convert.ToInt32(d.PriceRub),
                    d.CategoryId,
                    CategoryName = d.Category != null ? d.Category.Name : null,
                    Calories = Convert.ToInt32(d.CaloriesKcal),
                    Proteins = Convert.ToInt32(d.ProteinsG),
                    Fats = Convert.ToInt32(d.FatsG),
                    Carbs = Convert.ToInt32(d.CarbsG),
                    ImageUrl = string.IsNullOrWhiteSpace(d.ImageUrl)
                        ? "/Images/placeholder.png"
                        : "/Images/" + d.ImageUrl,
                    IsAvailable = d.IsAvailable
                })
                .ToListAsync();

            var drinks = drinkRows
                .Select(d =>
                {
                    var categoryName = d.CategoryName ?? string.Empty;

                    return new DrinkDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        VolumeLabel = !d.Quantity.HasValue || string.IsNullOrWhiteSpace(d.UnitName)
                            ? string.Empty
                            : d.Quantity.Value.ToString("0.##", CultureInfo.InvariantCulture) + " " + NormalizeVolumeUnit(d.UnitName),
                        Price = d.Price,
                        CategoryId = d.CategoryId,
                        Category = categoryName,
                        Calories = d.Calories,
                        Proteins = d.Proteins,
                        Fats = d.Fats,
                        Carbs = d.Carbs,
                        ImageUrl = string.IsNullOrWhiteSpace(d.ImageUrl)
                            ? "/Images/placeholder.png"
                            : "/Images/" + d.ImageUrl,
                        IsAvailable = d.IsAvailable
                    };
                })
                .ToList();

            // Добавки
            var toppingRows = await database.ToppingsAndSyrups
                .AsNoTracking()
                .Include(q => q.Category)
                .Include(q => q.UnitOfMeasure)
                .Where(t =>
                    t.Category == null ||
                    t.Category.Name == null ||
                    t.Category.Name.Trim().ToLower() != InactiveCategoryNameLower)
                .OrderByDescending(t => t.IsAvailable)
                .ThenBy(t => t.Name)
                .Select(t => new
                {
                    Id = t.Id,
                    t.Name,
                    t.Quantity,
                    UnitName = t.UnitOfMeasure != null ? t.UnitOfMeasure.Name : null,
                    Price = Convert.ToInt32(t.PriceRub),
                    CategoryName = t.Category != null ? t.Category.Name : null,
                    IsAvailable = t.IsAvailable
                })
                .ToListAsync();

            var toppings = toppingRows
                .Select(t =>
                {
                    var categoryName = t.CategoryName ?? string.Empty;
                    var unitName = (t.UnitName ?? string.Empty)
                        .Replace("Граммы", "гр")
                        .Replace("Миллилитры", "мл")
                        .Replace("Штуки", "шт");

                    return new ToppingDto
                    {
                        Id = t.Id,
                        Name = t.Name + " " + Convert.ToInt32(t.Quantity).ToString() + unitName,
                        Price = t.Price,
                        Category = categoryName,
                        IsAvailable = t.IsAvailable
                    };
                })
                .ToList();

            var drinkModifiers = await LoadDrinkModifierCatalogAsync();

            return Ok(new MenuResponse
            {
                Dishes = dishes,
                Drinks = drinks,
                Toppings = toppings,
                DrinkModifiers = drinkModifiers
            });
        }

        private async Task<DrinkModifierCatalogDto> LoadDrinkModifierCatalogAsync()
        {
            var allModifierIds = MilkModifierIngredientIds
                .Concat(CoffeeModifierIngredientIds)
                .Distinct()
                .ToList();

            var ingredients = await database.Ingredients
                .AsNoTracking()
                .Where(i => allModifierIds.Contains(i.Id))
                .Select(i => new
                {
                    i.Id,
                    Name = i.Name ?? $"Ингредиент #{i.Id}"
                })
                .ToDictionaryAsync(i => i.Id, i => i.Name);

            return new DrinkModifierCatalogDto
            {
                MilkOptions = BuildModifierOptions(MilkModifierIngredientIds, ingredients),
                CoffeeOptions = BuildModifierOptions(CoffeeModifierIngredientIds, ingredients)
            };
        }

        private static List<DrinkModifierOptionDto> BuildModifierOptions(
            IReadOnlyCollection<int> orderedIds,
            IReadOnlyDictionary<int, string> ingredientNamesById)
        {
            return orderedIds
                .Where(id => ingredientNamesById.ContainsKey(id))
                .Select(id => new DrinkModifierOptionDto
                {
                    Id = id,
                    Name = ingredientNamesById[id]
                })
                .ToList();
        }

        private static string BuildDishWeightLabel(int? technicalCardId, IReadOnlyDictionary<int, decimal> weightByCard)
        {
            if (!technicalCardId.HasValue || !weightByCard.TryGetValue(technicalCardId.Value, out var grams) || grams <= 0)
            {
                return string.Empty;
            }

            return grams.ToString("0.##", CultureInfo.InvariantCulture) + " гр";
        }

        private static void AddDishWeight(IDictionary<int, decimal> target, int technicalCardId, decimal grams)
        {
            if (grams <= 0)
            {
                return;
            }

            if (target.TryGetValue(technicalCardId, out var current))
            {
                target[technicalCardId] = current + grams;
            }
            else
            {
                target[technicalCardId] = grams;
            }
        }

        private static decimal PickWeight(decimal? outputWeight, decimal? netWeight, decimal? grossWeight)
        {
            if (outputWeight.HasValue && outputWeight.Value > 0)
            {
                return outputWeight.Value;
            }

            if (netWeight.HasValue && netWeight.Value > 0)
            {
                return netWeight.Value;
            }

            if (grossWeight.HasValue && grossWeight.Value > 0)
            {
                return grossWeight.Value;
            }

            return 0m;
        }

        private static decimal ConvertToGrams(decimal value, int? unitOfMeasureId)
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

        private static string NormalizeVolumeUnit(string unitName)
        {
            var normalized = unitName.Trim();

            return normalized switch
            {
                "Миллилитры" => "мл",
                "Литры" => "л",
                _ => normalized
            };
        }

    }
}
