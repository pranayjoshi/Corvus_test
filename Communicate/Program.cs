using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.Build.Construction;
using Microsoft.Build.Locator;
// using CodeGenerationPackage.Protos;
using System.Collections.Generic;
using Microsoft.Build.Evaluation;

class Program
{
    private static Dictionary<string, AdditionalFileInfo> _additionalFilesIndex;

    static async Task Main(string[] args)
    {
        // Ensure the MSBuild instance is registered
        MSBuildLocator.RegisterDefaults();

        // Initialize the additional files index
        _additionalFilesIndex = await CreateAdditionalFilesIndexAsync(args);

        // Parse the project file
        var projectPath = args[0];
        var project = LoadProject(projectPath);

        // Read code generation settings
        var settings = GetCodeGenerationSettings(project);

        // Get the dependency tree from the external service
        var dependencyTree = await GetDependencyTreeAsync(project, settings);

        // Check if code regeneration is needed
        var shouldRegenerate = CheckIfRegenerationNeeded(dependencyTree);

        if (shouldRegenerate)
        {
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            try
            {
                // Generate code
                var generatedCode = await GenerateCodeAsync(project, settings, cts.Token);

                // Write the generated code to the output directory
                var outputPath = Path.Combine(project.DirectoryPath, settings.OutputDirectory, "GeneratedCode.cs");
                File.WriteAllText(outputPath, generatedCode);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Code generation was canceled.");
            }
        }
    }

    private static async Task<string> GenerateCodeAsync(ProjectRootElement project, CodeGenerationSettings settings, CancellationToken cancellationToken)
    {
        // Create a gRPC channel to communicate with the external service
        var channel = GrpcChannel.ForAddress("https://your-service.example.com");
        var client = new GeneratorService.GeneratorServiceClient(channel);

        // Send a request to the external service
        var request = new CodeGenerationRequest { ProjectPath = project.FullPath, Settings = settings.ToProtobuf() };
        var reply = await client.GenerateCodeAsync(request, cancellationToken);

        return reply.GeneratedCode;
    }

    private static async Task<DependencyTree> GetDependencyTreeAsync(ProjectRootElement project, CodeGenerationSettings settings)
    {
        // Create a gRPC channel to communicate with the external service
        var channel = GrpcChannel.ForAddress("url");
        var client = new GeneratorService.GeneratorServiceClient(channel);

        // Send a request to the external service
        var request = new GetDependencyTreeRequest { ProjectPath = project.FullPath, Settings = settings.ToProtobuf() };
        var reply = await client.GetDependencyTreeAsync(request);

        return reply.Tree.FromProtobuf();
    }

    private static ProjectRootElement LoadProject(string projectPath)
    {
        var projectCollection = new ProjectCollection();
        var project = projectCollection.LoadProject(projectPath);
        return project.Xml;
    }

    private static CodeGenerationSettings GetCodeGenerationSettings(ProjectRootElement project)
    {
        var settingsXml = project.GetPropertyValue("CodeGenerationSettings");
        // Parse the XML settings and return a CodeGenerationSettings instance
        return new CodeGenerationSettings
        {
            OutputDirectory = project.GetPropertyValue("OutputDirectory"),
            Namespace = project.GetPropertyValue("Namespace"),
            GenerateClasses = bool.Parse(project.GetPropertyValue("GenerateClasses")),
            GenerateEnums = bool.Parse(project.GetPropertyValue("GenerateEnums"))
        };
    }

    private static bool CheckIfRegenerationNeeded(DependencyTree tree)
    {
        // Check the timestamps or hashes of the dependent files
        // Return true if any file has changed, false otherwise
        return true;
    }

    private static async Task<Dictionary<string, AdditionalFileInfo>> CreateAdditionalFilesIndexAsync(string[] args)
    {
        // Create a gRPC channel to communicate with the external service
        var channel = GrpcChannel.ForAddress("url");
        var client = new GeneratorService.GeneratorServiceClient(channel);

        // Send a request to the external service to get the additional files
        var request = new GetAdditionalFilesRequest { ProjectPath = args[0] };
        var reply = await client.GetAdditionalFilesAsync(request);

        var index = new Dictionary<string, AdditionalFileInfo>();
        foreach (var file in reply.Files)
        {
            index[file.FullPath] = new AdditionalFileInfo
            {
                FullPath = file.FullPath,
                Name = Path.GetFileName(file.FullPath),
                // Add any other relevant properties
            };
        }

        return index;
    }
}

public class AdditionalFileInfo
{
    public string FullPath { get; set; }
    public string Name { get; set; }
}

public class CodeGenerationSettings
{
    public string OutputDirectory { get; set; }
    public string JsonPath { get; set; }
    public bool GenerateClasses { get; set; }
    public bool GenerateEnums { get; set; }

    public CodeGenerationSettings ToProtobuf()
    {
        return new CodeGenerationSettings
        {
            OutputDirectory = OutputDirectory,
            JsonPath = JsonPath,
            GenerateClasses = GenerateClasses,
            GenerateEnums = GenerateEnums
        };
    }
}

public class DependencyTree
{
    public string Name { get; set; }
    public List<DependencyTree> Dependencies { get; set; }

    public DependencyTree ToProtobuf()
    {
        var proto = new DependencyTreeProto
        {
            Name = this.Name,
            Dependencies = { this.Dependencies.Select(d => d.ToProtobuf()) }
        };

        return proto;
    }

    public static DependencyTree FromProtobuf(DependencyTreeProto proto)
    {
        var tree = new DependencyTree
        {
            Name = proto.Name,
            Dependencies = proto.Dependencies.Select(DependencyTree.FromProtobuf).ToList()
        };

        return tree;
    }
}