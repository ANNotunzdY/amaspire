# amaspire

**Aspire Manifest to AWS CloudFormation Generator**

Generate production-ready AWS CloudFormation YAML templates from .NET Aspire manifests.

## Why CloudFormation YAML?

- **No build required**: Direct deployment with `aws cloudformation deploy`
- **Native AWS integration**: Stack diffs, rollbacks, and change sets
- **Readable**: YAML is cleaner for ECS TaskDefinitions and container configs
- **Consistent with ecosystem**: Similar to Aspirate's manifest→YAML approach

## Installation

### Prerequisites

- .NET 9.0 SDK or later
- AWS CLI (for deployment)

### Install as .NET Global Tool

```bash
dotnet tool install -g Amaspire
```

After installation, ensure .NET tools are in your PATH:

```bash
# Add to ~/.bashrc or ~/.bash_profile
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"

# Apply changes
source ~/.bashrc
```

### Build from Source

```bash
git clone https://github.com/YOUR_ORG/amaspire.git
cd amaspire
dotnet pack src/amaspire.csproj -o ./artifacts
dotnet tool install --global --add-source ./artifacts amaspire
```

### Verify Installation

```bash
amaspire --version
amaspire --help
```

## Quick Start

### 1. Generate Aspire Manifest

From your Aspire AppHost project:

```bash
# Using dotnet run
dotnet run --project ./AppHost --publisher manifest --output-path ./out/manifest.json

# Or using aspire CLI (if installed)
aspire do publish-manifest --output-path ./out/manifest.json
```

### 2. Generate CloudFormation Templates

```bash
amaspire generate \
  --manifest ./out/manifest.json \
  --output ./cfn \
  --env prod
```

### 3. Validate Templates

```bash
# Install cfn-lint (optional but recommended)
pip install cfn-lint

# Validate syntax
cfn-lint ./cfn/*.yaml

# Validate with AWS CLI
aws cloudformation validate-template --template-body file://./cfn/apiservice.yaml
```

### 4. Deploy to AWS

```bash
aws cloudformation deploy \
  --template-file ./cfn/apiservice.yaml \
  --stack-name myapp-prod-api \
  --capabilities CAPABILITY_NAMED_IAM \
  --parameter-overrides \
    VpcId=vpc-xxxxx \
    Subnets=subnet-xxxxx,subnet-yyyyy \
    ImageTag=v1.0.0
```

## MVP Features (v0.1)

- ✅ ECS Fargate TaskDefinition & Service
- ✅ Application Load Balancer (ALB) with Target Groups
- ✅ CloudWatch Logs with configurable retention
- ✅ ECR repository references
- ✅ Environment variable mapping
- ✅ Port binding from Aspire HTTP/HTTPS endpoints

## Resource Mapping

| Aspire Type | CloudFormation Resources |
|-------------|-------------------------|
| `project.v0` | ECS::TaskDefinition, ECS::Service, ELBv2::LoadBalancer, ELBv2::TargetGroup, ELBv2::Listener |
| `container.v0` | ECS::Service (or parameter for existing endpoints) |
| Logs | Logs::LogGroup |
| Images | ECR::Repository (optional) |

## Roadmap

- **v0.2**: Nested stacks, SSM/Secrets Manager, AutoScaling
- **v0.3**: Container resource policies (ECS vs existing endpoints)
- **v0.4**: Lambda + API Gateway, DynamoDB, S3
- **v0.5**: CI/CD templates (GitHub Actions, CodeBuild)

## License

MIT

## Contributing

See [CONTRIBUTING.md](./CONTRIBUTING.md)
