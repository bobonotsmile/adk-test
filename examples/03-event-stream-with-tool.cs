using System.Diagnostics;
using System.Text.Json;
using Crystal.Adk.Core;
using Crystal.Adk.Providers;

namespace adk_lab.examples;

internal static class Example03EventStreamWithTool
{
    public static async Task RunAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        var toolHub = new ToolHub();
        toolHub.Register(new QueryDeviceInfoTool());
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

        var host = new AgentHost(toolHub, provider, new AgentHostOptions
        {
            SystemPrompt = "如果用户要求查询设备信息，请优先调用 query_device_info 工具。",
            MaxRounds = 6
        });

        var session = host.CreateSession();
        var prompt = "请先说明你要做什么，然后调用 query_device_info 工具查询设备编号 EQ-001 的信息，最后总结设备状态。";

        Console.WriteLine("scenario = event-stream-with-tool");
        Console.WriteLine($"prompt = {prompt}");
        Console.WriteLine();

        await foreach (var evt in session.StreamEventsAsync(prompt))
        {
            if (evt.Kind == AgentEventKinds.ToolCallStarted)
            {
                Console.WriteLine($"[tool_call_started] {evt.ToolName} args={JsonSerializer.Serialize(evt.Arguments)}");
                continue;
            }

            if (evt.Kind == AgentEventKinds.ToolCallCompleted)
            {
                Console.WriteLine($"[tool_call_completed] {evt.ToolName} result={JsonSerializer.Serialize(evt.Result)} elapsedMs={evt.ElapsedMs}");
                continue;
            }

            if (evt.Kind == AgentEventKinds.FinalAnswer)
            {
                Console.WriteLine($"[final_answer] {evt.Text}");
                continue;
            }

            Console.WriteLine($"[{evt.Kind}] {evt.Text ?? evt.ErrorMessage}");
        }

        stopwatch.Stop();
        Console.WriteLine();
        Console.WriteLine($"elapsedMs = {stopwatch.ElapsedMilliseconds}");
    }

    private sealed class QueryDeviceInfoTool : IAgentTool
    {
        public AgentToolDescriptor Descriptor { get; } = new()
        {
            Name = "query_device_info",
            Description = "根据设备编号查询设备基础信息、运行状态和最近一次维护情况。",
            ParametersSchema = new
            {
                type = "object",
                properties = new
                {
                    deviceCode = new { type = "string", description = "设备编号，例如 EQ-001" }
                },
                required = new[] { "deviceCode" }
            }
        };

        public Task<object?> InvokeAsync(Dictionary<string, object?> arguments, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var deviceCode = arguments.TryGetValue("deviceCode", out var value) ? value?.ToString() : null;
            if (string.IsNullOrWhiteSpace(deviceCode))
            {
                throw new InvalidOperationException("deviceCode is required");
            }

            var fakeResult = new
            {
                deviceCode = deviceCode.Trim(),
                deviceName = "激光打标机 A1",
                workshop = "二号车间",
                line = "装配线-3",
                status = "running",
                healthLevel = "good",
                temperature = 42.6,
                lastMaintenanceAt = "2026-03-20 09:30:00",
                maintainer = "张工",
                remark = "设备运行稳定，建议 7 天内做例行巡检。"
            };

            return Task.FromResult<object?>(fakeResult);
        }
    }
}
