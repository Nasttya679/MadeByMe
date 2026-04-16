

output "ecs_main_exec_role_arn" {
  description = "ECS task execution role ARN"
  value       = aws_iam_role.ecsTaskExecutionRole.arn
}


output "ecs_task_role_arn" {
  description = "ECS task role ARN"
  value       = aws_iam_role.ecsTaskRole.arn
}
