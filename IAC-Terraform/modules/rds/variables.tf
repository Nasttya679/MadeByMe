
variable "private_subnet_db_ids" {
  type        = list(string)
  description = "List of private subnets ID for database"
}


variable "db_sg_id" {
  type        = string
  description = "Database security group ID"
}


variable "db_name" {
  type        = string
  description = "Database name"
}


variable "db_username" {
  type        = string
  description = "Database username"
}


variable "db_password" {
  type        = string
  description = "Data base user password"
}
