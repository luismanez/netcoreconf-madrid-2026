using Spectre.Console;

namespace WftEngineering.Demo;

internal sealed class DemoApplication(IAnsiConsole console, DemoSettings settings)
{
    public void Run()
    {
        // https://spectreconsole.net/console/widgets/figlet
        console.Write(new FigletText("WTF").Color(Color.Cyan1));
        console.Write(new Panel(
            "[bold]Production Change Assistant[/]\n[dim]Prompt · Context · Loop · Graph · Harness[/]")
            .Header("WTF Engineering")
            .RoundedBorder()
            .BorderColor(Color.Cyan1));
        console.WriteLine();

        // https://spectreconsole.net/console/how-to/displaying-tabular-data/
        var table = new Table().RoundedBorder().BorderColor(Color.Grey);
        table.AddColumn("Configuration");
        table.AddColumn("Status");
        table.AddRow("Foundry project", "[green]Configured[/]");
        table.AddRow("Model deployment", "[green]Configured[/]");
        table.AddRow("Telemetry transport", settings.OtlpProtocol.ToUpperInvariant());
        table.AddRow("Telemetry export", "[yellow]Pending feature 03[/]");
        console.Write(table);
        console.WriteLine();
        console.MarkupLine("[green]Scaffolding ready.[/] The agent workflow is not implemented yet.");
    }
}
