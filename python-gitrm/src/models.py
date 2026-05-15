from dataclasses import dataclass, field

@dataclass
class BuildSection:
    main_file: str = ""
    flags: str = ""
    output_path: str = ""

@dataclass
class ProjectConfig:
    build: BuildSection = field(default_factory=BuildSection)
    deps: list[str] = field(default_factory=list)