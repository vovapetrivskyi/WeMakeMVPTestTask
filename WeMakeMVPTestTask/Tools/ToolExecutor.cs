using System.Text.Json;
using WeMakeMVPTestTask.TreasuryApi;

namespace WeMakeMVPTestTask.Tools
{
	/// <summary>
	/// Dispatches tool-call requests from the LLM to the correct handler.
	/// </summary>
	public class ToolExecutor
	{
		private readonly TreasuryApiClient _treasuryClient;

		public ToolExecutor(TreasuryApiClient treasuryClient)
		{
			_treasuryClient = treasuryClient;
		}

		/// <summary>
		/// Executes the named tool with the JSON argument string the LLM produced,
		/// and returns a plain-text result to feed back into the conversation.
		/// </summary>
		public async Task<string> ExecuteAsync(string toolName, string argumentsJson)
		{
			return toolName switch
			{
				"get_current_date" => ExecuteGetCurrentDate(),
				"get_us_debt" => await ExecuteGetUsDebtAsync(argumentsJson),
				_ => $"Unknown tool: {toolName}"
			};
		}

		private static string ExecuteGetCurrentDate()
			=> DateTime.UtcNow.ToString("yyyy-MM-dd");

		private async Task<string> ExecuteGetUsDebtAsync(string argumentsJson)
		{
			var args = JsonDocument.Parse(argumentsJson).RootElement;

			var parameters = new DebtQueryParameters
			{
				StartDate = args.TryGetProperty("start_date", out var sd) ? sd.GetString() : null,
				EndDate = args.TryGetProperty("end_date", out var ed) ? ed.GetString() : null,
				Sort = args.TryGetProperty("sort", out var so) ? so.GetString() ?? "-record_date" : "-record_date",
				PageSize = args.TryGetProperty("page_size", out var ps) ? ps.GetInt32() : 5,
				PageNumber = args.TryGetProperty("page_number", out var pn) ? pn.GetInt32() : 1,
			};

			var response = await _treasuryClient.GetDebtRecordsAsync(parameters);

			if (response.Data.Count == 0)
				return "No records found for the specified date range.";

			var lines = response.Data.Select(r =>
				$"{r.RecordDate}: ${r.TotalPublicDebtOutstanding:N2}");

			var meta = $"(Showing {response.Data.Count} of {response.Meta.TotalCount} record(s))";
			return string.Join("\n", lines) + "\n" + meta;
		}
	}
}
