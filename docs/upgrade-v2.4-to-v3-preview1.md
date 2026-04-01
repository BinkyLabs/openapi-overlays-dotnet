# Upgrade Guide: v2.x → v3-preview1

This guide covers the breaking changes and new experimental APIs introduced in v3-preview1. It is intended for users upgrading from v2.4.0 (the last stable v2 release).

## Table of Contents

- [Breaking Changes](#breaking-changes)
  - [`OverlayDocument.Actions` now returns `IList<IOverlayAction>`](#overlaydocumentactions-now-returns-ilistioverlayaction)
- [New APIs](#new-apis)
  - [`IOverlayAction` interface](#ioverlayaction-interface)
  - [Reusable Actions (Experimental — BOO002)](#reusable-actions-experimental--boo002)

---

## Breaking Changes

### `OverlayDocument.Actions` now returns `IList<IOverlayAction>`

**Before (v2.x):**

```csharp
IList<OverlayAction>? actions = overlay.Actions;

foreach (OverlayAction action in overlay.Actions!)
{
    string? target = action.Target;
}
```

**After (v3-preview1):**

```csharp
IList<IOverlayAction>? actions = overlay.Actions;

foreach (IOverlayAction action in overlay.Actions!)
{
    string? target = action.Target;
}
```

**Why this changed:** `OverlayDocument.Actions` can now contain more than one concrete action type. In addition to `OverlayAction`, actions lists may include experimental `OverlayReusableActionReference` items when working with the Overlay 1.2 reusable-action feature. Returning the `IOverlayAction` interface makes the list polymorphic while keeping access to all common action properties.

**Migration steps:**

1. Update variable declarations that reference `IList<OverlayAction>` to `IList<IOverlayAction>`.
2. Replace loop variables typed as `OverlayAction` with `IOverlayAction`.
3. If you need concrete `OverlayAction`-specific behaviour, use a pattern-matching cast:

```csharp
foreach (IOverlayAction action in overlay.Actions!)
{
    if (action is OverlayAction concreteAction)
    {
        // access OverlayAction-only members, e.g. for customization
    }

    // use IOverlayAction members directly for all action types
    Console.WriteLine($"Target: {action.Target}");
}
```

4. If you build the `Actions` list yourself and assign it to the document, change the collection type:

```csharp
// Before
overlay.Actions = new List<OverlayAction>
{
    new OverlayAction { Target = "$.info.title", Update = JsonNode.Parse("\"My API\"") }
};

// After
overlay.Actions = new List<IOverlayAction>
{
    new OverlayAction { Target = "$.info.title", Update = JsonNode.Parse("\"My API\"") }
};
```

---

## New APIs

### `IOverlayAction` interface

`IOverlayAction` is a new public interface that describes every action type. It exposes all properties common to overlay actions, which means you can write code against the interface without needing to know the concrete type at compile time.

| Member | Description |
|---|---|
| `Target` | JSON Path expression selecting the nodes to act on (required) |
| `Description` | Human-readable description of what the action does |
| `Remove` | When `true`, the selected node(s) are removed |
| `Update` | JSON value merged into the selected node(s) |
| `Copy` | JSON Pointer to which the selected node(s) are copied |

`OverlayAction` implements `IOverlayAction` and its behaviour is unchanged. Existing serialization and application semantics are not affected.

---

### Reusable Actions (Experimental — BOO002)

> **Experimental feature.** These APIs require opting in to the `BOO002` diagnostic.
> Add `#pragma warning disable BOO002` (or the `[Experimental("BOO002")]` attribute on your own types) to suppress the compiler warning, and understand that the API shape may still change before the final v3 release.

Reusable actions implement the [OpenAPI Overlay 1.2 Components Object](https://spec.openapis.org/overlay/v1.2.0.html#components-object) and allow you to define parameterized actions once and reference them multiple times across an overlay document.

#### New types

| Type | Description |
|---|---|
| `OverlayComponents` | Container object that holds the named reusable actions map |
| `OverlayReusableAction` | A named, parameterizable action stored under `components/actions` |
| `OverlayReusableActionParameter` | Declares a parameter (with optional default) for a reusable action |
| `OverlayReusableActionReference` | An item in `OverlayDocument.Actions` that references a reusable action by name and supplies concrete parameter values |

#### Enabling the feature

Because these APIs are marked `[Experimental("BOO002")]`, the compiler will produce a warning unless you suppress it:

```csharp
#pragma warning disable BOO002
// ... your code using reusable actions
#pragma warning restore BOO002
```

Or add the suppression to your project file to enable it project-wide:

```xml
<PropertyGroup>
  <NoWarn>$(NoWarn);BOO002</NoWarn>
</PropertyGroup>
```

#### Defining reusable actions

Use `OverlayComponents` to build a reusable actions map:

> **Note:** Attaching `OverlayComponents` to `OverlayDocument` (via a `Components` property) is not yet available in v3-preview1 — it will be wired up in a subsequent preview. The snippet below shows the object construction; the assignment to the document will be documented once that property ships.

```csharp
#pragma warning disable BOO002

using BinkyLabs.OpenApi.Overlays;
using System.Text.Json.Nodes;

// Define a reusable action that sets a server URL using a parameter
var setServerAction = new OverlayReusableAction
{
    Target = "$.servers[0]",
    Update = JsonNode.Parse("""{"url": "{{serverUrl}}"}"""),
    Parameters =
    [
        new OverlayReusableActionParameter { Name = "serverUrl", Default = JsonValue.Create("https://api.example.com") }
    ]
};

var components = new OverlayComponents
{
    Actions = new Dictionary<string, OverlayReusableAction>
    {
        ["setServer"] = setServerAction
    }
};

// overlay.Components = components; // coming in a later preview

#pragma warning restore BOO002
```

#### Referencing reusable actions

Add `OverlayReusableActionReference` items to `OverlayDocument.Actions` to invoke a reusable action:

```csharp
#pragma warning disable BOO002

overlay.Actions =
[
    // Inline action — unchanged from v2
    new OverlayAction
    {
        Target = "$.info.title",
        Update = JsonNode.Parse("\"Production API\"")
    },

    // Reference to the reusable action, with a concrete parameter value
    new OverlayReusableActionReference
    {
        Id = "setServer",
        ParametersValue = new Dictionary<string, JsonNode>
        {
            ["serverUrl"] = JsonValue.Create("https://prod.example.com")!
        }
    }
];

#pragma warning restore BOO002
```

The resolved `Reference` property on `OverlayReusableActionReference` returns the canonical pointer `#/components/actions/{Id}`, which the library uses internally when serializing to YAML/JSON.

#### Reusable action with environment variables

A reusable action can also declare environment-variable bindings via `EnvironmentVariables`:

```csharp
#pragma warning disable BOO002

var deployAction = new OverlayReusableAction
{
    Target = "$.info",
    Update = JsonNode.Parse("""{"x-deploy-region": "{{region}}"}"""),
    Parameters =
    [
        new OverlayReusableActionParameter { Name = "region" }
    ],
    EnvironmentVariables =
    [
        new OverlayReusableActionParameter { Name = "DEPLOY_REGION", Default = JsonValue.Create("us-east-1") }
    ]
};

#pragma warning restore BOO002
```

---

## Summary of Changes

| Change | Type | Action required |
|---|---|---|
| `OverlayDocument.Actions` type changed to `IList<IOverlayAction>` | **Breaking** | Update variable types, loop-variable types, and list construction |
| `IOverlayAction` interface added | Additive | No action needed; `OverlayAction` still works as before |
| `OverlayComponents` added | Experimental (BOO002) | Opt in with `#pragma warning disable BOO002` |
| `OverlayReusableAction` added | Experimental (BOO002) | Opt in with `#pragma warning disable BOO002` |
| `OverlayReusableActionParameter` added | Experimental (BOO002) | Opt in with `#pragma warning disable BOO002` |
| `OverlayReusableActionReference` added | Experimental (BOO002) | Opt in with `#pragma warning disable BOO002` |
