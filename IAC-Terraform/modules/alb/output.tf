
output "app_tg_arn" {
  description = "App target group ARN"
  value       = aws_lb_target_group.app_tg.arn
}


output "alb_listener_arn" {
  description = "ALB HTTP listener ARN"
  value       = aws_lb_listener.http_listener.arn
}


output "alb_dns_name" {
  description = "Application Load Balancer DNS name"
  value       = aws_lb.main_lb.dns_name
}
