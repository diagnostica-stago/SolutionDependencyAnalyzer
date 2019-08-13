namespace SolutionDependencyAnalyzer
{
    public abstract class AWriter
    {
        public string OutputPath { get; }

        public AWriter(string outputPath)
        {
            OutputPath = outputPath;
        }
    }
}
