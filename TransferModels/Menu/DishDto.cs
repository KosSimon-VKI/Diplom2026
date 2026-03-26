using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferModels.Menu
{
    public class DishDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string WeightLabel { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public int Calories { get; set; }
        public int Proteins { get; set; }
        public int Fats { get; set; }
        public int Carbs { get; set; }
        public string ImageUrl { get; set; } // Новое поле
        public bool IsAvailable { get; set; }

    }
}
