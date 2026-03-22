using System.Net.Http.Json;
using System.Web;

namespace WeMakeMVPTestTask.TreasuryApi
{
	/// <summary>
	/// Build the URL and deserialize the response
	/// </summary>
	public class TreasuryApiClient
	{
		private const string Endpoint =
			"services/api/fiscal_service/v2/accounting/od/debt_to_penny";

		private const string Fields = "record_date,tot_pub_debt_out_amt";

		private readonly HttpClient _http;

		public TreasuryApiClient(HttpClient http) => _http = http;

		public async Task<TreasuryApiResponse> GetDebtRecordsAsync(DebtQueryParameters p)
		{
			var query = HttpUtility.ParseQueryString(string.Empty);

			query["fields"] = Fields;
			query["sort"] = p.Sort;
			query["page[number]"] = p.PageNumber.ToString();
			query["page[size]"] = Math.Clamp(p.PageSize, 1, 100).ToString();

			var filters = new List<string>();
			if (!string.IsNullOrWhiteSpace(p.StartDate))
				filters.Add($"record_date:gte:{p.StartDate}");
			if (!string.IsNullOrWhiteSpace(p.EndDate))
				filters.Add($"record_date:lte:{p.EndDate}");
			if (filters.Count > 0)
				query["filter"] = string.Join(",", filters);

			var url = $"{Endpoint}?{query}";

			var response = await _http.GetAsync(url);
			response.EnsureSuccessStatusCode();

			return await response.Content.ReadFromJsonAsync<TreasuryApiResponse>()
				   ?? throw new InvalidOperationException("Empty response from Treasury API.");
		}
	}
}
