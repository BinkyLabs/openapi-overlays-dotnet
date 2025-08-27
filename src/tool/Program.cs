using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BinkyLabs.OpenApi.Overlays;

namespace BinkyLabs.OpenApi.Overlays.Cli;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        // Create the root command
        var rootCommand = new RootCommand("OpenAPI Overlays CLI - Apply overlays to OpenAPI documents");

        // Create the apply command
        var applyCommand = CreateApplyCommand();
        rootCommand.AddCommand(applyCommand);

        // Set up cancellation token for Ctrl+C
        using var cancellationTokenSource = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        // Parse and invoke the command
        return await rootCommand.InvokeAsync(args);
    }

    private static Command CreateApplyCommand()
    {
        var applyCommand = new Command("apply", "Apply one or more overlays to an OpenAPI document");

        // Input description argument
        var inputArgument = new Argument<string>(
            name: "input",
            description: "Path to the input OpenAPI document");

        // Overlay option (can accept multiple values)
        var overlayOption = new Option<string[]>(
            name: "--overlay",
            description: "Path to overlay file(s). Can be specified multiple times.")
        {
            AllowMultipleArgumentsPerToken = false,
            Arity = ArgumentArity.OneOrMore
        };
        overlayOption.AddAlias("-o");

        // Output path option
        var outputOption = new Option<string>(
            name: "--output",
            description: "Path for the output file")
        {
            IsRequired = true
        };
        outputOption.AddAlias("-out");

        // Add arguments and options to the command
        applyCommand.AddArgument(inputArgument);
        applyCommand.AddOption(overlayOption);
        applyCommand.AddOption(outputOption);

        // Set the handler for the apply command
        applyCommand.SetHandler(async (input, overlays, output, cancellationToken) =>
        {
            await HandleApplyCommand(input, overlays, output, cancellationToken);
        }, inputArgument, overlayOption, outputOption);

        return applyCommand;
    }

    private static async Task HandleApplyCommand(
        string input,
        string[] overlays,
        string output,
        CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine($"Applying overlays to OpenAPI document...");
            Console.WriteLine($"Input: {input}");
            Console.WriteLine($"Overlays: {string.Join(", ", overlays)}");
            Console.WriteLine($"Output: {output}");

            // Validate input file exists
            if (!File.Exists(input))
            {
                Console.Error.WriteLine($"Error: Input file '{input}' does not exist.");
                Environment.Exit(1);
                return;
            }

            // Validate overlay files exist
            foreach (var overlay in overlays)
            {
                if (!File.Exists(overlay))
                {
                    Console.Error.WriteLine($"Error: Overlay file '{overlay}' does not exist.");
                    Environment.Exit(1);
                    return;
                }
            }

            // Ensure output directory exists
            var outputDirectory = Path.GetDirectoryName(output);
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // Check for cancellation
            cancellationToken.ThrowIfCancellationRequested();

            // TODO: Implement the actual overlay application logic here
            // For now, this is a placeholder implementation
            await ApplyOverlaysAsync(input, overlays, output, cancellationToken);

            Console.WriteLine("✅ Overlays applied successfully!");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\n❌ Operation was cancelled.");
            Environment.Exit(130); // Standard exit code for Ctrl+C
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"❌ Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static async Task ApplyOverlaysAsync(
        string inputPath,
        string[] overlayPaths,
        string outputPath,
        CancellationToken cancellationToken)
    {
        // TODO: Implement the actual overlay logic using the BinkyLabs.OpenApi.Overlays library
        // This is a placeholder implementation that demonstrates async operation with cancellation support

        Console.WriteLine("🔄 Processing input document...");
        await Task.Delay(500, cancellationToken); // Simulate async work

        foreach (var overlayPath in overlayPaths)
        {
            Console.WriteLine($"🔄 Applying overlay: {Path.GetFileName(overlayPath)}...");
            await Task.Delay(300, cancellationToken); // Simulate async work
            
            // Check for cancellation between operations
            cancellationToken.ThrowIfCancellationRequested();
        }

        Console.WriteLine("🔄 Writing output document...");
        await Task.Delay(200, cancellationToken); // Simulate async work

        // For now, just copy the input to output as a placeholder
        await File.WriteAllTextAsync(outputPath, 
            $"# Processed OpenAPI Document\n# Input: {inputPath}\n# Overlays: {string.Join(", ", overlayPaths)}\n# Processed at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC",
            cancellationToken);
    }
}
