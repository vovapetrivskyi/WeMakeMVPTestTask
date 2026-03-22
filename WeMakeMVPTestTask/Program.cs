using Microsoft.Extensions.DependencyInjection;
using WeMakeMVPTestTask;
using WeMakeMVPTestTask.Tools;
using WeMakeMVPTestTask.TreasuryApi;

var services = new ServiceCollection();

services.AddSingleton(_ =>
{
	var http = new HttpClient
	{
		BaseAddress = new Uri("https://api.fiscaldata.treasury.gov/")
	};
	http.DefaultRequestHeaders.Add("Accept", "application/json");
	return http;
});

services.AddSingleton<TreasuryApiClient>();
services.AddSingleton<ToolExecutor>();
services.AddSingleton<ChatService>();

var sp = services.BuildServiceProvider();

var chat = sp.GetRequiredService<ChatService>();

Console.WriteLine("U.S. Public Debt Assistant");
Console.WriteLine("Type your question or \"exit\" to quit.\n");

while (true)
{
	Console.Write("You: ");

	var input = Console.ReadLine();

	if (string.IsNullOrWhiteSpace(input))
	{
		continue;
	}

	if (input.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
	{
		break;
	}

	try
	{
		var response = await chat.SendMessageAsync(input.Trim());

		Console.Write("Assistant: ");

		Console.WriteLine(response);
	}
	catch (Exception ex)
	{
		Console.WriteLine($"Error: {ex.Message}");
	}

	Console.WriteLine();
}