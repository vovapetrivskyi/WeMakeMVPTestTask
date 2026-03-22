# U.S. Public Debt Console Assistant

A chat-based .NET 8 console app that answers questions about U.S. public debt  
using the Treasury **"Debt to the Penny"** dataset and an OpenAI LLM.


## Quick start

```bash
# 1. Set your OpenAI API key
export OPENAI_API_KEY=sk-...          # Linux / macOS
set    OPENAI_API_KEY=sk-...          # Windows CMD
$env:OPENAI_API_KEY = "sk-..."        # PowerShell

# 2. Run
dotnet run
```

## MCP tools

| Tool | Purpose |
|------|---------|
| `get_current_date` | Returns today's date (YYYY-MM-DD). Called before "current debt" queries. |
| `get_us_debt` | Fetches debt records from the Treasury API. Supports `start_date`, `end_date`, `sort`, `page_size`, `page_number`. |

## Conversation memory

All messages (system, user, assistant, tool results) are stored in a  
`List<ChatMessage>` on the `ChatService` instance. The list lives entirely  
in RAM and is discarded when the process exits — no files or databases used.

## Key design decisions

- **Single agentic loop** in `ChatService` keeps all LLM orchestration in one place.
- **`ToolExecutor`** is the only class that knows about tool implementations — adding a  
  new tool requires changes in `ToolDefinitions.cs` (schema) and `ToolExecutor.cs` (handler) only.
- **`TreasuryApiClient`** builds the filter/sort/page query string from a typed  
  `DebtQueryParameters` object — no raw string manipulation outside that class.
- **`StringToDecimalConverter`** handles the API's quirk of returning numbers as strings.
- **`SystemPrompt`** is a standalone file so prompt engineering is separated from code.

## Example questions

```
What is the current U.S. debt?
What was the U.S. debt in 2008?
How much did the debt increase in 2024?
What was the debt in 1886?          → "I don't have data for that period."
What is the GDP of Germany?         → "I don't have expertise in that area."
```