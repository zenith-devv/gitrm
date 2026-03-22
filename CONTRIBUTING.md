# Contributing to bob (build orchestrator binary)

First of all, thank you for considering contributing to bob! This project aims to stay simple (KISS) and efficient, and your help is much appreciated.

## How can I help?

### Testing
The easiest way to help is to use bob on your system (Linux or Windows) and report any bugs or weird behavior in the **Issues** tab.

### Adding New Builders
If you want to add support for a new language (e.g., Go, Java, Zig):
1. Take a look at the existing `IBuilder` interface.
2. Implement your builder logic.
3. Add it to the `Builders` list in `BuildAssistant.cs`.

### Improving Documentation
Found a typo in the README? Want to clarify how `bob-config.json` works? Feel free to submit a PR!

## Development Workflow

1. **Fork the repo** and create your branch from `main`.
2. **Setup:** Ensure you have the .NET SDK installed.
3. **Build & Test:** - Run `dotnet build` to compile the project.
   - Run `dotnet run -- build` to test the build logic on a sample project.
4. **Style:** Keep the code simple. Avoid over-engineering. We value "Basic C#" that works over complex patterns that are hard to maintain.

## Submitting Changes

1. **Commit** your changes with a clear message (e.g., `Add GoBuilder support`).
2. **Push** to your fork.
3. **Open a Pull Request** (PR) and describe what you've changed.
