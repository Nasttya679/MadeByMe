
output "database_dns" {
  description = "Database DNS name"
  value       = aws_db_instance.postgres_db.endpoint
}


output "database_id" {
  description = "Postgres RDS ID"
  value       = aws_db_instance.postgres_db.id
}
