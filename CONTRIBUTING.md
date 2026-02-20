# Contributing to BinkyLabs.OpenAPI.Overlay

Thanks for your interest in contributing to BinkyLabs.OpenAPI.Overlay! We welcome contributions from everyone, regardless of skill level or experience. Here are some guidelines to help you get started:

## Getting Started

To get started, you'll need to have the following tools installed:

- [.NET SDK 10.0](https://get.dot.net/)

## Recommended tools

- [Visual Studio Code](https://code.visualstudio.com/)
- [reportgenerator](https://www.nuget.org/packages/dotnet-reportgenerator-globaltool), if you want to be able to generate coverage reports from the pre-configured visual studio code test tasks.

## Building the project

```sh
dotnet build
```

## Running the tests

```sh
dotnet test
```

## Contributing Code

1. Fork the repository and clone it to your local machine. `gh repo fork binkylabs/openapi-overlay-dotnet --clone`
2. Create a new branch for your changes: `git checkout -b my-new-feature`
3. Make your changes and commit them: `git commit -am 'Add some feature'`
    - Include tests that cover your changes.
    - Update the documentation to reflect your changes, where appropriate.
4. Push your changes to your fork: `git push origin my-new-feature`
5. Create a pull request from your fork to the main repository. `gh pr create` (with the GitHub CLI)

## Commit message format

To support our automated release process, pull requests are required to follow the [Conventional Commit](https://www.conventionalcommits.org/en/v1.0.0/)
format.
Each commit message consists of a __header__, an optional __body__ and an optional __footer__. The header is the first line of the commit and
MUST have a __type__ (see below for a list of types) and a __description__. An optional __scope__ can be added to the header to give extra context.

```txt
<type>[optional scope]: <short description>
<BLANK LINE>
<optional body>
<BLANK LINE>
<optional footer(s)>
```

The recommended commit types used are:

- __feat__ for feature updates (increments the _minor_ version)
- __fix__ for bug fixes (increments the _patch_ version)
- __perf__ for performance related changes e.g. optimizing an algorithm
- __refactor__ for code refactoring changes
- __test__ for test suite updates e.g. adding a test or fixing a test
- __style__ for changes that don't affect the meaning of code. e.g. formatting changes
- __docs__ for documentation updates e.g. ReadMe update or code documentation updates
- __build__ for build system changes (gradle updates, external dependency updates)
- __ci__ for CI configuration file changes e.g. updating a pipeline
- __chore__ for miscallaneous non-sdk changesin the repo e.g. removing an unused file

Adding an exclamation mark after the commit type (`feat!`) or footer with the prefix __BREAKING CHANGE:__ will cause an increment of the _major_ version.

## Updates to public API surface

Because we need to maintain a compatible public API surface within a major version, this project is using the public API analyzers to ensure no prior public API is changed/removed inadvertently.

This means that:

- All entries in an __Unshipped__ document need to be moved to the __Shipped__ document before any public release.
- All new APIs being added need to be __Unshipped__ document before the pull request can be merged, otherwise build will fail with a message like the example below.

```txt
Error: /home/runner/work/OpenAPI.NET/OpenAPI.NET/src/Microsoft.OpenApi/Models/OpenApiSecurityScheme.cs(39,46): error RS0016: Symbol 'OAuth2MetadataUrl.set' is not part of the declared public API (https://github.com/dotnet/roslyn-analyzers/blob/main/src/PublicApiAnalyzers/PublicApiAnalyzers.Help.md) [/home/runner/work/OpenAPI.NET/OpenAPI.NET/src/Microsoft.OpenApi/Microsoft.OpenApi.csproj::TargetFramework=net8.0]
```

### Update the unshipped document

To update the unshipped document, simply run the following commands

```shell
# add the missing public api entries
dotnet format --diagnostics RS0016
# discard changes to cs files to avoid creating conflicts
git checkout *.cs
```

### Move items from unshipped to unshipped document

```pwsh
. ./scripts/promoteUnshipped.ps1
```

> Note: the promotion of APIs is automated through the dedicated workflow and should result in pull requests being automatically opened.


## Reporting Bugs

If you find a bug in this library, please report it by opening a new issue in the issue tracker. Please include as much detail as possible, including steps to reproduce the bug and any relevant error messages.

## License

This project welcomes contributions and suggestions. This project is under the MIT license and so will be your contributions.
