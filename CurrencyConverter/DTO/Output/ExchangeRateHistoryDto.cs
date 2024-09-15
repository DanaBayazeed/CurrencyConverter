namespace CurrencyConverter.DTO.Output
{
    public class ExchangeRateHistoryDto
    {
        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; } = [];

        public void Normalize(DateTime startDate, DateTime endDate)
        {
            StartDate = DateOnly.FromDateTime(startDate);

            EndDate = DateOnly.FromDateTime(endDate);

            for (DateOnly currentDate = StartDate; currentDate <= EndDate; currentDate = currentDate.AddDays(1))
			{
				string dateKey = currentDate.ToString("yyyy-MM-dd");

				if (!Rates.ContainsKey(dateKey))
					Rates[dateKey] = null!;
			}

			Rates = Rates.OrderBy(rate => DateTime.Parse(rate.Key)).ToDictionary();
		}
	 }
}
