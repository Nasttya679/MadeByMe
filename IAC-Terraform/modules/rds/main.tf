
resource "aws_db_subnet_group" "db_subnets" {
  name       = "main-db-subnet-group"
  subnet_ids = var.private_subnet_db_ids

  tags = {
    Name = "database-subnet-group"
  }
}


resource "aws_db_instance" "postgres_db" {
  allocated_storage           = 10
  engine                      = "postgres"
  instance_class              = "db.t3.micro"
  engine_version              = "15.17"
  db_name                     = var.db_name
  username                    = var.db_username
  password                    = var.db_password
  publicly_accessible         = false
  vpc_security_group_ids      = [var.db_sg_id]
  db_subnet_group_name        = aws_db_subnet_group.db_subnets.name
  skip_final_snapshot         = true
  multi_az                    = true
}
