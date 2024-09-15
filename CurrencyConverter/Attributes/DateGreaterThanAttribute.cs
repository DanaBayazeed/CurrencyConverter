using System.ComponentModel.DataAnnotations;

namespace CurrencyConverter.Attributes
{
    public class DateGreaterThanAttribute(string comparisonProperty) : ValidationAttribute
    {
        private readonly string _comparisonProperty = comparisonProperty;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult("The date value could not be null");

            var currentValue = DateOnly.Parse(value.ToString()!); ;

            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (property == null)
                throw new ArgumentException("Property with this name not found");

            var comparisonValue = property.GetValue(validationContext.ObjectInstance);

            if (comparisonValue == null)
                return new ValidationResult($"The value of {_comparisonProperty} could not be null.");

            var parsedComparisonValue = DateOnly.Parse(comparisonValue.ToString()!);

            if (currentValue > parsedComparisonValue)
                return ValidationResult.Success;

            return new ValidationResult(ErrorMessage ?? "The end date must be greater than the start date.");
        }
    }
}
