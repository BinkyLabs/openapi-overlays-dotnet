using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinkyLabs.OpenApi.Overlays;
using BinkyLabs.OpenApi.Overlays.Reader;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Cli;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        // Create the root command
        var rootCommand = new RootCommand("BinkyLabs OpenAPI Overlays CLI - Apply overlays to OpenAPI documents");

        // Create the apply command
        var applyCommand = CreateApplyCommand();
        rootCommand.Add(applyCommand);

        // Parse and invoke the command
        return await rootCommand.InvokeAsync(args);
    }

    private static Command CreateApplyCommand()
    {
        var applyCommand = new Command("apply", "Apply one or more overlays to an OpenAPI document");

        // Input description argument
        var inputArgument = new Argument<string>("input", "Path to the input OpenAPI document");

        // Overlay option (can accept multiple values)
        var overlayOption = new Option<string[]>("--overlay", "Path to overlay file(s). Can be specified multiple times.");
        overlayOption.AddAlias("-o");
        overlayOption.Arity = ArgumentArity.ZeroOrMore;
        overlayOption.IsRequired = true;

        // Output path option
        var outputOption = new Option<string>("--output", "Path for the output file");
        outputOption.AddAlias("-out");
        outputOption.IsRequired = true;

        // Add arguments and options to the command
        applyCommand.Add(inputArgument);
        applyCommand.Add(overlayOption);
        applyCommand.Add(outputOption);

        // Set the handler for the apply command
        applyCommand.SetHandler(async (context) =>
        {
            var input = context.ParseResult.GetValueForArgument(inputArgument);
            var overlays = context.ParseResult.GetValueForOption(overlayOption);
            var output = context.ParseResult.GetValueForOption(outputOption);
            var cancellationToken = context.GetCancellationToken();

            if (string.IsNullOrEmpty(input))
            {
                await Console.Error.WriteLineAsync("Error: Input argument is required.");
                context.ExitCode = 1;
                return;
            }

            if (string.IsNullOrEmpty(output))
            {
                await Console.Error.WriteLineAsync("Error: Output option is required.");
                context.ExitCode = 1;
                return;
            }

            await HandleApplyCommand(input, overlays ?? [], output, cancellationToken);
        });

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
                await Console.Error.WriteLineAsync($"Error: Input file '{input}' does not exist.");
                Environment.Exit(1);
                return;
            }

            // Validate overlay files exist
            var missingOverlays = overlays.Where(overlay => !File.Exists(overlay)).ToArray();
            if (missingOverlays.Length > 0)
            {
                foreach (var overlay in missingOverlays)
                {
                    await Console.Error.WriteLineAsync($"Error: Overlay file '{overlay}' does not exist.");
                }
                Environment.Exit(1);
                return;
            }

            // Ensure output directory exists
            var outputDirectory = Path.GetDirectoryName(output);
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // Check for cancellation
            cancellationToken.ThrowIfCancellationRequested();

            // Apply overlays using the BinkyLabs.OpenApi.Overlays library
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
            await Console.Error.WriteLineAsync($"❌ Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static async Task ApplyOverlaysAsync(
        string inputPath,
        string[] overlayPaths,
        string outputPath,
        CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine("🔄 Processing input document...");
            
            // Create reader settings (YAML reader is included by default)
            var readerSettings = new OverlayReaderSettings();
            readerSettings.AddJsonReader();

            // Apply overlays sequentially
            var allDiagnostics = new List<OverlayDiagnostic>();
            var overlayDocuments = new List<OverlayDocument>();

            foreach (var overlayPath in overlayPaths)
            {
                Console.WriteLine($"🔄 Loading overlay: {Path.GetFileName(overlayPath)}...");

                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();

                // Load the overlay document from file
                using var overlayStream = new FileStream(overlayPath, FileMode.Open, FileAccess.Read);
                var (overlayDocument, overlayDiagnostic) = await OverlayDocument.LoadFromStreamAsync(overlayStream, null, readerSettings, cancellationToken);

                if (overlayDocument == null)
                {
                    throw new InvalidOperationException($"Failed to load overlay: {overlayPath}. Errors: {string.Join(", ", overlayDiagnostic?.Errors.Select(e => e.Message) ?? Array.Empty<string>())}");
                }

                overlayDocuments.Add(overlayDocument);

                if (overlayDiagnostic != null)
                {
                    allDiagnostics.Add(overlayDiagnostic);
                }
            }

            var combinedOverlay = overlayDocuments.Count switch {
                0 => throw new InvalidOperationException("No overlays to apply."),
                1 => overlayDocuments[0],
                _ => overlayDocuments[0].CombineWith([.. overlayDocuments[1..]]),
            };

            var (openApiDocument, applyOverlayDiagnostic, _) = await combinedOverlay.ApplyToDocumentAsync(inputPath, cancellationToken: cancellationToken);
            allDiagnostics.Add(applyOverlayDiagnostic);

            // Write final result to output file
            Console.WriteLine("🔄 Writing output document...");

            using var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            using var textWriter = new StreamWriter(outputStream);
            using var writer = format switch
            {
                "yaml" => new OpenApiYamlWriter(textWriter),
                _ => new OpenApiJsonWriter(textWriter),
            };
            
            await openApiDocument.SerializeAsync(writer, cancellationToken);

            // Report any warnings
            var allWarnings = allDiagnostics.SelectMany(d => d.Warnings).ToList();
            if (allWarnings.Count > 0)
            {
                Console.WriteLine($"⚠️ Warnings during processing:");
                foreach (var warning in allWarnings)
                {
                    Console.WriteLine($"  - {warning.Message}");
                }
            }
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            throw new InvalidOperationException($"Failed to apply overlays: {ex.Message}", ex);
        }
    }
}
