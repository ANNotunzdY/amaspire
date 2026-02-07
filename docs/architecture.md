# Architecture

## Overview

amaspire transforms .NET Aspire manifests into AWS CloudFormation templates, enabling production deployment on AWS ECS/Fargate.

```
Aspire Manifest (JSON)
        ↓
   [Parser]
        ↓
   [Mapper] ← Resource mapping rules
        ↓
   [Template Engine] ← Scriban templates
        ↓
CloudFormation YAML
```

## Components

### 1. Parser
- Reads Aspire manifest JSON
- Validates schema version
- Deserializes into typed models

### 2. Mapper
Maps Aspire resource types to AWS resources:

| Aspire Type | AWS Resources |
|-------------|---------------|
| `project.v0` | ECS::TaskDefinition, ECS::Service, ELBv2::* |
| `container.v0` | ECS::Service or external endpoint |
| Bindings (HTTP/HTTPS) | ALB Listener, TargetGroup |
| Environment variables | TaskDefinition.Environment |
| Logs | Logs::LogGroup |

### 3. Template Engine
- Uses Scriban for YAML generation
- Supports parameterization (VPC, Subnets, ImageTag)
- Generates CloudFormation-compliant YAML

## Resource Generation Strategy

### MVP (v0.1)
- Single stack per service
- Assumes existing VPC/Subnets (parameters)
- ECR images pre-pushed by CI
- Basic health checks on `/health`

### Future
- Nested stacks (network/app separation)
- Service discovery (Cloud Map)
- Secrets Manager integration
- AutoScaling policies
- HTTPS/TLS with ACM

## Design Decisions

### Why YAML over CDK?
- No build step required
- Direct `aws cloudformation deploy`
- Easier to review and audit
- Consistent with Aspirate's approach

### Why Scriban?
- Lightweight templating
- Good .NET integration
- Supports complex logic without code generation

### Parameterization Strategy
- VPC/Subnets: User-provided (existing infrastructure)
- Image tags: CI/CD pipeline controls
- Resource sizing: Template parameters with defaults
- Environment variables: From Aspire manifest + overrides
