// CodeGenerationTask.cs
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Grpc.Net.Client;
using static GeneratorService;
using System.Threading;
using System.Threading.Tasks;
using System;
// using Newtonsoft.Json;

namespace MyCodeGenerator
{
    public class CodeGenerationTask : Task
    {
        public CodeGenerationTask(Action action, CancellationToken cancellationToken) : base(action, cancellationToken)
        {
        }

        [Required]
        public string[] FilteredJsons { get; set; }

        [Required]
        public CancellationToken CancellationToken { get; set; }

        [Required]
        public string GeneratorServiceAddress { get; set; }

        [Output]
        public string[] GeneratedJsons { get; set; }

        public override bool Execute()
        {
            var generatedJsons = new List<string>();

            foreach (var json in FilteredJsons)
            {
                CancellationToken.ThrowIfCancellationRequested();

                var generatedJson = GenerateCodeWithService(json, GeneratorServiceAddress);
                generatedJsons.Add(generatedJson);
            }

            GeneratedJsons = generatedJsons.ToArray();
            return true;
        }

        private string GenerateCodeWithService(string jsonInput, string grpcUrl)
        {
            // Create a gRPC channel to the generator service
            var channel = GrpcChannel.ForAddress(grpcUrl);
            var client = new GeneratorService.GeneratorServiceClient(channel);

            // Call the generator service
            var request = new GenerateCodeRequest { SourceCode = jsonInput };
            var response = client.GenerateCode(request);

            // Convert the generated code to JSON
            var generatedCode = response.GeneratedCode;

            return generatedCode;
        }
    }
}