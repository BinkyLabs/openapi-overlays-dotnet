# Upgrade Guide: v2.x → v3-preview1

This guide covers the breaking changes and new experimental APIs introduced in v3-preview1. It is intended for users upgrading from v2.4.0 (the last stable v2 release).

## Table of Contents

- [Breaking Changes](#breaking-changes)
  - [`OverlayDocument.Actions` now returns `IList<IOverlayAction>`](#overlaydocumentactions-now-returns-ilistioverlayaction)
- [New APIs](#new-apis)
  - [`IOverlayAction` interface](#ioverlayaction-interface)
  - [Reusable Actions (Experimental — BOO002)](#reusable-actions-experimental--boo002)
    - [Resolution helper APIs](#resolution-helper-apis)
    - [Validation and apply-time behavior updates](#validation-and-apply-time-behavior-updates)

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

1. If you build the `Actions` list yourself and assign it to the document, change the collection type:

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

```csharp
#pragma warning disable BOO002

using BinkyLabs.OpenApi.Overlays;
using System.Text.Json.Nodes;

// Define a reusable action that sets a server URL using a parameter
var setServerAction = new OverlayReusableAction
{
    Target = "$.servers[0]",
    Update = JsonNode.Parse("""{"url":"%param.serverUrl%"}"""),
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

overlay.Components = components;

#pragma warning restore BOO002
```

#### Referencing reusable actions

Add `OverlayReusableActionReference` items to `OverlayDocument.Actions` to invoke a reusable action:

```csharp
#pragma warning disable BOO002

var overlay = new OverlayDocument
{
    Components = new OverlayComponents
    {
        Actions = new Dictionary<string, OverlayReusableAction>
        {
            ["setServer"] = new()
            {
                Target = "$.servers[0]",
                Update = JsonNode.Parse("""{"url":"https://prod.example.com"}""")
            }
        }
    }
};

overlay.Actions =
[
    // Inline action — unchanged from v2
    new OverlayAction
    {
        Target = "$.info.title",
        Update = JsonNode.Parse("\"Production API\"")
    },

    // Reference to the reusable action, with a concrete parameter value.
    // Passing the host document enables target action resolution.
    new OverlayReusableActionReference("setServer", overlay)
    {
        Reference = new OverlayReusableActionReferenceItem("setServer", overlay)
        {
            ParameterValues = new Dictionary<string, JsonNode>
            {
                ["serverUrl"] = JsonValue.Create("https://prod.example.com")!
            }
        }
    }
];

#pragma warning restore BOO002
```

`OverlayReusableActionReference.Reference.Reference` returns the canonical pointer `#/components/actions/{id}`.
When a host `OverlayDocument` is provided, `TargetAction` resolves from `overlay.Components.Actions[id]`, and unset interface fields on the reference fall back to values from the resolved target action.

#### Reusable action with environment variables

A reusable action can also declare environment-variable bindings via `EnvironmentVariables`:

```csharp
#pragma warning disable BOO002

var deployAction = new OverlayReusableAction
{
    Target = "$.info",
    Update = JsonNode.Parse("""{"x-deploy-region":"%param.region%"}"""),
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

#### Resolution helper APIs

Two new experimental helper APIs were added to support explicit reusable-action resolution workflows:

| API | Description |
|---|---|
| `OverlayReusableActionReference.ResolveParameterValues()` | Resolves final parameter values for a reusable reference (provided values + defaults), and returns unresolved sets for undefined parameter values and missing required parameter values |
| `OverlayReusableAction.ResolveEnvironmentVariableValues(IDictionary<string, string>)` | Resolves environment variables from plain string inputs into `JsonNode` values (including defaults) and returns the missing-required environment variable set |

Notes:

- Reusable parameter/environment-variable definition names are validated and must match `ALPHA *( ALPHA / DIGIT / "_" )` (`[A-Za-z][A-Za-z0-9_]*`). The same validated names are used in `%param.name%` and `%env.name%` interpolation placeholders.
- Invalid names, duplicate definition names, null definitions, or unresolved target-action resolution preconditions raise `InvalidOperationException`.

#### Validation and apply-time behavior updates

Reusable action handling now includes additional runtime validation and interpolation behavior:

- Unresolved reusable references are now reported as diagnostics during parsing and before serialization.
- `OverlayDocument.ApplyToDocument*` now supports `OverlayReusableActionReference` entries directly by resolving them into concrete `OverlayAction` instances before apply.
- During apply, reusable resolution reads environment variable values from the current process environment.
- String interpolation is supported for `%param.name%` and `%env.name%` placeholders in inherited `target`, `description`, and `copy` values (only when that field is not overridden on the reference).
- `update` interpolation traverses recursively:
  - if a string node is exactly `%param.name%` or `%env.name%`, the referenced `JsonNode` value is injected (type-preserving via clone),
  - otherwise placeholder replacement is applied within the string value.
- Unresolved placeholders generate warnings; unresolved required values/invalid mappings generate errors and prevent action resolution.

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
| `OverlayReusableActionReference.ResolveParameterValues()` added | Experimental (BOO002) | Optional: use for explicit parameter/default resolution and validation diagnostics |
| `OverlayReusableAction.ResolveEnvironmentVariableValues(IDictionary<string, string>)` added | Experimental (BOO002) | Optional: use for explicit env/default resolution from non-structured inputs |
| Unresolved reusable references now emit parse/serialize diagnostics | Behavioral | Ensure each `#/components/actions/{id}` target exists |
| Reusable references now resolve during apply (with process env values) | Behavioral | Ensure required env vars are present in the process when applying |
| `%param.*%` / `%env.*%` interpolation added for reusable references | Behavioral | Use `%param.name%` and `%env.name%` syntax in reusable action values |
