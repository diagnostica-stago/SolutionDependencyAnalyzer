using Buildalyzer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SolutionDependencyAnalyzer
{
    public sealed class DependencyAnalyzer
    {
        public string Solution { get; }

        /// <summary>
        /// Contains the package ID as key, and its version as value
        /// </summary>
        public ConcurrentDictionary<string, string> PackageResults { get; } = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// A dictionary in which the key is a project, and the values are its dependencies
        /// </summary>
        public ConcurrentDictionary<string, IList<string>> ProjectResults { get; } = new ConcurrentDictionary<string, IList<string>>();

        /// <summary>
        /// A dictionary in which the key is a project, and the values are its package dependencies
        /// </summary>
        public ConcurrentDictionary<string, IList<string>> PackagesByProject { get; } = new ConcurrentDictionary<string, IList<string>>();

        /// <summary>
        /// A dictionary in which the key is a package, and the values are its project dependencies
        /// </summary>
        public ConcurrentDictionary<string, IList<string>> ProjectsByPackage { get; private set; } = new ConcurrentDictionary<string, IList<string>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="solutionPath">The full solution path</param>
        public DependencyAnalyzer(string solutionPath)
        {
            if (solutionPath != null && solutionPath.EndsWith("sln", StringComparison.InvariantCultureIgnoreCase) && File.Exists(solutionPath))
            {
                Solution = solutionPath;
            }
            else
            {
                throw new ArgumentException($"Invalid Solution: {solutionPath}", nameof(solutionPath));
            }
        }

        /// <summary>
        /// Runs the analysis on the solution projects. At the end of this method, these properties will be filled:
        /// - <see cref="PackageResults"/>
        /// - <see cref="ProjectResults"/>
        /// - <see cref="PackagesByProject"/>
        /// - <see cref="ProjectsByPackage"/>
        /// </summary>
        public async Task AnalyzeAsync()
        {
            var analyzerManager = new AnalyzerManager(Solution);
            var tasks = new List<Task>();
            foreach (var project in analyzerManager.Projects)
            {
                tasks.Add(AnalyzeProject(project));
            }

            await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
            ProjectsByPackage = GetProjectsByPackage(PackagesByProject);
        }

        private async Task AnalyzeProject(KeyValuePair<string, ProjectAnalyzer> project)
        {
            await Task.Run(() =>
            {
                var projectName = Path.GetFileNameWithoutExtension(project.Key);
                Console.WriteLine($"Building Project {projectName}");
                var results = project.Value.Build().FirstOrDefault();
                PackagesByProject.TryAdd(projectName, new List<string>());
                foreach (var kvp in results.PackageReferences.Where(p => p.Value.ContainsKey("Version")))
                {
                    PackagesByProject[projectName].Add(kvp.Key + " " + kvp.Value["Version"]);
                    PackageResults.TryAdd(kvp.Key, kvp.Value["Version"]);
                }
                foreach (var kvp in results.ProjectReferences)
                {
                    ProjectResults.TryAdd(projectName, results.ProjectReferences.Select(p => Path.GetFileNameWithoutExtension(p)).ToList());
                }
                Console.WriteLine($"Project {projectName} done");
            }).ConfigureAwait(false);
        }

        private ConcurrentDictionary<string, IList<string>> GetProjectsByPackage(ConcurrentDictionary<string, IList<string>> packageDepByProject)
        {
            var result = new ConcurrentDictionary<string, IList<string>>();
            foreach (var kvp in packageDepByProject)
            {
                foreach (var package in kvp.Value)
                {
                    if (!result.ContainsKey(package))
                    {
                        result.TryAdd(package, new List<string>());
                    }
                    result[package].Add(kvp.Key);
                }
            }
            return result;
        }
    }
}
