import os
import sys
import subprocess

from pathlib import Path
from models import GitrmYaml
from loguru import logger

def get_builder(config: GitrmYaml):
    logger.info(f"Detecting builder for {config.build.project_name}...")

    if config.build.main_file:
        main_file = config.build.main_file.lower()

        if main_file.endswith(".sln") or main_file.endswith(".csproj"):
            return DotnetBuilder()
        
        if main_file == "cargo.toml":
            return CargoBuilder()
        
        if main_file == "go.mod":
            return GoBuilder()
        
        if main_file == "pom.xml":
            return MavenBuilder()

        if main_file == "meson.build":
            return MesonBuilder()
        
        if main_file == "cmakelists.txt":
            return CMakeBuilder()
        
        if main_file == "makefile":
            return MakefileBuilder()
        
        logger.error(f"Could not find matching builder for \'{config.build.project_name}\'")
        return None
    else:
        logger.error("main_file field is missing in gitrm.yaml")
        sys.exit(1)

def run_custom_script(config: GitrmYaml):
    custom = config.custom_script
    
    if not custom.script or not custom.language:
        return

    script_path = Path(custom.script)
    if not script_path.exists():
        logger.error(f"Custom script not found: {custom.script}")
        sys.exit(1)

    logger.info(f"Executing custom {custom.language} script: {custom.script}...")
    
    try:
        if custom.language.lower() == 'python':
            subprocess.run([sys.executable, str(script_path)], check=True)
        elif custom.language.lower() in ['bash', 'sh']:
            subprocess.run(["bash", str(script_path)], check=True)
        else:
            subprocess.run([f"./{script_path}"], check=True)
            
        logger.success("Custom script executed successfully")
    except subprocess.CalledProcessError as e:
        logger.error(f"Custom script failed (exit code: {e.returncode})")
        sys.exit(1)
        
class BaseBuilder():
    def __init__(self, name):
        self.name = name
        
    def check_compiler(self, command: list):
        logger.info(f"Looking for {self.name}...")
        result = subprocess.run(command, capture_output=True, check=False)

        if result.returncode != 0:
            logger.error(f"{self.name} is not installed or not in PATH")
            sys.exit(1)

        logger.info(f"{self.name} found")
    
    def check_mainfile(self, main_file):
        logger.info(f"Looking for {main_file}...")

        if not os.path.exists(main_file):
            logger.error(f"Could not find {main_file}")
            sys.exit(1)

        logger.info(f"{main_file} found")

    def prepare(self, config: GitrmYaml, check_cmd: list):
        logger.info(f"Preparing to build \'{config.build.project_name}\'...")
        self.check_compiler(check_cmd)
        self.check_mainfile(config.build.main_file)

    def configure(self, command: list, config: GitrmYaml):
        logger.info(f"Running: {' '.join(command)}")
        conf_result = subprocess.run(command)

        if conf_result.returncode != 0:
            logger.error(f"Configuration failed for \'{config.build.project_name}\' (exit code {conf_result.returncode})")
            sys.exit(1)

    def build(self, command: list, config: GitrmYaml):
        logger.info(f"Running: {' '.join(command)}")
        build_result = subprocess.run(command)

        if build_result.returncode != 0:
            logger.error(f"Build failed for \'{config.build.project_name}\' (exit code {build_result.returncode})")
            sys.exit(1)

        logger.success(f"Building \'{config.build.project_name}\' finished successfully, output located in {config.build.output_path}")

class DotnetBuilder(BaseBuilder):
    def __init__(self):
        super().__init__("Dotnet")

    def run(self, config: GitrmYaml):
        self.prepare(config, ["dotnet", "--version"])
        cmd = ["dotnet", "publish", config.build.main_file]
    
        if config.build.flags:
            cmd.extend(config.build.flags.split())
        
        if config.build.output_path:
            cmd.extend(["-o", config.build.output_path])

        self.build(cmd, config)

class CargoBuilder(BaseBuilder):
    def __init__(self):
        super().__init__("Cargo")
    
    def run(self, config: GitrmYaml):
        self.prepare(config, ["cargo", "--version"])
        cmd = ["cargo", "build"]

        if config.build.output_path:
            cmd.extend(config.build.output_path)

        if config.build.flags:
            cmd.extend(config.build.flags.split())
        
        self.build(cmd, config)

class GoBuilder(BaseBuilder):
    def __init__(self):
        super().__init__("Go")

    def run(self, config: GitrmYaml):
        self.prepare(config, ["go", "version"])
        cmd = ["go", "build"]

        if config.build.output_path:
            cmd.extend(["-o", config.build.output_path])

        if config.build.flags:
            cmd.extend(config.build.flags.split())
        
        cmd.append("./...")

        self.build(cmd, config)

class MavenBuilder(BaseBuilder):
    def __init__(self):
        super().__init__("Maven")
    
    def run(self, config: GitrmYaml):
        main_file = config.build.main_file.lower()
        
        if not main_file.endswith("pom.xml"):
            logger.error("Java projects must use a pom.xml file (Maven). Single .java files are not supported.")
            sys.exit(1)

        self.prepare(config, ["mvn", "--version"])

        cmd = ["mvn", "package", "-f", config.build.main_file]

        if config.build.flags:
            cmd.extend(config.build.flags.split())

        if config.build.output_path:
            logger.warning("output_path is ignored for Maven builds — output is placed in 'target/' by Maven.")

        self.build(cmd, config)

class MesonBuilder(BaseBuilder):
    def __init__(self):
        super().__init__("Meson")

    def run(self, config: GitrmYaml):
        self.prepare(config, ["meson", "--version"])

        build_dir = config.build.output_path or "build"

        if not os.path.exists(os.path.join(build_dir, "build.ninja")):
            setup_cmd = ["meson", "setup", build_dir]
            if config.build.flags:
                setup_cmd.extend(config.build.flags.split())
            self.configure(setup_cmd, config)

        compile_cmd = ["meson", "compile", "-C", build_dir]

        self.build(compile_cmd, config)

class CMakeBuilder(BaseBuilder):
    def __init__(self):
        super().__init__("CMake")

    def run(self, config: GitrmYaml):
        self.prepare(config, ["cmake", "--version"])

        build_dir = config.build.output_path or "build"
        setup_cmd = ["cmake", "-S", ".", "-B", build_dir]
        
        if config.build.flags:
            setup_cmd.extend(config.build.flags.split())
        
        self.configure(setup_cmd, config)

        build_cmd = ["cmake", "--build", build_dir]
        self.build(build_cmd, config)

class MakefileBuilder(BaseBuilder):
    def __init__(self):
        super().__init__("Makefile")

    def run(self, config: GitrmYaml):
        self.prepare(config, ["make", "--version"])

        threads = os.cpu_count() or 1
        cmd = ["make", f"-j{threads}"]

        if os.path.exists("Kconfig") and "localmodconfig" in config.build.flags:
             logger.warning("Kernel project detected. Running localmodconfig...")
             self.configure(["make", "localmodconfig"], config)
        
        if config.build.flags:
            clean_flags = config.build.flags.replace("localmodconfig", "").strip()
            if clean_flags:
                cmd.extend(clean_flags.split())

        if config.build.output_path:
            cmd.append(f"O={config.build.output_path}")

        self.build(cmd, config)
