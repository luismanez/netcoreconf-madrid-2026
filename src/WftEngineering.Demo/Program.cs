using DotNetEnv;
using DotNetEnv.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using WftEngineering.Demo;

var console = AnsiConsole.Console;

if (args is ["--help"] or ["-h"])
{
    console.MarkupLine("[cyan]Production Change Assistant[/]");
    console.WriteLine("Run from the repository root after copying env.template to .env.");
    console.WriteLine("Usage: dotnet run --project src/WftEngineering.Demo");
    return 0;
}

if (args.Length > 0)
{
    console.MarkupLine("[red]Unknown arguments.[/] Use --help for usage. Scenarios will be added with the agent features.");
    return 2;
}

try
{
    // https://github.com/tonerdo/dotnet-env#using-net-configuration-provider
    using var configuration = new ConfigurationManager();
    configuration.AddDotNetEnv(".env", LoadOptions.NoEnvVars()).AddEnvironmentVariables();
    var settings = DemoSettings.FromConfiguration(configuration);

    // https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection/basics
    var services = new ServiceCollection();
    services.AddSingleton<IConfiguration>(configuration);
    services.AddSingleton(settings);
    services.AddSingleton<IAnsiConsole>(console);
    services.AddSingleton<DemoApplication>();

    using var provider = services.BuildServiceProvider(new ServiceProviderOptions
    {
        ValidateOnBuild = true,
        ValidateScopes = true
    });

    provider.GetRequiredService<DemoApplication>().Run();
    return 0;
}
catch (DemoConfigurationException error)
{
    console.MarkupLine($"[red]Configuration error:[/] {Markup.Escape(error.Message)}");
    console.WriteLine("Copy env.template to .env and fill in the required variables.");
    return 1;
}
catch (Exception)
{
    console.MarkupLine("[red]Startup failed.[/] Check .env syntax, file access, and terminal output.");
    return 1;
}
