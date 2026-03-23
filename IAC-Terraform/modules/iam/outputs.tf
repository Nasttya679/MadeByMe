

output "ecs_main_exec_role_arn" {
  description = "ECS task execution role ARN"
  value       = aws_iam_role.ecsTaskExecutionRole.arn
}
