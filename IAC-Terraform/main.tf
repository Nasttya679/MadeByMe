
terraform {
  required_version = ">=1.5.0"

  backend "s3" {
    bucket   = "made-by-me-app-state-bucket"
    key      = "app/state"
    region   = "us-east-1"
    encrypt  = true
  }
}


provider "aws" {
  region = var.aws_region
}


module "vpc" {
  source = "./modules/vpc"

  cidr = "10.0.0.0/24"

  available_zones_list = [
    "us-east-1a",
    "us-east-1b"
  ]

  public_subnets = [
    "10.0.0.0/27",
    "10.0.0.32/27"
  ]

  private_db_subnets = [
    "10.0.0.64/27",
    "10.0.0.96/27"
  ]
}


module "sg" {
  source      = "./modules/sg"
  main_vpc_id = module.vpc.vpc_id

  security_groups = {

    alb_sg = {
      ingress_ports_tcp       = [80, 443]
      ingress_ports_udp       = []
      allowed_cidr_blocks     = ["0.0.0.0/0"]
      allowed_security_groups = []
    }

    app_sg = {
      ingress_ports_tcp       = [5000]
      ingress_ports_udp       = []
      allowed_cidr_blocks     = []
      allowed_security_groups = ["alb_sg"]
    }

    db_sg = {
      ingress_ports_tcp       = [5432]
      ingress_ports_udp       = []
      allowed_cidr_blocks     = []
      allowed_security_groups = ["app_sg"]
    }
  }
}


module "alb" {
  source                    = "./modules/alb"
  
  vpc_id                    = module.vpc.vpc_id
  public_subnet_ids         = module.vpc.public_subnet_ids

  alb_sg_ids                = [module.sg.security_group_ids["alb_sg"]]

  alb_listener_http_port    = var.alb_listener_http_port
  alb_listener_https_port   = var.alb_listener_https_port
  app_listener_port         = var.app_listener_port
}


module "iam" {
  source = "./modules/iam"
}


module "rds" {
  source                  = "./modules/rds"
  private_subnet_db_ids   = module.vpc.private_db_subnet_ids
  db_sg_id                = module.sg.security_group_ids["db_sg"]

  db_name                 = var.db_name
  db_username             = var.db_username
  db_password             = var.db_password
}


module "ecs" {
  source                          = "./modules/ecs"
  aws_region                      = var.aws_region
  ecs_task_execution_role_arn     = module.iam.ecs_main_exec_role_arn
  ecs_task_role_arn               = module.iam.ecs_task_role_arn

  public_subnet_ids               = module.vpc.public_subnet_ids
  app_sg_ids                      = [module.sg.security_group_ids["app_sg"]]
  app_tg_arn                      = module.alb.app_tg_arn
  listener_arn                    = module.alb.alb_listener_arn
  public_alb_dns_name             = module.alb.alb_dns_name

  app_container_image             = var.app_container_image
  app_container_port              = var.app_listener_port

  task_resource_sizes             = var.task_sizes
  app_task_resources              = "medium"
  database_id                     = module.rds.database_id

  app_default_connection_string   = "Host=${module.rds.database_dns};Port=5432;Database=${var.db_name};Username=${var.db_username};Password=${var.db_password};"
}


module "asp" {
  source                  = "./modules/asp"
  ecs_cluster_name        = module.ecs.ecs_cluster_name
  ecs_app_service_name    = module.ecs.ecs_app_service_name
}
