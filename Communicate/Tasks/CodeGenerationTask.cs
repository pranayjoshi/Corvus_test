// CodeGenerationTask.cs
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Threading.Tasks;

namespace MyCodeGenerator
{
    public class CodeGenerationTask : Task
    {
        [Required]
        public ITaskItem[] FilteredFiles { get; set; }

        [Output]
        public ITaskItem[] GeneratedFiles { get; set; }

        public override bool Execute()
        {
            var generatedFiles = new List<ITaskItem>();

            foreach (var file in FilteredFiles)
            {
                var generatedFile = GenerateCode(file);
                generatedFiles.Add(generatedFile);
            }

            GeneratedFiles = generatedFiles.ToArray();
            return true;
        }

        private ITaskItem GenerateCode(ITaskItem file)
        {
            // Read the file content
            var fileContent = File.ReadAllText(file.ItemSpec);

            // Parse the code and generate new code
            var syntaxTree = CSharpSyntaxTree.ParseText(fileContent);
            var compilation = CSharpCompilation.Create("MyCodeGenerator")
                .AddSyntaxTrees(syntaxTree);

            // Implement your code generation logic here
            var generatedCode = "// Generated code goes here";

            // Create a new file for the generated code
            var generatedFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.cs");
            File.WriteAllText(generatedFilePath, generatedCode);

            return new TaskItem(generatedFilePath);
        }
    }
}