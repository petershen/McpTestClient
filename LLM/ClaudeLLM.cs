using Anthropic.SDK;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using System.Text;

namespace McpTestClient.LLM
{
    internal static class ClaudeLLM
    {
        public static async Task<string> GetResponseAsync(string prompt, IList<McpClientTool>? tools, string apiKey)
        {
            using var anthropicClient = new AnthropicClient(new APIAuthentication(apiKey))
                .Messages
                .AsBuilder()
                .UseFunctionInvocation()
                .Build();

            var options = new ChatOptions
            {
                MaxOutputTokens = 1000,
                ModelId = "claude-sonnet-4-20250514",
                Tools = [.. tools]
            };

            StringBuilder responseBuilder = new StringBuilder();
            await foreach (var message in anthropicClient.GetStreamingResponseAsync(prompt, options))
            {
                responseBuilder.Append(message);
                Thread.Sleep(10);
            }

            return responseBuilder.ToString();
        }

        public static async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(string prompt, IList<McpClientTool>? tools, string apiKey)
        {
            using var anthropicClient = new AnthropicClient(new APIAuthentication(apiKey))
                .Messages
                .AsBuilder()
                .UseFunctionInvocation()
                .Build();

            var options = new ChatOptions
            {
                MaxOutputTokens = 1000,
                ModelId = "claude-sonnet-4-20250514",
                Tools = [.. tools]
            };

            await foreach (var message in anthropicClient.GetStreamingResponseAsync(prompt, options))
            {
                yield return message;
            }
        }
    }
}
