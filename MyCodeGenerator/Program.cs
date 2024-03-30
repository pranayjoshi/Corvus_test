using System;
using System.Reflection;

class Program
{
    static void Main(string[] args)
    {
        // Get the assembly where the Program class is defined
        Assembly assembly = Assembly.GetExecutingAssembly();

        // Get the custom attributes applied to the assembly
        var attributes = assembly.GetCustomAttributes();

        foreach (var attribute in attributes)
        {
            // Check if the attribute is of type CodeGenerationTaskAttribute
            if (attribute is CodeGenerationTaskAttribute codeGenerationAttribute)
            {
                // Access the properties of the attribute
                string filterFiles = codeGenerationAttribute.FilteredFiles;
                string generatedFiles = codeGenerationAttribute.GeneratedFiles;
                string generatorServiceAddress = codeGenerationAttribute.GeneratorServiceAddress;

                // Output the values of the properties
                Console.WriteLine($"FilteredFiles: {filterFiles}");
                Console.WriteLine($"GeneratedFiles: {generatedFiles}");
                Console.WriteLine($"GeneratorServiceAddress: {generatorServiceAddress}");
            }
        }
    }
}

// Define a custom attribute to represent the CodeGenerationTask
[AttributeUsage(AttributeTargets.Assembly)]
class CodeGenerationTaskAttribute : Attribute
{
    // Properties to store the arguments
    public string FilteredFiles { get; set; }
    public string GeneratedFiles { get; set; }
    public string GeneratorServiceAddress { get; set; }
}
