public interface IBuilder
{
    string Name { get; }
    bool CanHandle(string extension);
    bool Detect(string directory) => false;
    void Build(ProjectConfig config);
}