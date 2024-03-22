// Program.cs
using System;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;


namespace MyCodeGenerator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Register MSBuild instance
            MSBuildLocator.RegisterDefaults();

            // Load the target project
            var projectPath = args[0];
            // var project = await ProjectLoader.LoadProjectAsync(projectPath);

            // Configure the code generation tasks
            var filterTask = project.CreateTask<FilterFilesTask>()
                .SetProperty("Predicate", "*.cs")
                .SetProperty("AdditionalFiles", project.GetItems("AdditionalFiles"));

            var generationTask = project.CreateTask<CodeGenerationTask>()
                .SetProperty("FilteredFiles", filterTask.Output
                .Items);

            // Execute the tasks
            var success = await generationTask.ExecuteAsync();

            if (success)
            {
                Console.WriteLine("Code generation completed successfully.");
            }
            else
            {
                Console.WriteLine("Code generation failed.");
            }
        }
    }
}