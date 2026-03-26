using System.Collections.Generic;

namespace TransferModels.Menu
{
    public class MenuResponse
    {
        public List<DishDto> Dishes { get; set; } = new List<DishDto>();
        public List<DrinkDto> Drinks { get; set; } = new List<DrinkDto>();
        public List<ToppingDto> Toppings { get; set; } = new List<ToppingDto>();
        public DrinkModifierCatalogDto DrinkModifiers { get; set; } = new DrinkModifierCatalogDto();
    }

    public class DrinkModifierCatalogDto
    {
        public List<DrinkModifierOptionDto> MilkOptions { get; set; } = new List<DrinkModifierOptionDto>();
        public List<DrinkModifierOptionDto> CoffeeOptions { get; set; } = new List<DrinkModifierOptionDto>();
    }

    public class DrinkModifierOptionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
