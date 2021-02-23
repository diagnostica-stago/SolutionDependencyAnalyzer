using Microsoft.VisualStudio.TestTools.UnitTesting;
using SolutionDependencyAnalyzer;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SolutionDependencyAnalyzerTests
{
    [TestClass]
    public class DependencyAnalyzerTest
    {
        /// <summary>
        /// Very basic test, to ensure DependencyAnalyzer is not completely broken
        /// It will be hard enough to maintain as is, so I didn't test the contents of the various properties
        /// </summary>
        [TestMethod]
        public async Task BasicSolutionAnalysis()
        {
            var currentDir = AppContext.BaseDirectory;
            var goUp = string.Format("..{0}..{0}..{0}..{0}", Path.DirectorySeparatorChar);
            var solutionDir = Path.GetFullPath(Path.Combine(currentDir, goUp));
            var solution = Path.Combine(solutionDir, "SolutionDependencyAnalyzer.sln");
            var analyzer = new DependencyAnalyzer(solution);
            await analyzer.AnalyzeAsync();
            Assert.IsTrue(analyzer.PackageResults.Count == 8);
            Assert.IsTrue(analyzer.PackagesByProject.Count == 2);
            Assert.IsTrue(analyzer.ProjectResults.Count == 2);
            Assert.IsTrue(analyzer.ProjectsByPackage.Count == 8);
        }
    }
}
