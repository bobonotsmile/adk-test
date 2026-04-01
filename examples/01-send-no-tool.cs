using System.Diagnostics;
using Crystal.Adk.Core;
using Crystal.Adk.Providers;

namespace adk_lab.examples;

internal static class Example01SendNoTool
{
    public static async Task RunAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        using var httpClient = new HttpClient();

        // 直接改这里的 provider 参数
        var provider = ChatProvider.Create(httpClient, new ChatProviderSettings
        {
            Vendor = "ollama",
            BaseUrl = "http://172.31.12.93:11434/api/chat",
            Model = "gpt-oss:20b",
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
        var prompt = "请用三句话介绍你自己，不要调用任何工具。";

        Console.WriteLine("scenario = send-no-tool");
        Console.WriteLine($"prompt = {prompt}");
        Console.WriteLine();

        var result = await session.RunAsync(prompt);
        stopwatch.Stop();
        Console.WriteLine(result.FinalMessage);
        Console.WriteLine();
        Console.WriteLine($"elapsedMs = {stopwatch.ElapsedMilliseconds}");
    }
}
