# Contributing to amaspire

Thank you for your interest in contributing!

## Development Setup

1. Clone the repository
2. Install .NET 9 SDK
3. Restore dependencies: `dotnet restore`
4. Build: `dotnet build`
5. Run tests: `dotnet test`

## Running Locally

```bash
# Build and install as global tool
dotnet pack src/amaspire.csproj
dotnet tool install --global --add-source ./artifacts amaspire

# Test with sample
amaspire generate \
  --manifest ./samples/sample-manifest.json \
  --output ./test-output \
  --env dev
```

## Adding New Resource Types

1. Update `Models/AspireManifest.cs` if needed
2. Add mapping logic in `Generator.cs`
3. Create Scriban template for CloudFormation resources
4. Add tests in `tests/`
5. Update `docs/resource-mapping.md`

## Testing CloudFormation Templates

```bash
# Install cfn-lint
pip install cfn-lint

# Validate generated templates
cfn-lint ./test-output/*.yaml

# Validate with AWS CLI
aws cloudformation validate-template --template-body file://./test-output/apiservice.yaml
```

## Pull Request Process

1. Create a feature branch
2. Add tests for new functionality
3. Update documentation
4. Ensure CI passes
5. Request review

## Code Style

- Follow standard C# conventions
- Use nullable reference types
- Keep methods focused and small
- Add XML comments for public APIs

## Roadmap Priorities

See [README.md](./README.md) for version roadmap. Current focus:
- v0.1: ECS/Fargate + ALB + Logs (MVP)
- v0.2: Nested stacks, Secrets Manager, AutoScaling
