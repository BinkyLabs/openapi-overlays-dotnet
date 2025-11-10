using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using BinkyLabs.OpenApi.Overlays.Reader;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using Microsoft.OpenApi.YamlReader;

using SharpYaml.Serialization;

namespace BinkyLabs.OpenApi.Overlays.Cli;

internal static class OverlayCliApp
{
    public static async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        var rootCommand = new RootCommand("BinkyLabs OpenAPI Overlays CLI - Apply overlays to OpenAPI documents");
        var applyCommand = CreateApplyCommand("apply", "Apply one or more overlays to an OpenAPI document", ApplyOverlaysAsync);
        rootCommand.Add(applyCommand);
        var applyAndNormalizeCommand = CreateApplyCommand("apply-and-normalize", "Apply one or more overlays to an OpenAPI document, and normalize the output with OpenAPI.net", ApplyOverlaysAndNormalizeAsync);
        rootCommand.Add(applyAndNormalizeCommand);
        return await rootCommand.Parse(args).InvokeAsync(cancellationToken: cancellationToken);
    }

    private static Argument<string> CreateInputArgument()
    {
        var inputArgument = new Argument<string>("input") { Description = "Path to the input OpenAPI document" };
        return inputArgument;
    }

    private static Option<string[]> CreateOverlayOption()
    {
        var overlayOption = new Option<string[]>("--overlay") { Description = "Path to overlay file(s). Can be specified multiple times." };
        overlayOption.Aliases.Add("-o");
        overlayOption.Arity = ArgumentArity.OneOrMore;
        overlayOption.Required = true;
        return overlayOption;
    }

    private static Option<string> CreateOutputOption()
    {
        var outputOption = new Option<string>("--output") { Description = "Path for the output file" };
        outputOption.Aliases.Add("-out");
        outputOption.Required = true;
        return outputOption;
    }

    private static Option<bool> CreateForceOption()
    {
        var forceOption = new Option<bool>("--force") { Description = "Overwrite output file without confirmation" };
        forceOption.Aliases.Add("-f");
        return forceOption;
    }

    private static Command CreateApplyCommand(string name, string description, Func<string, string[], string, CancellationToken, Task> applyAsync)
    {
        var applyCommand = new Command(name, description);

        var inputArgument = CreateInputArgument();

        var overlayOption = CreateOverlayOption();

        var outputOption = CreateOutputOption();

        var forceOption = CreateForceOption();

        applyCommand.Add(inputArgument);
        applyCommand.Add(overlayOption);
        applyCommand.Add(outputOption);
        applyCommand.Add(forceOption);

        applyCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            var input = parseResult.GetValue(inputArgument);
            var overlays = parseResult.GetValue(overlayOption);
            var output = parseResult.GetValue(outputOption);
            var force = parseResult.GetValue(forceOption);

            if (string.IsNullOrEmpty(input))
            {
                await Console.Error.WriteLineAsync("Error: Input argument is required.");
                return 1;
            }

            if (string.IsNullOrEmpty(output))
            {
                await Console.Error.WriteLineAsync("Error: Output option is required.");
                return 1;
            }

            await HandleCommandAsync(input, overlays ?? [], output, force, applyAsync, cancellationToken);
            return 0;
        });

        return applyCommand;
    }

    private static async Task HandleCommandAsync(
        string input,
        string[] overlays,
        string output,
        bool force,
        Func<string, string[], string, CancellationToken, Task> applyAsync,
        CancellationToken cancellationToken)
    {
        try
        {
            await Console.Out.WriteLineAsync($"Applying overlays to OpenAPI document...");
            await Console.Out.WriteLineAsync($"Input: {input}");
            await Console.Out.WriteLineAsync($"Overlays: {string.Join(", ", overlays)}");
            await Console.Out.WriteLineAsync($"Output: {output}");

            if (!File.Exists(input))
            {
                await Console.Error.WriteLineAsync($"Error: Input file '{input}' does not exist.");
                Environment.Exit(1);
                return;
            }

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

            var outputDirectory = Path.GetDirectoryName(output);
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // Check if output file exists and ask for confirmation if not forced
            if (File.Exists(output) && !force)
            {
                await Console.Out.WriteAsync($"Output file '{output}' already exists. Overwrite? (y/n): ");
                var response = Console.ReadLine();
                if (response?.Trim().ToLowerInvariant() != "y")
                {
                    await Console.Out.WriteLineAsync("Operation cancelled by user.");
                    Environment.Exit(0);
                    return;
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            await applyAsync(input, overlays, output, cancellationToken).ConfigureAwait(false);

            Console.WriteLine("Overlays applied successfully!");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was cancelled.");
            Environment.Exit(130);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static async Task<(OverlayDocument CombinedOverlay, List<OverlayDiagnostic> AllDiagnostics)> LoadAndCombineOverlaysAsync(
        string[] overlayPaths,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Processing input document...");

        var allDiagnostics = new List<OverlayDiagnostic>();
        var overlayDocuments = new List<OverlayDocument>();

        foreach (var overlayPath in overlayPaths)
        {
            Console.WriteLine($"Loading overlay: {Path.GetFileName(overlayPath)}...");
            cancellationToken.ThrowIfCancellationRequested();

            using var overlayStream = new FileStream(overlayPath, FileMode.Open, FileAccess.Read);

            var (overlayDocument, overlayDiagnostic) = await OverlayDocument.LoadFromStreamAsync(overlayStream, cancellationToken: cancellationToken);

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

        var combinedOverlay = overlayDocuments.Count switch
        {
            0 => throw new InvalidOperationException("No overlays to apply."),
            1 => overlayDocuments[0],
            _ => overlayDocuments[0].CombineWith([.. overlayDocuments[1..]]),
        };

        return (combinedOverlay, allDiagnostics);
    }

    private static void DisplayWarnings(List<OverlayDiagnostic> allDiagnostics)
    {
        var allWarnings = allDiagnostics.SelectMany(static d => d.Warnings).ToArray();
        if (allWarnings.Length > 0)
        {
            Console.WriteLine($"Warnings during processing:");
            foreach (var warning in allWarnings)
            {
                Console.WriteLine($"  - {warning.Message}");
            }
        }
    }

    private static void CheckDiagnosticsAndThrowIfErrors(
        OpenApiDiagnostic? openApiDocumentDiagnostic,
        OverlayDiagnostic applyOverlayDiagnostic)
    {
        if (openApiDocumentDiagnostic is { Errors.Count: > 0 })
        {
            var errorMessages = string.Join(", ", openApiDocumentDiagnostic.Errors.Select(static e => e.Message));
            throw new InvalidOperationException($"Failed to apply overlays. Errors: {errorMessages}");
        }
        else if (applyOverlayDiagnostic is { Errors.Count: > 0 })
        {
            var errorMessages = string.Join(", ", applyOverlayDiagnostic.Errors.Select(static e => e.Message));
            throw new InvalidOperationException($"Failed to apply overlays. Errors: {errorMessages}");
        }
    }

    private static async Task ApplyOverlaysAndNormalizeAsync(
        string inputPath,
        string[] overlayPaths,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (combinedOverlay, allDiagnostics) = await LoadAndCombineOverlaysAsync(overlayPaths, cancellationToken);

            var (openApiDocument, applyOverlayDiagnostic, openApiDocumentDiagnostic, _) = await combinedOverlay.ApplyToDocumentAndLoadAsync(inputPath, cancellationToken: cancellationToken);
            allDiagnostics.Add(applyOverlayDiagnostic);

            if (openApiDocument is null)
            {
                CheckDiagnosticsAndThrowIfErrors(openApiDocumentDiagnostic, applyOverlayDiagnostic);
                throw new InvalidOperationException("OpenApiDocument is null after applying overlays.");
            }

            Console.WriteLine("Writing output document...");

            using var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);

            await openApiDocument.SerializeAsync(outputStream, openApiDocumentDiagnostic?.SpecificationVersion ?? OpenApiSpecVersion.OpenApi3_1, openApiDocumentDiagnostic?.Format ?? OpenApiConstants.Json, cancellationToken);

            DisplayWarnings(allDiagnostics);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidOperationException($"Failed to apply overlays: {ex.Message}", ex);
        }
    }

    private static async Task ApplyOverlaysAsync(
        string inputPath,
        string[] overlayPaths,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (combinedOverlay, allDiagnostics) = await LoadAndCombineOverlaysAsync(overlayPaths, cancellationToken);

            var (jsonNode, applyOverlayDiagnostic, openApiDocumentDiagnostic, _) = await combinedOverlay.ApplyToDocumentAsync(inputPath, cancellationToken: cancellationToken);
            allDiagnostics.Add(applyOverlayDiagnostic);

            if (jsonNode is null)
            {
                CheckDiagnosticsAndThrowIfErrors(openApiDocumentDiagnostic, applyOverlayDiagnostic);
                throw new InvalidOperationException("JsonNode is null after applying overlays.");
            }

            Console.WriteLine("Writing output document...");

            using var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);

            switch (openApiDocumentDiagnostic?.Format)
            {
                case "yml":
                case "yaml":
                    var yamlStream = new YamlStream(new YamlDocument(jsonNode.ToYamlNode()));
                    var writer = new StreamWriter(outputStream);
                    yamlStream.Save(writer, true);
                    await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
                    break;
                case "json":
                    await JsonSerializer.SerializeAsync(outputStream, jsonNode, cancellationToken: cancellationToken).ConfigureAwait(false);
                    break;
                default:
                    throw new NotImplementedException($"'{openApiDocumentDiagnostic?.Format}' output format is not yet implemented.");
            }

            await outputStream.FlushAsync(cancellationToken).ConfigureAwait(false);

            DisplayWarnings(allDiagnostics);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidOperationException($"Failed to apply overlays: {ex.Message}", ex);
        }
    }
}