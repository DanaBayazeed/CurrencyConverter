

namespace CurrencyConverter.DTO.Output.Pagination
{
    public class PagedResponseDto<T>
    {
        public T? Data { get; private set; }

        public Pagination? Pagination { get; private set; } = null;

        public PagedResponseDto(){}

        public PagedResponseDto(T data, Pagination pagination)
        {
            Data = data;
            Pagination = pagination;
        }
    }
}