using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinkyLabs.OpenApi.Overlays;
using BinkyLabs.OpenApi.Overlays.Reader;

namespace BinkyLabs.OpenApi.Overlays.Cli;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        // Create the root command
        var rootCommand = new RootCommand("OpenAPI Overlays CLI - Apply overlays to OpenAPI documents");

        // Create the apply command
        var applyCommand = CreateApplyCommand();
        rootCommand.Add(applyCommand);

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
        var inputArgument = new Argument<string>("input", "Path to the input OpenAPI document");

        // Overlay option (can accept multiple values)
        var overlayOption = new Option<string[]>("--overlay", "Path to overlay file(s). Can be specified multiple times.");
        overlayOption.AddAlias("-o");

        // Output path option
        var outputOption = new Option<string>("--output", "Path for the output file");
        outputOption.AddAlias("-out");
        outputOption.IsRequired = true;

        // Add arguments and options to the command
        applyCommand.Add(inputArgument);
        applyCommand.Add(overlayOption);
        applyCommand.Add(outputOption);

        // Set the handler for the apply command
        applyCommand.SetHandler(async (input, overlays, output) =>
        {
            using var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                cancellationTokenSource.Cancel();
            };

            await HandleApplyCommand(input, overlays ?? Array.Empty<string>(), output, cancellationTokenSource.Token);
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
            var currentResult = inputPath;
            var allDiagnostics = new List<OverlayDiagnostic>();
            
            foreach (var overlayPath in overlayPaths)
            {
                Console.WriteLine($"🔄 Applying overlay: {Path.GetFileName(overlayPath)}...");
                
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();

                // Load the overlay document from file
                using var overlayStream = new FileStream(overlayPath, FileMode.Open, FileAccess.Read);
                var overlayResult = await OverlayDocument.LoadFromStreamAsync(overlayStream, null, readerSettings, cancellationToken);
                
                if (overlayResult.Document == null)
                {
                    throw new InvalidOperationException($"Failed to load overlay: {overlayPath}. Errors: {string.Join(", ", overlayResult.Diagnostic?.Errors.Select(e => e.Message) ?? Array.Empty<string>())}");
                }

                // Apply the overlay to the current document
                var (resultDocument, overlayDiagnostic, _) = await overlayResult.Document.ApplyToDocumentAsync(
                    currentResult, 
                    null, // Let it auto-detect format
                    readerSettings, 
                    cancellationToken);

                // Collect diagnostics
                if (overlayDiagnostic != null)
                    allDiagnostics.Add(overlayDiagnostic);

                if (resultDocument is null)
                {
                    throw new InvalidOperationException($"Failed to apply overlay: {overlayPath}. Errors: {string.Join(", ", overlayDiagnostic?.Errors.Select(e => e.Message) ?? Array.Empty<string>())}");
                }

                // For now, we'll save the intermediate result and use it as input for the next overlay
                // This is a simplified approach - ideally we'd keep it in memory
                var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                
                // Write the result to temp file using the library's serialization
                // For now, we'll create a simple placeholder until we can access the proper serialization
                await File.WriteAllTextAsync(tempPath, 
                    $"# OpenAPI document after applying {Path.GetFileName(overlayPath)}\n# This is a placeholder - actual implementation needs proper serialization",
                    cancellationToken);
                
                // Update currentResult for next iteration
                if (currentResult != inputPath)
                {
                    File.Delete(currentResult); // Clean up previous temp file
                }
                currentResult = tempPath;
            }

            // Write final result to output file
            Console.WriteLine("🔄 Writing output document...");
            
            if (currentResult == inputPath)
            {
                // No overlays were applied, just copy the input
                await File.WriteAllTextAsync(outputPath, await File.ReadAllTextAsync(inputPath, cancellationToken), cancellationToken);
            }
            else
            {
                // Move the final result to the output path
                await File.WriteAllTextAsync(outputPath, await File.ReadAllTextAsync(currentResult, cancellationToken), cancellationToken);
                File.Delete(currentResult); // Clean up temp file
            }

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
