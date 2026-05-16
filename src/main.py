import os
import sys
import typer
import shutil
import subprocess

from pathlib import Path
from loguru import logger
from utils import load_config, make_config
from builders import get_builder, run_custom_script

__version__ = "v0.7-beta-1-g8f1668b (build 2026.05.16-1933)"

logger.remove()
logger.add(sys.stderr, format="<level>{level: ^S8}</level>| {message}")

app = typer.Typer(
    help="gitrm (git repo manager) - Tool for building and managing projects.",
    add_completion=False,
)

@app.command()
def build():
    """
    Read gitrm.yaml and compile project to executable.
    """
    config = load_config("gitrm.yaml")

    if config.custom_script.override:
        run_custom_script(config)
        return
    
    builder = get_builder(config)
    
    if builder:
        logger.info(f"{builder.name} project detected")
        builder.run(config)
        run_custom_script(config)
    else:
        raise typer.Exit(code=1)

@app.command()
def clone(url: str):
    """
    Clone a repository and automatically build the project (if gitrm.yaml exists)
    """
    repo_name = url.split("/")[-1].replace(".git", "")
    target_dir = Path.cwd() / repo_name

    logger.info(f"Cloning {url} into {repo_name}...")

    try:
        subprocess.run(["git", "clone", url], check=True)
    except subprocess.CalledProcessError:
        logger.error("Failed to clone repository")
        raise typer.Exit(1)

    config_file = target_dir / "gitrm."
    if config_file.exists():
        logger.info(f"Found {config_file}, starting automatic build...")

    current_dir = os.getcwd()
    try:
        os.chdir(target_dir)
        build()
    finally:
        os.chdir(current_dir)
       
@app.command()
def config():
    """
    Create an empty gitrm.yaml template.
    """
    make_config("gitrm.yaml")

@app.command()
def version():
    """
    Display gitrm version.
    """
    typer.echo(f"gitrm (git repo manager) version {__version__}")
    typer.echo("Copyright (c) 2026 Michael Zenith")

if __name__ == "__main__":
    app()
