import os
import sys
import subprocess
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
        
        logger.error(f"Could not find matching builder for \'{config.build.project_name}\'")
        return None
    else:
        logger.error("main_file field is missing in gitrm.yaml")
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
            cmd.extend(config.build.flags)
        
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
            cmd.extend(config.build.flags)

        self.build(cmd, config)

        
