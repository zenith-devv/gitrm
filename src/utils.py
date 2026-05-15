import os
import sys
import yaml

from loguru import logger
from dataclasses import asdict
from models import GitrmYaml, BuildSection, CustomScriptSection

def make_config(file_path="gitrm.yaml"):
    if not os.path.exists(file_path):
        template = GitrmYaml()
        with open(file_path, 'w', encoding='utf-8') as f:
            yaml.dump(asdict(template), f, sort_keys=False, default_flow_style=False)
        logger.info("Created empty gitrm.yaml template")
    else:
        logger.error("gitrm.yaml already exists. Aborting")
        sys.exit(1)

def load_config(file_path: str) -> GitrmYaml:
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            raw_data = yaml.safe_load(f)
        
        if not raw_data:
            return GitrmYaml()

        build_data = raw_data.get('build', {})
        build_section = BuildSection(**build_data)
        
        script_data = raw_data.get('custom_script', {})
        script_section = CustomScriptSection(**script_data)
        
        return GitrmYaml(
            build=build_section,
            deps=raw_data.get('deps', []),
            custom_script=script_section
        )
        
    except FileNotFoundError:
        logger.error("gitrm.yaml was not found")
        sys.exit(1)
    except TypeError as e:
        logger.error(f"YAML structure is invalid: {e}")
        sys.exit(1)