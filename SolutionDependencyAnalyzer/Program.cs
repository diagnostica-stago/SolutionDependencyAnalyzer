using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace SolutionDependencyAnalyzer
{
    public class Program
    {
        [Argument(0, Description = "The solution file to analyze.")]
        [Required]
        public string? Solution { get; set; }

        [Argument(1, Description = "The output path for file results")]
        [Required]
        public string? OutputPath { get; set; }

        [Option("-g|--create-graph-image", Description = "Runs dot to create a png from the dotfile. Make sure to have dot installed before activating this option")]
        public bool WriteGraph { get; set; }

        [Option("--exclude-test-projects", Description = "Excludes test projects from analysis. A test project is anything with \"test\" in its name.")]
        public bool ExcludeTestProjects { get; set; }

        [Option("-x|--exclude-project", Description = "Excludes a specific project from analysis")]
        public string[] ProjectsToExclude { get; set; }

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
            var dependencyAnalyzer = new DependencyAnalyzer(Solution!);
            await dependencyAnalyzer.AnalyzeAsync(ExcludeTestProjects, ProjectsToExclude).ConfigureAwait(false);

            var markdownWriter = new MarkdownWriter(OutputPath!);
            var dotWriter = new DotWriter(OutputPath!);
            var tasks = new Task[]
            {
                dotWriter.WriteProjectDependencyGraph(dependencyAnalyzer.ProjectResults, Path.GetFileNameWithoutExtension(Solution!), WriteGraph),
                markdownWriter.WritePackages(dependencyAnalyzer.PackageResults),
                markdownWriter.WritePackagesDependenciesByProject(dependencyAnalyzer.PackagesByProject),
                markdownWriter.WriteProjectDependenciesByPackage(dependencyAnalyzer.ProjectsByPackage)
            };
            await Task.WhenAll(tasks).ConfigureAwait(false);
            Console.WriteLine("Done");
        }
    }
}