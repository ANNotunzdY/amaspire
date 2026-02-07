# Resource Mapping Reference

This document defines how Aspire manifest resources map to AWS CloudFormation resources.

## project.v0 (Container Project)

### Aspire Manifest
```json
{
  "type": "project.v0",
  "image": "myapp-api",
  "env": {
    "KEY": "value"
  },
  "bindings": {
    "http": {
      "scheme": "http",
      "containerPort": 8080
    }
  }
}
```

### Generated CloudFormation Resources

1. **AWS::ECS::TaskDefinition**
   - Family: `${StackName}-${ServiceName}`
   - Fargate compatibility
   - Container image from ECR
   - Port mappings from bindings
   - Environment variables from `env`

2. **AWS::ECS::Service**
   - Launch type: FARGATE
   - Desired count: 2 (default)
   - Network mode: awsvpc
   - Load balancer integration

3. **AWS::ElasticLoadBalancingV2::LoadBalancer**
   - Type: application
   - Scheme: internet-facing
   - Subnets from parameters

4. **AWS::ElasticLoadBalancingV2::TargetGroup**
   - Target type: ip
   - Health check: `/health`
   - Port from container binding

5. **AWS::ElasticLoadBalancingV2::Listener**
   - Port: 80 (HTTP)
   - Forward to target group

6. **AWS::Logs::LogGroup**
   - Name: `/ecs/${StackName}-${ServiceName}`
   - Retention: 14 days (default)

7. **AWS::IAM::Role** (Task Execution)
   - Managed policy: AmazonECSTaskExecutionRolePolicy

## container.v0 (Generic Container)

### MVP Behavior
- Generate ECS Service (same as project.v0)

### Future (v0.3)
- Support `AsExisting` pattern for external endpoints
- Parameter-based connection strings

## Bindings

### HTTP/HTTPS
```json
"bindings": {
  "http": {
    "scheme": "http",
    "containerPort": 8080
  }
}
```
→ ALB Listener (port 80) → TargetGroup → ECS Task (port 8080)

### Future: HTTPS
- ACM certificate parameter
- Listener on port 443
- HTTP→HTTPS redirect

## Environment Variables

### From Manifest
```json
"env": {
  "KEY": "value"
}
```
→ TaskDefinition.ContainerDefinitions[].Environment

### Auto-injected
- `ASPNETCORE_URLS`: `http://0.0.0.0:${ContainerPort}`

### Future: Secrets
- Secrets Manager references
- SSM Parameter Store

## Parameters

All generated templates include:
- `VpcId`: Existing VPC
- `Subnets`: List of subnet IDs
- `EcrRepoName`: ECR repository name
- `ImageTag`: Docker image tag
- `ContainerPort`: Container port (default from binding)
- `LogRetention`: CloudWatch Logs retention days

## Outputs

- `LoadBalancerUrl`: ALB DNS name
- `ServiceName`: ECS Service name
