// // GeneratorService.cs
// using System.Threading.Tasks;
// using Grpc.Core;
// using static GeneratorService;

// class GeneratorServiceImpl : GeneratorService.GeneratorServiceBase
// {
//     public override Task<GenerateCodeResponse> GenerateCode(GenerateCodeRequest request, ServerCallContext context)
//     {
//         // Implement code generation logic here
//         var generatedCode = GenerateCodeFromSource(request.SourceCode);

//         return Task.FromResult(new GenerateCodeResponse { GeneratedCode = generatedCode });
//     }

//     private string GenerateCodeFromSource(string sourceCode)
//     {
//         // Your code generation logic goes here
//         return "// Generated code goes here";
//     }
// }