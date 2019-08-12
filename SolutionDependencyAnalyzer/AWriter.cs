using System;
using System.Collections.Generic;
using System.Text;

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
