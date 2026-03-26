using System.Linq;

namespace GardenNookWeb.Utils
{
    public static class PhoneNumberNormalizer
    {
        public static bool TryNormalizeRussian(string? phoneNumber, out string normalized)
        {
            normalized = string.Empty;

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return false;
            }

            var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());

            if (digits.Length == 10)
            {
                digits = "7" + digits;
            }

            if (digits.Length != 11 || digits[0] != '7')
            {
                return false;
            }

            normalized = digits;
            return true;
        }
    }
}
