

resource "aws_appautoscaling_target" "app_scaling_target" {
  max_capacity       = 3
  min_capacity       = 1

  resource_id        = "service/${var.ecs_cluster_name}/${var.ecs_app_service_name}"
  scalable_dimension = "ecs:service:DesiredCount"
  service_namespace   = "ecs"
}


resource "aws_appautoscaling_policy" "app_cpu_scaling" {
  name               = "app-cpu-scaling"
  policy_type        = "TargetTrackingScaling"

  resource_id        = aws_appautoscaling_target.app_scaling_target.resource_id
  scalable_dimension = aws_appautoscaling_target.app_scaling_target.scalable_dimension
  service_namespace  = aws_appautoscaling_target.app_scaling_target.service_namespace

  target_tracking_scaling_policy_configuration {
    target_value = 70

    predefined_metric_specification {
      predefined_metric_type = "ECSServiceAverageCPUUtilization"
    }

    scale_in_cooldown  = 120
    scale_out_cooldown = 60
  }
}


resource "aws_appautoscaling_policy" "app_memory_scaling" {
  name               = "app-memory-scaling"
  policy_type        = "TargetTrackingScaling"

  resource_id        = aws_appautoscaling_target.app_scaling_target.resource_id
  scalable_dimension = aws_appautoscaling_target.app_scaling_target.scalable_dimension
  service_namespace  = aws_appautoscaling_target.app_scaling_target.service_namespace

  target_tracking_scaling_policy_configuration {
    target_value = 70

    predefined_metric_specification {
      predefined_metric_type = "ECSServiceAverageMemoryUtilization"
    }

    scale_in_cooldown  = 180
    scale_out_cooldown = 60
  }
}
