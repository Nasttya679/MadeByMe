
output "security_group_ids" {
  value = {
    for name, sg in aws_security_group.this :
    name => sg.id
  }

  description = "Map of created security group IDs where key is the security group name"
}