
variable "aws_region" {
  type        = string
  description = "AWS region to deploy resources"
  default     = "us-east-1"
}


variable "task_sizes" {
  description = "ECS task CPU and RAM configurations"

  type = map(object({
    cpu    = number
    memory = number
  }))

  default = {
      small = {
        cpu    = 512
        memory = 1024
      }

      medium = {
        cpu    = 512
        memory = 2048
      }

      large = {
        cpu    = 2048
        memory = 8192
      }
  }
}


variable "app_listener_port" {
  type        = number
  description = "App container listener port"
  default     = 5000
}


variable "alb_listener_http_port" {
  type        = number
  description = "ALB HTTP listener port"
  default     = 80
}


variable "alb_listener_https_port" {
  type        = number
  description = "ALB HTTPS listener port"
  default     = 443
}


variable "app_container_image" {
  description = "App service container image with tag version"
  type        = string
  default     = "971778147356.dkr.ecr.us-east-1.amazonaws.com/made-by-me-app:1.0"
}


variable "db_name" {
  type        = string
  description = "Database name"
  default     = "MadeByMeExam"
}


variable "db_username" {
  type        = string
  description = "Database username"
}


variable "db_password" {
  type        = string
  description = "Data base user password"
}
