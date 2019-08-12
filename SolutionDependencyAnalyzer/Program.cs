using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Buildalyzer;
using McMaster.Extensions.CommandLineUtils;

namespace SolutionDependencyAnalyzer
{
    public class Program
    {
        [Argument(0, Description = "The solution file to analyze.")]
        public string Solution { get; set; }

        [Argument(1, Description = "The output path for file results")]
        public string OutputPath { get; set; }

        public static Task<int> Main(string[] args)
        {
            return CommandLineApplication.ExecuteAsync<Program>(args);
        }

        private ValidationResult OnValidate()
        {
            if (Solution != null && Solution.EndsWith("sln", StringComparison.InvariantCultureIgnoreCase) && File.Exists(Solution))
            {
                if (!string.IsNullOrEmpty(OutputPath) && !Directory.Exists(OutputPath))
                {
                    return new ValidationResult("Output file invalid");
                }
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult("Solution file invalid");
            }
        }

        private async Task OnExecuteAsync()
        {
            var dependencyAnalyzer = new DependencyAnalyzer(Solution);
            await dependencyAnalyzer.AnalyzeAsync().ConfigureAwait(false);

            var markdownWriter = new MarkdownWriter(OutputPath);
            var dotWriter = new DotWriter(OutputPath);
            var tasks = new Task[]
            {
                dotWriter.WriteProjectDependencyGraph(dependencyAnalyzer.ProjectResults, Path.GetFileNameWithoutExtension(Solution)),
                markdownWriter.WritePackages(dependencyAnalyzer.PackageResults),
                markdownWriter.WritePackagesDependenciesByProject(dependencyAnalyzer.PackagesByProject),
                markdownWriter.WriteProjectDependenciesByPackage(dependencyAnalyzer.ProjectsByPackage)
            };
            await Task.WhenAll(tasks).ConfigureAwait(false);
            Console.WriteLine("Done. Press any key to exit.");
            Console.ReadKey();
        }
    }
}