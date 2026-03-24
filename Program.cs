using McpTestClient.LLM;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Client;

namespace McpTestClient
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Configuration
                .AddEnvironmentVariables()
                .AddUserSecrets<Program>();

            string apiKey = builder.Configuration["ANTHROPIC_API_KEY"] ?? throw new InvalidOperationException("ANTHROPIC_API_KEY is not set in configuration.");

            var (command, arguments) = GetCommandAndArguments(args);

            var clientTransport = new StdioClientTransport(new()
            {
                Name = "Demo Server",
                Command = command,
                Arguments = arguments,
            });

            await using var mcpClient = await McpClient.CreateAsync(clientTransport);

            var tools = await mcpClient.ListToolsAsync();
            foreach (var tool in tools)
            {
                Console.WriteLine($"Connected to server with tools: {tool.Name}");
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("MCP Client Started!");
            Console.ResetColor();

            PromptForInput();
            while (Console.ReadLine() is string query && !"exit".Equals(query, StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    PromptForInput();
                    continue;
                }

                Console.WriteLine();

                await foreach (var message in ClaudeLLM.GetStreamingResponseAsync(query, tools, apiKey))
                {
                    Console.Write(message);
                }

                PromptForInput();
            }
        }

        static (string command, string[] arguments) GetCommandAndArguments(string[] args)
        {
            return args switch
            {
                [var script] when script.EndsWith(".py") => ("python", args),
                [var script] when script.EndsWith(".js") => ("node", args),
                [var script] when Directory.Exists(script) || (File.Exists(script) && script.EndsWith(".csproj")) => ("dotnet", ["run", "--project", script, "--no-build"]),
                _ => throw new NotSupportedException("An unsupported server script was provided. Supported scripts are .py, .js, or .csproj")
            };
        }

        static void PromptForInput()
        {
            Console.WriteLine("Enter a command (or 'exit' to quit):");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("> ");
            Console.ResetColor();
        }
    }
}
