using System.ComponentModel.DataAnnotations;

namespace CurrencyConverter.DTO.Input.Pagination
{
	public class PagedRequestDto
	{
		[Range(0, 1000, ErrorMessage = "Page must be equal or greater than 0.")] 
		public int Page { get; set; } = 1;

		[Range(5, 20, ErrorMessage = "Page must be equal or greater than 5.")]
		public int PageSize { get; set; } = 10;
	}
}
