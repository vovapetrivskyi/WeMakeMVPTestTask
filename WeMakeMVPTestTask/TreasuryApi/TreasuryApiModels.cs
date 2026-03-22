using System.Text.Json.Serialization;

namespace WeMakeMVPTestTask.TreasuryApi
{
	/// <summary>
	/// Parameters collected from the LLM tool call and passed to the API client
	/// </summary>
	public class DebtQueryParameters
	{
		public string? StartDate { get; init; }
		public string? EndDate { get; init; }
		public string Sort { get; init; } = "-record_date";
		public int PageSize { get; init; } = 5;
		public int PageNumber { get; init; } = 1;
	}

	/// <summary>
	/// Treasury Fiscal Data API response
	/// </summary>
	public class TreasuryApiResponse
	{
		[JsonPropertyName("data")]
		public List<DebtRecord> Data { get; init; } = new();

		[JsonPropertyName("meta")]
		public ResponseMeta Meta { get; init; } = new();
	}

	/// <summary>
	/// Single daily debt record
	/// </summary>
	public class DebtRecord
	{
		[JsonPropertyName("record_date")]
		public string RecordDate { get; init; } = string.Empty;

		[JsonPropertyName("tot_pub_debt_out_amt")]
		[JsonConverter(typeof(StringToDecimalConverter))]
		public decimal TotalPublicDebtOutstanding { get; init; }
	}

	/// <summary>
	/// Pagination metadata
	/// </summary>
	public class ResponseMeta
	{
		[JsonPropertyName("total-count")]
		public int TotalCount { get; init; }
	}


	/// <summary>
	/// The Treasury API returns numeric values as JSON strings.
	/// This converter handles transparent parsing to decimal.
	/// </summary>
	public class StringToDecimalConverter : JsonConverter<decimal>
	{
		public override decimal Read(
			ref System.Text.Json.Utf8JsonReader reader,
			Type typeToConvert,
			System.Text.Json.JsonSerializerOptions options)
		{
			if (reader.TokenType == System.Text.Json.JsonTokenType.String)
			{
				var raw = reader.GetString();
				return decimal.TryParse(raw, System.Globalization.NumberStyles.Any,
										System.Globalization.CultureInfo.InvariantCulture,
										out var value)
					? value
					: 0m;
			}
			return reader.GetDecimal();
		}

		public override void Write(
			System.Text.Json.Utf8JsonWriter writer, decimal value,
			System.Text.Json.JsonSerializerOptions options)
			=> writer.WriteStringValue(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
	}
}
