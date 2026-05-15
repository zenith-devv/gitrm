# Contributing to gitrm

Thank you for your interest in improving gitrm! We follow the KISS (Keep It Simple, Stupid) principle. We value code that is easy to read, maintain, and understand over complex abstractions.

## How to help?

### Bug reporting and testing
As a minimalist tool, stability is key. If you find a project that `gitrm` fails to detect or build correctly, please open an **Issue** with:
* Your `gitrm.yaml` file.
* The error log output.
* Your OS/distro details.

### Adding New Builders
The builder system is now modular. To add a new language:
1. Create a new class in `builders.py` that inherits from `BaseBuilder`.
2. Implement the `run()` method using `self.prepare`, `self.build` and `self.configure` if necessary.
3. Register your new builder in the `get_builder()` factory function.

## Development Workflow

### Project structure
* `main.py`: CLI entry point (Typer)
* `builders.py`: Logic for different languages
* `models.py`: Data structures
* `utils.py`: YAML parsing

## Local testing

To test your changes without compiling:

```bash
python3 src/main.py build
```

## Style guide
* **No over-engineering:** If a problem can be solved with a simple `if` statement, don't use a Design Pattern.
* **Type Hints:** Always use Python type hinting for function arguments and return types.
* **Logging:** Use the `loguru` logger for all outputs. Avoid naked `print()` statements.

## Submitting changes
1. **Fork** the repository.
2. **Create a branch** (e.g., `feature/add-zig-support`).
3. **Commit** with descriptive messages.
4. **Push** and open a **Pull Request**.