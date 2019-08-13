# SolutionDependencyAnalyzer

There are two ways to use the tool: Command line or use the [NuGet package](https://www.nuget.org/packages/dependency-analyzer/) in your project.

## Command Line

### Installation

```
dotnet tool install --global dependency-analyzer
```

### Usage
```
dependency-analyzer <SolutionFullPath> <OutputPath>
```

### Files Created

This tool will create five files in the provided **OutputPath**:
- `packages.md` contains the list of NuGet packages used by the projects in the solution, with their version.
- `packagesByProject.md` will list the NuGet packages used by each project in the solution, with their version.
- `projectsByPackage.md` does the opposite: it will list the projects that use each package
- `projectDependencyGraph.dot` is the dependency graph of the projects in the solution. SolutionDependencyAnalyzer also generates a png of that graph, `projectDependencyGraph.png`


## Nuget Dependency

### Installation
Add the [dependency-analyzer NuGet package](https://www.nuget.org/packages/dependency-analyzer/) to your project

### Usage
Create a `DependencyAnalyzer` and call it, with `Solution containing the full solution path`: 
```cs
var dependencyAnalyzer = new DependencyAnalyzer(Solution);
await dependencyAnalyzer.AnalyzeAsync();
``` 
Now `dependencyAnalyzer` properties contain everything you need:
- `PackageResults` contains the package ID as key, and its version as value
- `ProjectResults` is a dictionary in which the key is a project, and the values are its project dependencies
- `PackagesByProject` is a dictionary in which the key is a project, and the values are its package dependencies
- `ProjectsByPackage` is a dictionary in which the key is a package, and the values are its project dependencies

You can also call the writers (`MarkdownWriter` and `DotWriter`) if you want to write the same fils as the comand line tool does.


### Thanks
This project was heavily inspired by [dotnet-depends](https://github.com/mholo65/depends), and like dotnet-depends, it uses the amazing [Buildalyzer](https://github.com/daveaglick/Buildalyzer)