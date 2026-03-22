using OpenAI.Chat;
using WeMakeMVPTestTask.Tools;

namespace WeMakeMVPTestTask
{
	/// <summary>
	/// Owns the in-RAM conversation history and drives the LLM tools agentic loop.
	/// A new instance means a fresh session
	/// </summary>
	public class ChatService
	{
		private readonly ChatClient _llmClient;
		private readonly ToolExecutor _toolExecutor;

		// Full conversation history kept in RAM for the lifetime of this instance.
		private readonly List<ChatMessage> _history = new();

		private static readonly ChatCompletionOptions LlmOptions = new()
		{
			Tools = { ToolDefinitions.GetCurrentDate, ToolDefinitions.GetUsDebt }
		};

		public ChatService(ToolExecutor toolExecutor)
		{
			var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
				?? throw new InvalidOperationException(
					"Set the OPENAI_API_KEY environment variable before running.");

			_llmClient = new ChatClient("gpt-4o-mini", apiKey);
			_toolExecutor = toolExecutor;

			_history.Add(new SystemChatMessage(SystemPrompt.Text));
		}

		/// <summary>
		/// Appends the user turn to history, then runs the agentic loop until the 
		/// model produces a final text reply
		/// </summary>
		public async Task<string> SendMessageAsync(string userMessage)
		{
			_history.Add(new UserChatMessage(userMessage));

			// Agentic loop: keep going as long as the model wants to call tools.
			while (true)
			{
				var completion = await _llmClient.CompleteChatAsync(_history, LlmOptions);

				if (completion.Value.FinishReason == ChatFinishReason.ToolCalls)
				{
					// 1. Record the assistant turn that triggered the tool calls
					_history.Add(new AssistantChatMessage(completion.Value));

					// 2. Execute every tool the model requested
					foreach (var toolCall in completion.Value.ToolCalls)
					{
						var result = await _toolExecutor.ExecuteAsync(
							toolCall.FunctionName,
							toolCall.FunctionArguments.ToString());

						_history.Add(new ToolChatMessage(toolCall.Id, result));
					}
				}
				else
				{
					// Final text answer.
					var answer = completion.Value.Content[0].Text;
					_history.Add(new AssistantChatMessage(answer));
					return answer;
				}
			}
		}
	}
}
