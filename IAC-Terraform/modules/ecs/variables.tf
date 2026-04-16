
variable "aws_region" {
  description = "AWS region name"
  type        = string
}


variable "ecs_task_execution_role_arn" {
  description = "ECS task execution role ARN"
  type        = string
}


variable "ecs_task_role_arn" {
  description = "ECS task role ARN"
  type        = string
}


variable "public_subnet_ids" {
  description = "List of public subnet ids for ECS services"
  type        = list(string)
}


variable "app_sg_ids" {
  description = "List of security group IDs for App service"
  type        = list(string)
}


variable "app_tg_arn" {
  description = "App target group ARN"
  type        = string
}


variable "listener_arn" {
  description = "ALB listener ARN"
  type        = string
}


variable "public_alb_dns_name" {
  description = "Public ALB DNS name"
  type        = string
}


variable "app_container_image" {
  description = "App service container image with tag version"
  type        = string
}


variable "app_container_port" {
  description = "App container listener port"
  type        = number
}


variable "task_resource_sizes" {
  description = "ECS task CPU and memory configurations"

  type = map(object({
    cpu    = number
    memory = number
  }))
}


variable "app_task_resources" {
  description = "App task size resources"
  type        = string
}


variable "app_default_connection_string" {
  description = "App credentials connetion string"
  type        = string
}


variable "database_id" {
  description = "Postgres RDS ID"
  type        = string
}
