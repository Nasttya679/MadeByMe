
output "alb_dns_name" {
  description = "Application load balancer DNS name"
  value       = "http://${module.alb.alb_dns_name}"
}


output "app_dns_name" {
  description = "MadeByMe App DNS URL"
  value       = "https://madebyme.trainee-keycloack.store"
}
