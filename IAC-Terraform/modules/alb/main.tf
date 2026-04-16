
resource "aws_lb" "main_lb" {
  name               = "app-alb"
  load_balancer_type = "application"
  subnets            = var.public_subnet_ids
  security_groups    = var.alb_sg_ids
  internal           = false

  tags = {
    Name = "Main-ALB"
  }
}


resource "aws_lb_target_group" "app_tg" {
  name          = "app-tg"
  port          = var.app_listener_port
  protocol      = "HTTP"
  vpc_id        = var.vpc_id
  target_type   = "ip"

  health_check {
    path                = "/"
    matcher             = "200-299"
    interval            = 30
    timeout             = 5
    healthy_threshold   = 2
    unhealthy_threshold = 2
  }

  tags = {
    Name = "App-tg"
  }
}


resource "aws_lb_listener" "http_listener" {
  load_balancer_arn = aws_lb.main_lb.arn
  port              = var.alb_listener_http_port
  protocol          = "HTTP"


  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.app_tg.arn
  }



  # default_action {
  #   type = "redirect"

  #   redirect {
  #     port        = "443"
  #     protocol    = "HTTPS"
  #     status_code = "HTTP_301"
  #   }
  # }

  tags = {
    Name = "Main-ALB-listener"
  }
}


# resource "aws_lb_listener" "https_listener" {
#   load_balancer_arn  = aws_lb.main_lb.arn
#   port               = var.alb_listener_https_port
#   protocol           = "HTTPS"
#   ssl_policy         = "ELBSecurityPolicy-2016-08"

#   certificate_arn    = data.aws_acm_certificate.main_certificate.arn

#   default_action {
#     type             = "forward"
#     target_group_arn = aws_lb_target_group.app_tg.arn
#   }
# }


# resource "aws_lb_listener_rule" "madebyme_app_rule" {
#   listener_arn = aws_lb_listener.https_listener.arn
#   priority     = 10

#   action {
#     type             = "forward"
#     target_group_arn = aws_lb_target_group.app_tg.arn
#   }

#   condition {
#     host_header {
#       values = ["madebyme.trainee-keycloack.store"]
#     }
#   }
# }
