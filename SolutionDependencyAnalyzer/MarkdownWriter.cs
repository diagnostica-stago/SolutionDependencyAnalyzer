using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SolutionDependencyAnalyzer
{
    public sealed class MarkdownWriter : AWriter
    {
        public MarkdownWriter(string outputPath) : base(outputPath)
        {
        }

        /// <summary>
        /// Lists all provides packages in the packages.md file
        /// </summary>
        /// <param name="packages">The packages to write</param>
        public async Task WritePackages(IDictionary<string, string> packages)
        {
            var fileName = Path.Combine(OutputPath, "packages.md");
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            using (var file = File.CreateText(fileName))
            {
                await file.WriteLineAsync("# Nuget dependencies").ConfigureAwait(false);
                foreach (var package in packages.Select(kvp => kvp.Key + " " + kvp.Value))
                {
                    await file.WriteLineAsync($" - {package}").ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Lists the package dependencies by project in the packagesByProject.md file
        /// </summary>
        /// <param name="packagesByProject">A dictionary in which the key is a project, and the values are its package dependencies</param>
        public async Task WritePackagesDependenciesByProject(IDictionary<string, IList<string>> packagesByProject)
        {
            if (packagesByProject == null)
            {
                throw new ArgumentNullException(nameof(packagesByProject));
            }
            await WriteDependencyFile(packagesByProject, "packagesByProject.md", "Package dependencies by project").ConfigureAwait(false);
        }

        /// <summary>
        /// Lists the project dependencies by package in the projectsByPackage.md file
        /// </summary>
        /// <param name="projectsByPackage">A dictionary in which the key is a package, and the values are its project dependencies</param>
        /// <returns></returns>
        public async Task WriteProjectDependenciesByPackage(IDictionary<string, IList<string>> projectsByPackage)
        {
            if (projectsByPackage == null)
            {
                throw new ArgumentNullException(nameof(projectsByPackage));
            }
            await WriteDependencyFile(projectsByPackage, "projectsByPackage.md", "Project dependencies by package").ConfigureAwait(false);
        }

        private async Task WriteDependencyFile(IDictionary<string, IList<string>> projectsByPackage, string fileName, string title)
        {
            var filePath = Path.Combine(OutputPath, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            using (var file = File.CreateText(filePath))
            {
                await file.WriteLineAsync($"# {title}").ConfigureAwait(false);
                foreach (var kvp in projectsByPackage)
                {
                    await file.WriteLineAsync($"### {kvp.Key}").ConfigureAwait(false);
                    foreach (var project in kvp.Value)
                    {
                        await file.WriteLineAsync($" - {project}").ConfigureAwait(false);
                    }
                }
            }
        }

    }
}
