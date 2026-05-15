import os
import sys
import subprocess
from models import GitrmYaml
from loguru import logger

class BaseBuilder():
    def __init__(self, name):
        self.name = name

    def check_compiler(self, command: list): # e.g. ["dotnet", "--version"]
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
        self.check_compiler(["dotnet", "--version"])
        self.check_mainfile(config.build.main_file)
        cmd = ["dotnet", "publish", config.build.main_file]
    
        if config.build.flags:
            cmd.extend(config.build.flags.split())
        
        if config.build.output_path:
            cmd.extend(["-o", config.build.output_path])

        self.build(cmd, config)

