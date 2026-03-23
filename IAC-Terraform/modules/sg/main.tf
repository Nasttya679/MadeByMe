
resource "aws_security_group" "this" {
  for_each = var.security_groups

  name   = each.key
  vpc_id = var.main_vpc_id

  tags = {
    Name = each.key
  }
}


resource "aws_security_group_rule" "ingress_tcp_cidr" {

  for_each = {
    for item in flatten([
      for sg_name, sg in var.security_groups : [
        for port in sg.ingress_ports_tcp : {
          key     = "${sg_name}-tcp-${port}-cidr"
          sg_name = sg_name
          port    = port
          cidrs   = sg.allowed_cidr_blocks
        }
        if length(sg.allowed_cidr_blocks) > 0
      ]
    ]) : item.key => item
  }

  type              = "ingress"
  from_port         = each.value.port
  to_port           = each.value.port
  protocol          = "tcp"
  cidr_blocks       = each.value.cidrs
  security_group_id = aws_security_group.this[each.value.sg_name].id
}


resource "aws_security_group_rule" "ingress_tcp_sg" {

  for_each = {
    for item in flatten([
      for sg_name, sg in var.security_groups : [
        for port in sg.ingress_ports_tcp : [
          for source_sg in sg.allowed_security_groups : {
            key       = "${sg_name}-tcp-${port}-${source_sg}"
            sg_name   = sg_name
            port      = port
            source_sg = source_sg
          }
        ]
      ]
    ]) : item.key => item
  }

  type                     = "ingress"
  from_port                = each.value.port
  to_port                  = each.value.port
  protocol                 = "tcp"
  source_security_group_id = aws_security_group.this[each.value.source_sg].id
  security_group_id        = aws_security_group.this[each.value.sg_name].id
}


resource "aws_security_group_rule" "ingress_udp_cidr" {

  for_each = {
    for item in flatten([
      for sg_name, sg in var.security_groups : [
        for port in sg.ingress_ports_udp : {
          key     = "${sg_name}-udp-${port}-cidr"
          sg_name = sg_name
          port    = port
          cidrs   = sg.allowed_cidr_blocks
        }
        if length(sg.allowed_cidr_blocks) > 0
      ]
    ]) : item.key => item
  }

  type              = "ingress"
  from_port         = each.value.port
  to_port           = each.value.port
  protocol          = "udp"
  cidr_blocks       = each.value.cidrs
  security_group_id = aws_security_group.this[each.value.sg_name].id
}


resource "aws_security_group_rule" "ingress_udp_sg" {

  for_each = {
    for item in flatten([
      for sg_name, sg in var.security_groups : [
        for port in sg.ingress_ports_udp : [
          for source_sg in sg.allowed_security_groups : {
            key       = "${sg_name}-udp-${port}-${source_sg}"
            sg_name   = sg_name
            port      = port
            source_sg = source_sg
          }
        ]
      ]
    ]) : item.key => item
  }

  type                     = "ingress"
  from_port                = each.value.port
  to_port                  = each.value.port
  protocol                 = "udp"
  source_security_group_id = aws_security_group.this[each.value.source_sg].id
  security_group_id        = aws_security_group.this[each.value.sg_name].id
}


resource "aws_security_group_rule" "egress_all" {
  for_each = aws_security_group.this

  type              = "egress"
  from_port         = 0
  to_port           = 0
  protocol          = "-1"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = each.value.id
}