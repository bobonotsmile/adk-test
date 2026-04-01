using System.Diagnostics;
using Crystal.Adk.Core;
using Crystal.Adk.Providers;

namespace adk_lab.examples;

internal static class Example05TextStreamNoToolVolc
{
    public static async Task RunAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        using var httpClient = new HttpClient();

        // 火山引擎 / 方舟协议当前走 Vendor = "ark"
        var provider = ChatProvider.Create(httpClient, new ChatProviderSettings
        {
            Vendor = "ark",
            ApiKey = "6a022b0c-ad9a-4db5-b93b-3398a57c166f",
            BaseUrl = "https://ark.cn-beijing.volces.com/api/v3/chat/completions",
            Model = "doubao-seed-2-0-pro-260215",
            Temperature = 0.2,
            TopP = 0.9,
            MaxOutputTokens = 512,
            EnableThinking = false
        });

        var host = new AgentHost(provider, new AgentHostOptions
        {
            SystemPrompt = "你是一个简洁的助手。",
            MaxRounds = 4
        });

        var session = host.CreateSession();
        var prompt = "请解释什么是设备履历分析，不要调用任何工具。";

        Console.WriteLine("scenario = text-stream-no-tool-volc");
        Console.WriteLine($"prompt = {prompt}");
        Console.WriteLine();

        await foreach (var chunk in session.StreamTextAsync(prompt))
        {
            Console.Write(chunk.Text);
        }

        stopwatch.Stop();
        Console.WriteLine();
        Console.WriteLine($"elapsedMs = {stopwatch.ElapsedMilliseconds}");
    }
}
