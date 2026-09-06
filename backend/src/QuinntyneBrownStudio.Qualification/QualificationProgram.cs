using Microsoft.Extensions.Configuration;
namespace QuinntyneBrownStudio.Qualification;

public static class QualificationProgram
{
    public static async Task<int> Main(string[] args)
    {
        using var cancellation = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) => { eventArgs.Cancel = true; cancellation.Cancel(); };
        return await new QualificationRunner(new ConfigurationBuilder().AddEnvironmentVariables().Build()).Run(args, cancellation.Token);
    }
}
