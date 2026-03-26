using System.ComponentModel.DataAnnotations;

namespace GardenNookWeb.Models
{
    public class RegistrationModel
    {
        [Required(ErrorMessage = "Введите полное имя")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Введите номер телефона")]
        [RegularExpression(@"^7\d{10}$", ErrorMessage = "Введите номер телефона в формате +7 (999) 123-45-67")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        public string Password { get; set; }
        public string? Error { get; set; }
        public string? SuccsesMessage { get; set; }
    }
}
