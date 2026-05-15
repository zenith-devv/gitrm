# gitrm (Git Repository Manager)

**gitrm** is a minimalist build tool and repository manager for power users. 
Stop wasting time remembering compiler flags. One tool, one command, any language.

## Features
* **Multi-language support:** Native handling for C#, Rust, Go, Java, and C/C++ (CMake/Meson/Makefile).
* **Git integration:** The `gitrm fetch <url>` command automatically clones a repository and builds it if a config file is detected.
* **YAML Configuration:** A clean, modern `gitrm.yaml` format for project management.
* **Simplified Dependencies:** A straightforward list of required packages in the `deps` section.
* **Custom scripts:** Use your custom setup scripts instead of basic building process - Python and Bash supported.
* **Native Distribution:** Built with Python and compiled into a native binary using Nuitka for maximum performance.

**The project is still in development and many features are yet to come.**

## How it works
Instead of entering dozens of compiler flags manually, you use a single command: `gitrm build`. 
The program reads `gitrm.yaml`, checks `main_file` and automatically selects the appropriate compiler. If the compiler is installed on your system, `gitrm` begins the build process.

### Example `gitrm.yaml` for a .NET project:
```yaml
build:
  project_name: "example"
  main_file: 'gitrm.csproj'
  flags: '-c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true'
  output_path: './dist'
deps:
  - yamldotnet
  - tomlyn
custom_script:
  language: ''
  script: ''
  override: false
```

### Example for a C++ project (CMake):
```yaml
build:
  main_file: 'CMakeLists.txt'
  flags: '-D CMAKE_BUILD_TYPE=Release'
  output_path: 'build'
deps:
  - nlohmann-json
  - fmt
custom_script:
  language: ''
  script: ''
  override: false
```

### If you want gitrm to compile your project your way:
```yaml
build:
  main_file: ''
  flags: ''
  output_path: ''
deps: []
custom_script:
  language: 'python'
  script: 'setup_gui.py'
  override: true
```

If `override` is false, the script will run after the compilation if you specified the data. If true, this will replace the compile process with the given script.

## Supported languages
* **C#** (Dotnet)
* **Rust** (Cargo)
* **C++** (Makefile, CMake, Meson)
* **Go**
* **Java** (Maven)

## Setup and installation
To compile gitrm and install it into your local binary folder, run the provided build.sh script. It will build the binary and move it to `~/.local/bin/gitrm`. Ensure this directory is in your system's `$PATH`.

## Quick start
* `gitrm config` - Generate a template for your project.
* `gitrm clone` - Clone a repository and automatically build the project (if gitrm.yaml exists)
  * `-k`, `--keep` - Keep the source code after a successful build.
* `gitrm build`  - Detect and compile the current project.
* `gitrm version` - Show current version.