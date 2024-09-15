using System.Text.Json.Serialization;

namespace CurrencyConverter.DTO.Output.Pagination
{
    public class Pagination
    {
        public int Total { get; private set; }

        public int Page { get;  set; }

        public int PageSize { get; private set; }

        public int Count { get; private set; }

        public int TotalPages { get; private set; }

        public bool HasPreviousPage => Page > 1;

        public bool HasNextPage => Page < TotalPages;

        [JsonIgnore]
        public int Take => PageSize;

        [JsonIgnore]
        public int Skip => (Page - 1) * PageSize;

        [JsonConstructor]
        public Pagination(int total, int? page, int? pageSize)
        {
            Total = total < 0 ? 0 : total;

            PageSize = pageSize.HasValue && pageSize > 0 ? Math.Min(Total, pageSize.Value) : Math.Min(Total, 10);

            TotalPages = Total == 0 ? 1 : (int) Math.Ceiling((decimal) Total / PageSize);

            Page = page.HasValue && page > 0 ? Math.Min(TotalPages, page.Value) : 1;

            Count = Page == TotalPages ? (Total % PageSize > 0)? Total % PageSize : PageSize : PageSize;
        }
    }

}
