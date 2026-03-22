using OpenAI.Chat;

namespace WeMakeMVPTestTask.Tools
{
	/// <summary>
	/// Declares the two MCP tools exposed to the LLM.
	/// </summary>
	public static class ToolDefinitions
	{
		public static readonly ChatTool GetCurrentDate = ChatTool.CreateFunctionTool(
			functionName: "get_current_date",
			functionDescription: "Returns today's date in YYYY-MM-DD format. " +
						 "Always call this first when the user asks about current or latest debt.",
			functionParameters: BinaryData.FromString("""
            {
                "type": "object",
                "properties": {},
                "required": []
            }
            """)
		);

		public static readonly ChatTool GetUsDebt = ChatTool.CreateFunctionTool(
			functionName: "get_us_debt",
			functionDescription: """
            Fetches U.S. public debt records from the Treasury "Debt to the Penny" API.
            The dataset starts on 1993-01-04 — do not query earlier dates.
            Each record has: record_date (YYYY-MM-DD) and tot_pub_debt_out_amt (total debt in USD).
            Use start_date/end_date to narrow the range; use sort and page_size to control output.
            For a single "what was the debt on / around date X" question, request the closest 
            record by sorting appropriately and limiting to 1 result.
            For year-level questions (e.g. "debt in 2008") fetch the last record of that year: 
            start_date=YYYY-01-01, end_date=YYYY-12-31, sort=-record_date, page_size=1.
            For growth/difference questions fetch the first record of the start period and 
            the last record of the end period in two separate calls.
            """,
			functionParameters: BinaryData.FromString("""
            {
                "type": "object",
                "properties": {
                    "start_date": {
                        "type": "string",
                        "description": "Inclusive start of date range (YYYY-MM-DD)."
                    },
                    "end_date": {
                        "type": "string",
                        "description": "Inclusive end of date range (YYYY-MM-DD)."
                    },
                    "sort": {
                        "type": "string",
                        "description": "Field to sort by. Use 'record_date' (ascending) or '-record_date' (descending).",
                        "enum": ["record_date", "-record_date"],
                        "default": "-record_date"
                    },
                    "page_size": {
                        "type": "integer",
                        "description": "How many records to return (1–100). Default 5.",
                        "default": 5
                    },
                    "page_number": {
                        "type": "integer",
                        "description": "1-based page index for pagination. Default 1.",
                        "default": 1
                    }
                },
                "required": []
            }
            """)
		);
	}
}
