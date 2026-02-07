using System.CommandLine;
using Amaspire;

var rootCommand = new RootCommand("Generate AWS CloudFormation templates from .NET Aspire manifests");

var generateCommand = new Command("generate", "Generate CloudFormation templates from Aspire manifest");
var manifestOption = new Option<FileInfo>("--manifest", "Path to Aspire manifest JSON file") { IsRequired = true };
var outputOption = new Option<DirectoryInfo>("--output", "Output directory for CloudFormation templates") { IsRequired = true };
var envOption = new Option<string>("--env", () => "prod", "Environment name (dev, staging, prod)");

generateCommand.AddOption(manifestOption);
generateCommand.AddOption(outputOption);
generateCommand.AddOption(envOption);

generateCommand.SetHandler(async (manifest, output, env) =>
{
    await Generator.GenerateAsync(manifest, output, env);
}, manifestOption, outputOption, envOption);

rootCommand.AddCommand(generateCommand);

return await rootCommand.InvokeAsync(args);
