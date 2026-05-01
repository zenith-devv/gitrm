# gitrm (Git Repository Manager)

**gitrm** is a minimalist build helper and repository manager for Linux systems. It is inspired by the KISS philosophy and the simplicity of Arch Linux's `makepkg`. It streamlines the process of fetching source code, managing dependencies, and compiling projects without the need to remember complex compiler flags for every different language.

## Features
* **Multi-language support:** Native handling for C#, Rust, Go, Java, and C/C++ (GCC/Clang).
* **Git integration:** The `gitrm fetch <url>` command automatically clones a repository and builds it if a config file is detected.
* **YAML Configuration:** A clean, modern `gitrm.yaml` format for project management.
* **Simplified Dependencies:** A straightforward list of required packages in the `deps` section.
* **Lightweight:** Built with .NET and compiled into a single, high-performance binary.

**The project is still in development and many features are yet to come (for instance automatic project dependencies fetching).**

## How it works
Instead of entering dozens of compiler flags manually, you use a single command: `gitrm build`. 
The program reads `gitrm.yaml`, checks the `mainFile` extension, and automatically selects the appropriate compiler. If the compiler is installed on your system, `gitrm` begins the build process.

### Example `gitrm.yaml` for a .NET project:
```yaml
build:
  mainFile: 'gitrm.csproj'
  flags: '-c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true'
  outputPath: './dist'

deps:
  - yamldotnet
  - tomlyn
```

### Example for a C++ project (CMake):
```yaml
build:
  mainFile: 'CMakeLists.txt'
  flags: '-D CMAKE_BUILD_TYPE=Release'
  outputPath: 'build'

deps:
  - nlohmann-json
  - fmt
```

## Supported languages
* **C#**
* **Rust** (Cargo)
* **C++** (GCC/G++, CMake, Meson)
* **Go**
* **Java** (Maven, Gradle)

## Setup and installation
To compile gitrm and install it into your local binary folder, run the provided build.sh script. It will build the binary and move it to `~/.local/bin/gitrm`. Ensure this directory is in your system's `$PATH`.

