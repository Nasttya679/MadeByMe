
resource "aws_iam_role" "ecsTaskExecutionRole" {
  name               = "EKS-execution-role"
  assume_role_policy = "${data.aws_iam_policy_document.assume_role_policy.json}"
}


resource "aws_iam_role_policy_attachment" "ecsTaskExecutionRole_policy" {
  role       = "${aws_iam_role.ecsTaskExecutionRole.name}"
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}


resource "aws_iam_role" "ecsTaskRole" {
  name = "EKS-task-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17",
    Statement = [{
      Effect = "Allow",
      Principal = {
        Service = "ecs-tasks.amazonaws.com"
      },
      Action = "sts:AssumeRole"
    }]
  })
}


resource "aws_iam_role_policy" "ecs_exec_policy" {
  name = "ecs-exec-policy"
  role = aws_iam_role.ecsTaskRole.id

  policy = jsonencode({
    Version = "2012-10-17",
    Statement = [
      {
        Effect = "Allow",
        Action = [
          "ssmmessages:CreateControlChannel",
          "ssmmessages:CreateDataChannel",
          "ssmmessages:OpenControlChannel",
          "ssmmessages:OpenDataChannel",
          "ssm:UpdateInstanceInformation",
          "ec2messages:*"
        ],
        Resource = "*"
      }
    ]
  })
}
