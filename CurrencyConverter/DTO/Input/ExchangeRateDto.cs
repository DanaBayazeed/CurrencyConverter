using CurrencyConverter.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CurrencyConverter.DTO.Input
{
    public class ExchangeRateDto
    {
        [Required]
        [ExecludeCurrency("TRY", "PLN", "THP", "MXN")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be exactly 3 letters.")]
        public required string BaseCurrency { get; set; }

        [Required]
		[ExecludeCurrency("TRY", "PLN", "THP", "MXN")]
		[StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be exactly 3 letters.")]
        public required string QuoteCurrency { get; set; }


        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; } = 1;
    }
}
