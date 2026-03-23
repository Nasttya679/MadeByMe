
variable "main_vpc_id" {
  type        = string
  description = "ID of main VPC"
}


variable "security_groups" {
  description = "Security groups configuration map"

  type = map(object({
    ingress_ports_tcp        = list(number)
    ingress_ports_udp        = list(number)
    allowed_cidr_blocks      = list(string)
    allowed_security_groups  = optional(list(string), [])
  }))
}