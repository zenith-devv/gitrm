from dataclasses import dataclass, field

@dataclass
class BuildSection:
    project_name: str = "untitled"
    main_file: str = ""
    flags: str = ""
    output_path: str = ""

@dataclass
class CustomScriptSection:
    language: str = ""
    script: str = ""
    override: bool = False

@dataclass
class GitrmYaml:
    build: BuildSection = field(default_factory=BuildSection)
    deps: list[str] = field(default_factory=list)
    custom_script: CustomScriptSection = field(default_factory=CustomScriptSection)