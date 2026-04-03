using System.Text;
using Crystal.Adk.Abstractions;
using Crystal.Adk.Providers;
using Crystal.Adk.Session;

Console.InputEncoding = new UTF8Encoding(false);
Console.OutputEncoding = new UTF8Encoding(false);

var env = LoadDotEnv(ResolveDotEnvPath());
var apiKey = GetRequiredEnv(env, "ARK_API_KEY");
var model = GetRequiredEnv(env, "OLLAMA_MODEL");
var baseUrl = env.TryGetValue("OLLAMA_BASE_URL", out var envBaseUrl) && !string.IsNullOrWhiteSpace(envBaseUrl)
    ? envBaseUrl
    : "https://ark.cn-beijing.volces.com/api/v3/chat/completions";

var options = new ChatProviderOptions
{
    Vendor = "ollama",
    //ApiKey = apiKey,
    Model = model,
    BaseUrl = baseUrl,
    TimeoutMs = 60000,
    Temperature = 0.7,
    TopP = 0.9,
    MaxOutputTokens = 4096,
    EnableThinking = false
};

Console.WriteLine($"vendor: {options.Vendor}");
Console.WriteLine($"model: {options.Model}");
Console.WriteLine($"url: {options.BaseUrl}");
Console.WriteLine("输入空行退出。输入 /stream 开启一次流式模式。");

using var httpClient = new HttpClient();
var provider = ChatProviderFactory.Create(httpClient, options);
var history = new SessionMessageManager(new[]
{
    new RuntimeMessage
    {
        Role = "system",
        Content = "你是一个简洁的助手。"
    }
});
var session = new AgentSession(provider, history);

while (true)
{
    Console.Write("\nuser> ");
    var userInput = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(userInput))
    {
        break;
    }

    if (string.Equals(userInput, "/stream", StringComparison.OrdinalIgnoreCase))
    {
        Console.Write("stream user> ");
        var streamInput = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(streamInput))
        {
            continue;
        }

        var thinkingStarted = false;
        var answerStarted = false;

        await foreach (var chunk in session.StreamTextAsync(streamInput))
        {
            if (!string.IsNullOrEmpty(chunk.ThinkingText))
            {
                if (!thinkingStarted)
                {
                    Console.Write("\nthinking> ");
                    thinkingStarted = true;
                }

                Console.Write(chunk.ThinkingText);
            }

            if (!string.IsNullOrEmpty(chunk.Text))
            {
                if (!answerStarted)
                {
                    Console.Write("\nassistant> ");
                    answerStarted = true;
                }

                Console.Write(chunk.Text);
            }
        }

        Console.WriteLine();
        continue;
    }

    var assistant = await session.RunAsync(userInput);
    Console.WriteLine();

    if (!string.IsNullOrWhiteSpace(assistant.ThinkingContent))
    {
        Console.WriteLine($"thinking> {assistant.ThinkingContent}");
    }

    Console.WriteLine($"assistant> {assistant.Content}");
}

static Dictionary<string, string> LoadDotEnv(string filePath)
{
    if (!File.Exists(filePath))
    {
        throw new FileNotFoundException($"未找到 .env 文件: {filePath}");
    }

    var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    foreach (var rawLine in File.ReadAllLines(filePath))
    {
        var line = rawLine.Trim();
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
        {
            continue;
        }

        var separatorIndex = line.IndexOf('=');
        if (separatorIndex <= 0)
        {
            continue;
        }

        var key = line[..separatorIndex].Trim();
        var value = line[(separatorIndex + 1)..].Trim();

        if (value.Length >= 2 &&
            ((value.StartsWith('"') && value.EndsWith('"')) ||
             (value.StartsWith('\'') && value.EndsWith('\''))))
        {
            value = value[1..^1];
        }

        values[key] = value;
    }

    return values;
}

static string GetRequiredEnv(IReadOnlyDictionary<string, string> env, string key)
{
    if (env.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
    {
        return value;
    }

    throw new InvalidOperationException($"缺少必填配置: {key}");
}

static string ResolveDotEnvPath()
{
    var candidates = new[]
    {
        Path.Combine(Directory.GetCurrentDirectory(), ".env"),
        Path.Combine(AppContext.BaseDirectory, ".env")
    };

    foreach (var candidate in candidates)
    {
        if (File.Exists(candidate))
        {
            return candidate;
        }
    }

    throw new FileNotFoundException(
        ".env 文件未找到。已查找位置：" + string.Join("；", candidates),
        candidates[0]);
}
