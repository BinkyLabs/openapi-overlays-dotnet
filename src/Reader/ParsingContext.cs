// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader.V1;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace BinkyLabs.OpenApi.Overlays.Reader;

/// <summary>
/// The Parsing Context holds temporary state needed whilst parsing an OpenAPI Document
/// </summary>
public class ParsingContext
{
    private readonly Stack<string> _currentLocation = new();
    private readonly Dictionary<string, object> _tempStorage = new();
    private readonly Dictionary<object, Dictionary<string, object>> _scopedTempStorage = new();
    private readonly Dictionary<string, Stack<string>> _loopStacks = new();

    /// <summary>
    /// Extension parsers
    /// </summary>
    public Dictionary<string, Func<JsonNode, OverlaySpecVersion, IOpenApiExtension>>? ExtensionParsers { get; set; } = 
        new();

    internal RootNode? RootNode { get; set; }
    /// <summary>
    /// The base url for the document
    /// </summary>
    public Uri? BaseUrl { get; set; }

    /// <summary>
    /// Default content type for a response object
    /// </summary>
    public List<string>? DefaultContentType { get; set; }

    /// <summary>
    /// Diagnostic object that returns metadata about the parsing process.
    /// </summary>
    public OpenApiDiagnostic Diagnostic { get; }

    /// <summary>
    /// Create Parsing Context
    /// </summary>
    /// <param name="diagnostic">Provide instance for diagnostic object for collecting and accessing information about the parsing.</param>
    public ParsingContext(OpenApiDiagnostic diagnostic)
    {
        Diagnostic = diagnostic;
    }
    private const string OverlayV1Version = "1.0.0";

    /// <summary>
    /// Initiates the parsing process.  Not thread safe and should only be called once on a parsing context
    /// </summary>
    /// <param name="jsonNode">Set of Json nodes to parse.</param>
    /// <param name="location">Location of where the document that is getting loaded is saved</param>
    /// <returns>An OverlayDocument populated based on the passed yamlDocument </returns>
    public OverlayDocument Parse(JsonNode jsonNode, Uri location)
    {
        RootNode = new RootNode(this, jsonNode);

        var inputVersion = GetVersion(RootNode);

        OverlayDocument doc;

        switch (inputVersion)
        {
            case string version when OverlayV1Version.Equals(version, StringComparison.OrdinalIgnoreCase):
                VersionService = new OverlayV1VersionService(Diagnostic);
                doc = VersionService.LoadDocument(RootNode, location);
                this.Diagnostic.SpecificationVersion = OverlaySpecVersion.Overlay1_0;
                ValidateRequiredFields(doc, version);
                break;

            default:
                throw new OpenApiUnsupportedSpecVersionException(inputVersion);
        }

        return doc;
    }

    /// <summary>
    /// Initiates the parsing process of a fragment.  Not thread safe and should only be called once on a parsing context
    /// </summary>
    /// <param name="jsonNode"></param>
    /// <param name="version">OpenAPI version of the fragment</param>
    /// <param name="overlayDocument">The OverlayDocument object to which the fragment belongs, used to lookup references.</param>
    /// <returns>An OverlayDocument populated based on the passed yamlDocument </returns>
    public T? ParseFragment<T>(JsonNode jsonNode, OverlaySpecVersion version, OverlayDocument overlayDocument) where T : IOpenApiElement
    {
        var node = ParseNode.Create(this, jsonNode);

        var element = default(T);

        switch (version)
        {
            case OverlaySpecVersion.Overlay1_0:
                VersionService = new OverlayV1VersionService(Diagnostic);
                element = this.VersionService.LoadElement<T>(node, overlayDocument);
                break;
            default:
                throw new OpenApiUnsupportedSpecVersionException(version.ToString());

        }

        return element;
    }

    /// <summary>
    /// Gets the version of the Open API document.
    /// </summary>
    private static string GetVersion(RootNode rootNode)
    {
        var versionNode = rootNode.Find(new("/overlay"));

        if (versionNode is not null)
        {
            return versionNode.GetScalarValue().Replace("\"", string.Empty);
        }

        throw new OpenApiException("Version node not found.");
    }

    /// <summary>
    /// Service providing all Version specific conversion functions
    /// </summary>
    internal IOverlayVersionService? VersionService { get; set; }

    /// <summary>
    /// End the current object.
    /// </summary>
    public void EndObject()
    {
        _currentLocation.Pop();
    }

    /// <summary>
    /// Get the current location as string representing JSON pointer.
    /// </summary>
    public string GetLocation()
    {
        return "#/" + string.Join("/", _currentLocation.Reverse().Select(s => s.Replace("~", "~0").Replace("/", "~1")).ToArray());
    }

    /// <summary>
    /// Gets the value from the temporary storage matching the given key.
    /// </summary>
    public T? GetFromTempStorage<T>(string key, object? scope = null)
    {
        Dictionary<string, object>? storage;

        if (scope == null)
        {
            storage = _tempStorage;
        }
        else if (!_scopedTempStorage.TryGetValue(scope, out storage))
        {
            return default;
        }

        return storage.TryGetValue(key, out var value) ? (T)value : default;
    }

    /// <summary>
    /// Sets the temporary storage for this key and value.
    /// </summary>
    public void SetTempStorage(string key, object? value, object? scope = null)
    {
        Dictionary<string, object>? storage;

        if (scope == null)
        {
            storage = _tempStorage;
        }
        else if (!_scopedTempStorage.TryGetValue(scope, out storage))
        {
            storage = _scopedTempStorage[scope] = new();
        }

        if (value == null)
        {
            storage.Remove(key);
        }
        else
        {
            storage[key] = value;
        }
    }

    /// <summary>
    /// Starts an object with the given object name.
    /// </summary>
    public void StartObject(string objectName)
    {
        _currentLocation.Push(objectName);
    }

    /// <summary>
    /// Maintain history of traversals to avoid stack overflows from cycles
    /// </summary>
    /// <param name="loopId">Any unique identifier for a stack.</param>
    /// <param name="key">Identifier used for current context.</param>
    /// <returns>If method returns false a loop was detected and the key is not added.</returns>
    public bool PushLoop(string loopId, string key)
    {
        if (!_loopStacks.TryGetValue(loopId, out var stack))
        {
            stack = new();
            _loopStacks.Add(loopId, stack);
        }

        if (!stack.Contains(key))
        {
            stack.Push(key);
            return true;
        }
        else
        {
            return false;  // Loop detected
        }
    }

    /// <summary>
    /// Reset loop tracking stack
    /// </summary>
    /// <param name="loopid">Identifier of loop to clear</param>
    internal void ClearLoop(string loopid)
    {
        _loopStacks[loopid].Clear();
    }

    /// <summary>
    /// Exit from the context in cycle detection
    /// </summary>
    /// <param name="loopid">Identifier of loop</param>
    public void PopLoop(string loopid)
    {
        if (_loopStacks[loopid].Count > 0)
        {
            _loopStacks[loopid].Pop();
        }
    }

    private void ValidateRequiredFields(OverlayDocument doc, string version)
    {
        if (OverlayV1Version.Equals(version, StringComparison.OrdinalIgnoreCase) && RootNode is not null)
        {
            if (doc.Actions == null)
                RootNode.Context.Diagnostic.Errors.Add(new OpenApiError("", $"Actions is a REQUIRED field at {RootNode.Context.GetLocation()}"));
            if (doc.Info == null)
                RootNode.Context.Diagnostic.Errors.Add(new OpenApiError("", $"Info is a REQUIRED field at {RootNode.Context.GetLocation()}"));
        }
    }
}
