using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

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
            if (packages == null || !packages.Any())
            {
                return;
            }
            var providers = new List<Lazy<INuGetResourceProvider>>();
            providers.AddRange(Repository.Provider.GetCoreV3());
            var packageSource = new PackageSource("https://api.nuget.org/v3/index.json");
            var sourceRepository = new SourceRepository(packageSource, providers);
            var packageSearchResource = await sourceRepository.GetResourceAsync<PackageSearchResource>().ConfigureAwait(false);

            var fileName = Path.Combine(OutputPath, "packages.md");
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            using (var file = File.CreateText(fileName))
            {
                await file.WriteLineAsync("# Nuget dependencies").ConfigureAwait(false);
                foreach (var (packageId, version) in packages.OrderBy(p => p.Key))
                {
                    var searchMetadata = await packageSearchResource.SearchAsync(packageId, new SearchFilter(true),0, 1, null, CancellationToken.None).ConfigureAwait(false);
                    var metadata = searchMetadata.FirstOrDefault();
                    if (metadata != null && metadata.Identity.Id == packageId && !string.IsNullOrWhiteSpace(metadata.ProjectUrl?.ToString()))
                    {
                        var url = metadata.ProjectUrl?.ToString();
                        await file.WriteLineAsync($" - [{packageId}]({url}) {version}").ConfigureAwait(false);
                    }
                    else
                    {
                       await file.WriteLineAsync($" - {packageId} {version}").ConfigureAwait(false);
                    }
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

        private async Task WriteDependencyFile(IDictionary<string, IList<string>> dependencies, string fileName, string title)
        {
            var filePath = Path.Combine(OutputPath, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            using (var file = File.CreateText(filePath))
            {
                await file.WriteLineAsync($"# {title}").ConfigureAwait(false);
                foreach (var (node, leafs) in dependencies.OrderBy(t => t.Key))
                {
                    await file.WriteLineAsync($"### {node}").ConfigureAwait(false);
                    foreach (var leaf in leafs.OrderBy(v => v))
                    {
                        await file.WriteLineAsync($" - {leaf}").ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
