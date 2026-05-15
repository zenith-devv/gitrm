import os
import sys
import subprocess
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
    
    def check_mainfile(self, main_file):
        logger.info(f"Looking for {main_file}...")
        if not os.path.exists(main_file):
            logger.error(f"Could not find {main_file}")
            sys.exit(1)