// GeneratorService.cs
using System.Threading.Tasks;
using Grpc.Core;
using static GeneratorService;

class GeneratorServiceImpl : GeneratorServiceClient
{
    public override Task<GenerateCodeResponse> GenerateCode(GenerateCodeRequest request, ServerCallContext context)
{
    CancellationToken cancellationToken = context.CancellationToken;

    // Implement code generation logic here
    var generatedCode = GenerateCodeFromSource(request.SourceCode, cancellationToken);

    return Task.FromResult(new GenerateCodeResponse { GeneratedCode = generatedCode });
}

private string GenerateCodeFromSource(string sourceCode, CancellationToken cancellationToken)
{
    // Your code generation logic goes here
    // You should periodically check cancellationToken.IsCancellationRequested and stop processing if it's true

    StringBuilder generatedCode = new StringBuilder();

    foreach (var line in sourceCode.Split('\n'))
    {
        // Check if cancellation was requested
        if (cancellationToken.IsCancellationRequested)
        {
            // Stop processing and return
            return "// Code generation was cancelled";
        }

        // Continue with your code generation logic
        generatedCode.AppendLine("// Generated code for line goes here");
    }

    return generatedCode.ToString();
}
}