using adk_lab.examples;

var exampleId = args.Length > 0 ? args[0].Trim() : "01";

Console.WriteLine($"example = {exampleId}");
Console.WriteLine();

try
{
    switch (exampleId)
    {
        case "01":
            await Example01SendNoTool.RunAsync();
            break;
        case "02":
            await Example02TextStreamNoTool.RunAsync();
            break;
        case "03":
            await Example03EventStreamWithTool.RunAsync();
            break;
        case "04":
            await Example04SendNoToolVolc.RunAsync();
            break;
        case "05":
            await Example05TextStreamNoToolVolc.RunAsync();
            break;
        case "06":
            await Example06EventStreamWithToolVolc.RunAsync();
            break;
        default:
            Console.WriteLine("Usage:");
            Console.WriteLine("  dotnet run --project .\\adk-lab.csproj -- 01");
            Console.WriteLine("  dotnet run --project .\\adk-lab.csproj -- 02");
            Console.WriteLine("  dotnet run --project .\\adk-lab.csproj -- 03");
            Console.WriteLine("  dotnet run --project .\\adk-lab.csproj -- 04");
            Console.WriteLine("  dotnet run --project .\\adk-lab.csproj -- 05");
            Console.WriteLine("  dotnet run --project .\\adk-lab.csproj -- 06");
            break;
    }
}
catch (Exception ex)
{
    Console.WriteLine("error:");
    Console.WriteLine(ex.Message);
    if (ex.InnerException is not null)
    {
        Console.WriteLine();
        Console.WriteLine("inner:");
        Console.WriteLine(ex.InnerException.Message);
    }
}
