using System.ComponentModel.DataAnnotations;

namespace CurrencyConverter.Attributes
{
    public class ExecludeCurrencyAttribute(params string[] currencies) : ValidationAttribute
    {
        private readonly string[] _currencies = currencies;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
			if (value is string currency && _currencies.Contains(currency, StringComparer.OrdinalIgnoreCase))
				return new ValidationResult("Currency conversion not supported.");

			return ValidationResult.Success!;
        }
    }
}
