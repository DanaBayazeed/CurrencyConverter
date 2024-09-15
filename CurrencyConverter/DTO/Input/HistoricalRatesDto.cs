using CurrencyConverter.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CurrencyConverter.DTO.Input
{
    public class HistoricalRatesDto
    {
        [Required]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be exactly 3 letters.")]
        public required string BaseCurrency { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Date must be in the format YYYY-MM-DD.")]
        public required string From { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Date must be in the format YYYY-MM-DD.")]
        [DateGreaterThan("From", ErrorMessage = "The end date must be greater than the start date.")]
        public required string To { get; set; }

    }
}
