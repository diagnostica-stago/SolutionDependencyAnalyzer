using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SolutionDependencyAnalyzer
{
    public sealed class DotWriter : AWriter
    {
        public DotWriter(string outputPath) : base(outputPath)
        {
        }

        /// <summary>
        /// Creates a dot file and the associated png graph of project dependencies
        /// </summary>
        /// <param name="projectDependencies">A dictionary in which the key is a project, and the values are its dependencies</param>
        /// <param name="graphTitle">Title of the generated dot graph</param>
        /// <param name="createImage">true if the graph image should be created</param>
        public async Task WriteProjectDependencyGraph(IDictionary<string, IList<string>> projectDependencies, string graphTitle, bool createImage)
        {
            if (projectDependencies == null)
            {
                throw new ArgumentNullException(nameof(projectDependencies));
            }
            var fileName = Path.Combine(OutputPath, "projectDependencyGraph.dot");
            var graphFileName = Path.Combine(OutputPath, "projectDependencyGraph.png");
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            if (File.Exists(graphFileName))
            {
                File.Delete(graphFileName);
            }
            using (var file = File.CreateText(fileName))
            {
                await file.WriteLineAsync($"digraph \"{graphTitle}\" {{").ConfigureAwait(false);
                await file.WriteLineAsync("splines=ortho;").ConfigureAwait(false);
                foreach (var (project, dependencies) in projectDependencies)
                {
                    // if edges a -> b and b -> c exist, remove a -> c to have a readable graph
                    var toExclude = dependencies.SelectMany(v => projectDependencies.ContainsKey(v) ? projectDependencies[v] : new List<string>()).Distinct();

                    foreach (var dependency in dependencies.Except(toExclude))
                    {
                        await file.WriteLineAsync($"\"{project}\" -> \"{dependency}\"").ConfigureAwait(false);
                    }
                }
                await file.WriteLineAsync("}").ConfigureAwait(false);
            }

            if (createImage)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo("dot.exe");
                startInfo.Arguments = $"-Tpng {fileName} -o {graphFileName}";
                var process = Process.Start(startInfo);
                process.WaitForExit();
            }
        }
    }
}
