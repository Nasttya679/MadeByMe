
resource "aws_vpc" "main_vpc" {
  cidr_block           = var.cidr
  enable_dns_support   = true
  enable_dns_hostnames = true

  tags = {
    Name = "main-vpc"
  }
}


resource "aws_subnet" "public_subnets" {
  for_each = {
    for idx, cidr in var.public_subnets :
    idx => cidr
  }

  vpc_id                  = aws_vpc.main_vpc.id
  cidr_block              = each.value
  availability_zone       = element(var.available_zones_list, each.key % length(var.available_zones_list))
  map_public_ip_on_launch = true

  tags = {
    Name = "public-subnet-${each.key + 1}"
  }
}


resource "aws_subnet" "private_db_subnets" {
  for_each = {
    for idx, cidr in var.private_db_subnets :
    idx => cidr
  }

  vpc_id            = aws_vpc.main_vpc.id
  cidr_block        = each.value
  availability_zone = element(var.available_zones_list, each.key % length(var.available_zones_list))

  tags = {
    Name = "private-subnet-database-${each.key + 1}"
  }
}


resource "aws_internet_gateway" "main_igw" {
  vpc_id = aws_vpc.main_vpc.id

  tags = {
    Name = "main-igw"
  }
}


resource "aws_eip" "main_eip" {
  domain = "vpc"

  tags = {
    Name = "main-eip"
  }
}


resource "aws_nat_gateway" "main_nat" {
  allocation_id = aws_eip.main_eip.id
  subnet_id     = values(aws_subnet.public_subnets)[0].id
}


resource "aws_route_table" "main_public_route_table" {
  vpc_id       = aws_vpc.main_vpc.id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.main_igw.id
  }

  tags = {
    Name = "main-public-route-table"
  }
}


resource "aws_route_table_association" "public_assoc" {
  for_each       = aws_subnet.public_subnets

  subnet_id      = each.value.id
  route_table_id = aws_route_table.main_public_route_table.id
}


resource "aws_route_table" "main_private_route_table" {
  vpc_id = aws_vpc.main_vpc.id

  route {
    cidr_block     = "0.0.0.0/0"
    nat_gateway_id = aws_nat_gateway.main_nat.id
  }

  tags = {
    Name = "main-private-route-table"
  }
}


resource "aws_route_table_association" "private_db_assoc" {
  for_each       = aws_subnet.private_db_subnets

  subnet_id      = each.value.id
  route_table_id = aws_route_table.main_private_route_table.id
}
