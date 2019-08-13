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
        public async Task WriteProjectDependencyGraph(IDictionary<string, IList<string>> projectDependencies, string graphTitle)
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
                foreach (var kvp in projectDependencies)
                {
                    // if edges a -> b and b -> c exist, remove a -> c to have a readable graph
                    var toExclude = kvp.Value.SelectMany(v => projectDependencies.ContainsKey(v) ? projectDependencies[v] : new List<string>()).Distinct();

                    foreach (var dependency in kvp.Value.Except(toExclude))
                    {
                        await file.WriteLineAsync($"\"{kvp.Key}\" -> \"{dependency}\"").ConfigureAwait(false);
                    }
                }
                await file.WriteLineAsync("}").ConfigureAwait(false);
            }

            ProcessStartInfo startInfo = new ProcessStartInfo("dot.exe");
            startInfo.Arguments = $"-Tpng {fileName} -o {graphFileName}";
            var process = Process.Start(startInfo);
            process.WaitForExit();
        }
    }
}
