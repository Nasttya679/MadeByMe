
output "ecs_cluster_name" {
  description = "ECS cluster name"
  value       = aws_ecs_cluster.main_cluster.name
}


output "ecs_app_service_name" {
  description = "ECS cluster app service name"
  value       = aws_ecs_service.app_service.name
}
