
resource "aws_ecs_cluster" "main_cluster" {
  name = "MadeByMeApp-cluster"
}


resource "aws_ecs_task_definition" "app_task" {
  family                   = "app-task"
  requires_compatibilities = ["FARGATE"]
  network_mode             = "awsvpc"
  cpu                      = var.task_resource_sizes[var.app_task_resources].cpu
  memory                   = var.task_resource_sizes[var.app_task_resources].memory
  execution_role_arn       = var.ecs_task_execution_role_arn
  task_role_arn            = var.ecs_task_role_arn

  container_definitions = jsonencode([
    {
      name      = "app-ui"
      image     = var.app_container_image
      essential = true

      portMappings = [
        {
          containerPort = var.app_container_port
          protocol      = "tcp"
        }

      ]

      environment = [
        {
          name  = "ASPNETCORE_ENVIRONMENT"
          value = "Production"
        },

        {
          name  = "DEFAULT_CONNECTION"
          value = var.app_default_connection_string
        }, 

        {
          name  = "ASPNETCORE_URLS"
          value = "http://0.0.0.0:5000"
        }
      ]
    }
  ])

  tags = {
    Name = "App-task"
  }
}


resource "aws_ecs_service" "app_service" {
  name                    = "app-service"
  cluster                 = aws_ecs_cluster.main_cluster.id
  task_definition         = aws_ecs_task_definition.app_task.arn
  launch_type             = "FARGATE"
  enable_execute_command  = true
  desired_count           = 1

  network_configuration {
    subnets          = var.public_subnet_ids
    security_groups  = var.app_sg_ids
    assign_public_ip = true
  }

  load_balancer {
    container_name   = "app-ui"
    container_port   = var.app_container_port
    target_group_arn = var.app_tg_arn
  }

  depends_on = [var.listener_arn, var.database_id]

  tags = {
    Name = "App-service"
  }
}
