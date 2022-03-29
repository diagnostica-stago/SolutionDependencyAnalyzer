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
            Assert.AreEqual(6, analyzer.PackageResults.Count);
            Assert.AreEqual(2, analyzer.PackagesByProject.Count);
            Assert.AreEqual(2, analyzer.ProjectResults.Count);
            Assert.AreEqual(6, analyzer.ProjectsByPackage.Count);
        }
    }
}
