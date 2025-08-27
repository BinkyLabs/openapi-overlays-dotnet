using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using BinkyLabs.OpenApi.Overlays.Reader;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Cli;

public class OverlayCliApp
{
    public async Task<int> RunAsync(string[] args)
    {
        var rootCommand = new RootCommand("BinkyLabs OpenAPI Overlays CLI - Apply overlays to OpenAPI documents");
        var applyCommand = CreateApplyCommand();
        rootCommand.Add(applyCommand);
        return await rootCommand.Parse(args).InvokeAsync();
    }

    private Command CreateApplyCommand()
    {
        var applyCommand = new Command("apply", "Apply one or more overlays to an OpenAPI document");

        var inputArgument = new Argument<string>("input")
        {
            Description = "Path to the input OpenAPI document"
        };

        var overlayOption = new Option<string[]>("--overlay") { Description = "Path to overlay file(s). Can be specified multiple times." };
        overlayOption.Aliases.Add("-o");
        overlayOption.Arity = ArgumentArity.OneOrMore;
        overlayOption.Required = true;

        var outputOption = new Option<string>("--output") { Description = "Path for the output file" };
        outputOption.Aliases.Add("-out");
        outputOption.Required = true;

        applyCommand.Add(inputArgument);
        applyCommand.Add(overlayOption);
        applyCommand.Add(outputOption);

        applyCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            var input = parseResult.GetValue(inputArgument);
            var overlays = parseResult.GetValue(overlayOption);
            var output = parseResult.GetValue(outputOption);

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

            await HandleApplyCommand(input, overlays ?? [], output, cancellationToken);
            return 0;
        });

        return applyCommand;
    }

    private async Task HandleApplyCommand(
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

            cancellationToken.ThrowIfCancellationRequested();

            await ApplyOverlaysAsync(input, overlays, output, cancellationToken);

            Console.WriteLine("✅ Overlays applied successfully!");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\n❌ Operation was cancelled.");
            Environment.Exit(130);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"❌ Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private async Task ApplyOverlaysAsync(
        string inputPath,
        string[] overlayPaths,
        string outputPath,
        CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine("🔄 Processing input document...");

            var readerSettings = new OverlayReaderSettings();
            readerSettings.AddJsonReader();

            var allDiagnostics = new List<OverlayDiagnostic>();
            var overlayDocuments = new List<OverlayDocument>();

            foreach (var overlayPath in overlayPaths)
            {
                Console.WriteLine($"🔄 Loading overlay: {Path.GetFileName(overlayPath)}...");
                cancellationToken.ThrowIfCancellationRequested();

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

            var combinedOverlay = overlayDocuments.Count switch
            {
                0 => throw new InvalidOperationException("No overlays to apply."),
                1 => overlayDocuments[0],
                _ => overlayDocuments[0].CombineWith([.. overlayDocuments[1..]]),
            };

            var (openApiDocument, applyOverlayDiagnostic, _) = await combinedOverlay.ApplyToDocumentAsync(inputPath, cancellationToken: cancellationToken);
            allDiagnostics.Add(applyOverlayDiagnostic);

            Console.WriteLine("🔄 Writing output document...");

            using var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            var format = InspectStreamFormat(outputStream);
            using var textWriter = new StreamWriter(outputStream);
            OpenApiWriterBase writer = format switch
            {
                "yaml" => new OpenApiYamlWriter(textWriter),
                _ => new OpenApiJsonWriter(textWriter),
            };

            if (openApiDocument is null)
                throw new InvalidOperationException("OpenApiDocument is null after applying overlays.");

            await openApiDocument.SerializeAsync(
                outputStream,
                OpenApiSpecVersion.OpenApi2_0,
                format,
                cancellationToken
            );
            await openApiDocument.SerializeAsync(outputStream, OpenApiSpecVersion.OpenApi2_0, format, cancellationToken);

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

    private string InspectStreamFormat(Stream stream)
    {
        if (stream is null) throw new ArgumentNullException(nameof(stream));

        long initialPosition = stream.Position;
        int firstByte = stream.ReadByte();

        if (char.IsWhiteSpace((char)firstByte))
        {
            firstByte = stream.ReadByte();
        }

        stream.Position = initialPosition;

        char firstChar = (char)firstByte;
        return firstChar switch
        {
            '{' or '[' => OpenApiConstants.Json,
            _ => OpenApiConstants.Yaml
        };
    }
}