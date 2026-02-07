using System.Text.Json;
using Amaspire.Models;
using Scriban;

namespace Amaspire;

public static class Generator
{
    public static async Task GenerateAsync(FileInfo manifestFile, DirectoryInfo outputDir, string environment)
    {
        if (!manifestFile.Exists)
            throw new FileNotFoundException($"Manifest file not found: {manifestFile.FullName}");

        outputDir.Create();

        var json = await File.ReadAllTextAsync(manifestFile.FullName);
        var manifest = JsonSerializer.Deserialize<AspireManifest>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("Failed to parse manifest");

        Console.WriteLine($"Generating CloudFormation templates for environment: {environment}");
        Console.WriteLine($"Found {manifest.Resources.Count} resources");

        foreach (var (name, resource) in manifest.Resources)
        {
            if (resource.Type == "project.v0")
            {
                await GenerateEcsServiceAsync(name, resource, outputDir, environment);
            }
        }

        Console.WriteLine($"✓ Templates generated in {outputDir.FullName}");
    }

    private static async Task GenerateEcsServiceAsync(string name, Resource resource, DirectoryInfo outputDir, string environment)
    {
        var httpBinding = resource.Bindings?.FirstOrDefault(b => b.Value.Scheme == "http" || b.Value.Scheme == "https");
        var containerPort = httpBinding?.Value.ContainerPort ?? 8080;

        var template = Template.Parse(EcsServiceTemplate);
        var model = new
        {
            service_name = name,
            environment = environment,
            container_port = containerPort,
            image = resource.Image ?? $"{name}:latest",
            env_vars = resource.Env?.Select(kv => new { key = kv.Key, value = kv.Value.ToString() }).ToList() ?? new()
        };
        
        var output = await template.RenderAsync(model);

        var fileName = $"{name}.yaml";
        await File.WriteAllTextAsync(Path.Combine(outputDir.FullName, fileName), output);
        Console.WriteLine($"  → {fileName}");
    }

    private const string EcsServiceTemplate = @"AWSTemplateFormatVersion: '2010-09-09'
Description: 'Aspire-generated stack ({{ service_name }}) for ECS on Fargate with ALB'

Parameters:
  VpcId:
    Type: String
    Description: VPC ID for the ECS service
  Subnets:
    Type: List<AWS::EC2::Subnet::Id>
    Description: Subnets for the ECS service and ALB
  EcrRepoName:
    Type: String
    Default: '{{ service_name }}'
    Description: ECR repository name
  ImageTag:
    Type: String
    Description: Docker image tag to deploy
  ContainerPort:
    Type: Number
    Default: {{ container_port }}
    Description: Container port
  LogRetention:
    Type: Number
    Default: 14
    Description: CloudWatch Logs retention in days

Resources:
  LogGroup:
    Type: AWS::Logs::LogGroup
    Properties:
      LogGroupName: !Sub '/ecs/${AWS::StackName}-{{ service_name }}'
      RetentionInDays: !Ref LogRetention

  Cluster:
    Type: AWS::ECS::Cluster
    Properties:
      ClusterName: !Sub '${AWS::StackName}-cluster'

  TaskExecutionRole:
    Type: AWS::IAM::Role
    Properties:
      AssumeRolePolicyDocument:
        Version: '2012-10-17'
        Statement:
          - Effect: Allow
            Principal:
              Service: ecs-tasks.amazonaws.com
            Action: 'sts:AssumeRole'
      ManagedPolicyArns:
        - arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy

  TaskDefinition:
    Type: AWS::ECS::TaskDefinition
    Properties:
      Family: !Sub '${AWS::StackName}-{{ service_name }}'
      RequiresCompatibilities:
        - FARGATE
      Cpu: '512'
      Memory: '1024'
      NetworkMode: awsvpc
      ExecutionRoleArn: !GetAtt TaskExecutionRole.Arn
      ContainerDefinitions:
        - Name: {{ service_name }}
          Image: !Sub '${AWS::AccountId}.dkr.ecr.${AWS::Region}.amazonaws.com/${EcrRepoName}:${ImageTag}'
          PortMappings:
            - ContainerPort: !Ref ContainerPort
          LogConfiguration:
            LogDriver: awslogs
            Options:
              awslogs-group: !Ref LogGroup
              awslogs-region: !Ref 'AWS::Region'
              awslogs-stream-prefix: ecs
          Environment:
{{~ for env in env_vars ~}}
            - Name: {{ env.key }}
              Value: '{{ env.value }}'
{{~ end ~}}
            - Name: ASPNETCORE_URLS
              Value: !Sub 'http://0.0.0.0:${ContainerPort}'

  ALB:
    Type: AWS::ElasticLoadBalancingV2::LoadBalancer
    Properties:
      Name: !Sub '${AWS::StackName}-{{ service_name }}-alb'
      Type: application
      Scheme: internet-facing
      Subnets: !Ref Subnets

  TargetGroup:
    Type: AWS::ElasticLoadBalancingV2::TargetGroup
    Properties:
      Name: !Sub '${AWS::StackName}-{{ service_name }}-tg'
      TargetType: ip
      Port: !Ref ContainerPort
      Protocol: HTTP
      VpcId: !Ref VpcId
      HealthCheckPath: /health
      HealthCheckIntervalSeconds: 30
      HealthCheckTimeoutSeconds: 5
      HealthyThresholdCount: 2
      UnhealthyThresholdCount: 3

  Listener:
    Type: AWS::ElasticLoadBalancingV2::Listener
    Properties:
      LoadBalancerArn: !Ref ALB
      Port: 80
      Protocol: HTTP
      DefaultActions:
        - Type: forward
          TargetGroupArn: !Ref TargetGroup

  Service:
    Type: AWS::ECS::Service
    DependsOn: Listener
    Properties:
      ServiceName: !Sub '${AWS::StackName}-{{ service_name }}'
      Cluster: !Ref Cluster
      LaunchType: FARGATE
      DesiredCount: 2
      TaskDefinition: !Ref TaskDefinition
      NetworkConfiguration:
        AwsvpcConfiguration:
          AssignPublicIp: ENABLED
          Subnets: !Ref Subnets
      LoadBalancers:
        - ContainerName: {{ service_name }}
          ContainerPort: !Ref ContainerPort
          TargetGroupArn: !Ref TargetGroup

Outputs:
  LoadBalancerUrl:
    Description: URL of the load balancer
    Value: !Sub 'http://${ALB.DNSName}'
  ServiceName:
    Description: ECS Service name
    Value: !Ref Service
";
}
