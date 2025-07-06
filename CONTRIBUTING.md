# Contributing to BinkyLabs.OpenAPI.Overlay

Thanks for your interest in contributing to BinkyLabs.OpenAPI.Overlay! We welcome contributions from everyone, regardless of skill level or experience. Here are some guidelines to help you get started:

## Getting Started

To get started, you'll need to have the following tools installed:

- [.NET SDK 9.0](https://get.dot.net/)

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
    - Add an entry to the `CHANGELOG.md` file describing your changes if appropriate.
4. Push your changes to your fork: `git push origin my-new-feature`
5. Create a pull request from your fork to the main repository. `gh pr create` (with the GitHub CLI)

## Reporting Bugs

If you find a bug in this library, please report it by opening a new issue in the issue tracker. Please include as much detail as possible, including steps to reproduce the bug and any relevant error messages.

## License

This project welcomes contributions and suggestions. This project is under the MIT license and so will be your contributions.
