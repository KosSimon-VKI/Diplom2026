namespace TransferModels.Menu
{
    public class DrinkDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string VolumeLabel { get; set; }
        public decimal Price { get; set; }
        public int? CategoryId { get; set; }
        public string Category { get; set; }
        public int Calories { get; set; }
        public int Proteins { get; set; }
        public int Fats { get; set; }
        public int Carbs { get; set; }
        public string ImageUrl { get; set; } // Новое поле
        public bool IsAvailable { get; set; }

    }
}
