# Contributing to bob (build orchestrator binary)

First of all, thank you for considering contributing to bob! This project aims to stay simple (KISS) and efficient, and your help is much appreciated.

## How can I help?

### Testing
The easiest way to help is to use bob on your system (Linux is recommended) and report any bugs in the **Issues** tab.

### Adding New Builders
If you want to add support for a new language (e.g., Go, Java, Zig):
1. Take a look at the existing `IBuilder` interface.
2. Implement your builder logic.
3. Add it to the `Builders` list in `BuildAssistant.cs`.

## Development Workflow

1. **Fork the repo** and create your branch from `main`.
2. **Setup:** Ensure you have the .NET SDK installed.
3. **Build & Test:**
   - Run `dotnet build` to compile the project.
   - Run `dotnet run -- build` to test the build logic on a sample project.
5. **Style:** Keep the code simple. We prefer readable "Basic C#" over complex design patterns.
6. **Log messages:** Follow the convention of **lowercase** bob logger messages, e.g. `[bob] cargo is present`. 

## Submitting Changes

1. **Commit** your changes with a clear message (e.g., `Add GoBuilder support`).
2. **Push** to your fork.
3. **Open a Pull Request** (PR) and describe what you've changed.
