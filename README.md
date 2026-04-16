# MadeByMe Web Application



# Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Technologies](#technologies)
- [Setup & Run](#setup--run)
  - [Local Run](#1-local-run)
  - [Docker Compose](#2-docker-compose)
  - [Deploy to AWS ECS](#3-deploy-to-aws-ecs)
- [Automation](#automation)
  - [Cloud Automation](#cloud-automation)
  - [Local Automation](#local-automation)


------------------------------------------------------


# Overview

MadeByMe is a multi-layered ASP.NET Core web application designed as an online marketplace where users can create, browse, and purchase handmade products

The project follows Clean Architecture principles, separating concerns into distinct layers (Domain, Application, Infrastructure, Web, Tests), making the system scalable, maintainable, and testable


------------------------------------------------------


# Architecture

``` bash
MadeByMe/
│
├── Domain              Core business entities
├── Application         Business logic(services, DTOs, interfaces)
├── Infrastructure      Data access (EF Core, repositories)
├── Web                 ASP.NET Core MVC (controllers, views)
├── Tests               Unit tests
├── IAC-Terraform       Infrastructure deployed to AWS by Terraform IAC
```

1. Domain Layer (Contains core business models)

```
- Entities/
```


2. Application Layer (Contains business logic and contracts)

```
- Services/
│
├── Implementation/              
├── Interfaces/
|
- DTOs/      
|
- ViewModels/   
```

3. Infrastructure Layer (Handles data access)

```
- Repositories/
│
├── Implementation/              
├── Interfaces/
|
- Data/
| 
| 
- Migrations/
```


4. Web Layer (Presentation layer)

```
- Controllers/
|
- Views/
| 
- wwwroot/
|
- Properties/
|
|
- appsettings.json
- Program.cs
```

------------------------------------------------------


# Technologies
- ASP.NET Core (.NET 8)
- Entity Framework Core
- PostgreSQL
- Razor Views (MVC)
- Dependency Injection


------------------------------------------------------

# Setup & Run

### Clone repository

``` bash
git clone https://github.com/Nasttya679/MadeByMe.git
cd MadeByMe
```

## 1. Local run

Run from the root directory:

``` bash
dotnet restore
dotnet build
```

1. Before running the application, make sure that a PostgreSQL database is running locally on your PC

  - The database must be created using the same credentials as defined in `Web/appsettings.json`:

  ```bash
  Host=localhost;Port=5432;Database=MadeByMeExam;Username=postgres;Password=postgres12345
  ```

  - Alternatively, you can update the connection string in `Web/appsettings.json` to match the credentials of your existing local PostgreSQL setup

2. Create migrations if not exist in folder - `Infrastrucure/Migrations/`:

```bash
dotnet ef migrations add Init --project Infrastructure --startup-project Web
```

and run: 

```bash
dotnet ef database update --project Infrastructure --startup-project Web
```

3. Run Application:
```bash
cd Web
dotnet run
```

In this case application listening on: 
```bash
http://localhost:5213
```
but check the logs in console to be confident



## 2. Docker Compose

1. In root directory create `.env` file:
``` bash
POSTGRES_DB=MadeByMeExam
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres12345

ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://0.0.0.0:5000

DEFAULT_CONNECTION=Host=postgres;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD};
```

there are secret environment variables

2. Run from the root directory:

``` bash
docker compose up --build -d
```
this will pull postgres image, build application images if not exists, and run application and postgres database

3. In this case application listening on: 
```bash
http://localhost:5000
```
but check list of containers to be confident:

```bash
docker ps -a 
```



## 3. Deploy to AWS ECS

## Prerequisites

Before running this project, ensure the following requirements are met:

- AWS CLI is installed and configured with user credentials on your local PC
  - Configuration can be verified using: `aws sts get-caller-identity`
  - This command returns information about the currently authenticated AWS account (user/role), helping confirm that your CLI is properly configured

- Terraform is installed on your local PC

- Terraform state is stored in an S3 backend:
  ```hcl
  terraform {
    required_version = ">=1.5.0"

    backend "s3" {
      bucket  = "made-by-me-app-state-bucket"
      key     = "app/state"
      region  = "us-east-1"
      encrypt = true
    }
  }
  ```
    - In this project, the default S3 bucket used for Terraform state is: `made-by-me-app-state-bucket`

    - However, this bucket is not strictly required: 
      - You can create a new S3 bucket if needed
      - It is recommended to create it in the same region where the application is deployed
      - Then update the bucket field in the Terraform backend configuration accordingly

- A Route53 domain is configured (used for accessing the application via a custom domain):
  - `trainee-keycloack.store`

- HTTPS certificate is already set up (manually or via IaC in AWS Certificate Manager):
  - The certificate is issued and valid for `trainee-keycloack.store`
  - The certificate must include the following tag:
    - `Name = "Main-domain-certificate"`
  - This is required because in this case Terraform uses this tag to locate the correct ACM certificate via a data source lookup


## (Part 1) Create file with secret variables

1. In folder `IAC-Terraform/` create file `terraform.tfvars`:
``` bash
db_username = "postgres"
db_password = "postgres12345"
```

there are credentials for AWS PostgreSQL RDS


## (Part 2) Build and push image to ECR

## Tagging strategy (important)

It is recommended to always version Docker images using semantic tags instead of relying only on `latest`

Example format:
- `1.0`
- `1.0.1`
- `1.1`
- `2.0`

This helps to:
- Track deployments in production
- Roll back to previous versions easily
- Avoid overwriting images unintentionally

## Recommended workflow

1. Run from the root directory::
``` bash
docker build -t made-by-me-app:1.0 .
```
this will build an application image version `1.0`

2. Login to AWS ECR, if ECR repository is private:

``` bash
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin 971778147356.dkr.ecr.us-east-1.amazonaws.com
```

3. In AWS console -> ECR - create private repository `made-by-me-app`

4. Tag Docker Image:
``` bash
docker tag made-by-me-app:1.0 971778147356.dkr.ecr.us-east-1.amazonaws.com/made-by-me-app:1.0
```

5. Push Image to AWS ECR:
``` bash
docker push 971778147356.dkr.ecr.us-east-1.amazonaws.com/made-by-me-app:1.0
```

## Deploying a specific application version (important)

### If you want to deploy a specific version of the application, you can control it via Terraform variables:

1. In AWS console -> ECR -> Private registry -> Repositories -> `made-by-me-app` check exist versions of the application

2. Find such variable in root `variables.tf` file:

```hcl id="t1x8p4"
variable "app_container_image" {
  description = "App service container image with tag version"
  type        = string
  default     = "971778147356.dkr.ecr.us-east-1.amazonaws.com/made-by-me-app:2.0"
}
```

3. Change only the image tag in the Docker image reference. The image must already exist in ECR. For example, update only the version part of the image:

  - From:
  ```hcl id="t1x8p4"
  default = "971778147356.dkr.ecr.us-east-1.amazonaws.com/made-by-me-app:2.0"
  ```
  - To:

  ```hcl id="t1x8p4"
  default = "971778147356.dkr.ecr.us-east-1.amazonaws.com/made-by-me-app:1.0.1"
  ```



## (Part 3) Deploy infrastructure and run app

1. From root directory change path:
``` bash
cd IAC-Terraform
```


2. Initialize terraform project:
``` bash
terraform init
```


3. Plan and Apply infrastrucure:
``` bash
terraform plan
terraform apply
```

## Configure AWS resources (important)

In AWS Console:

  1. Go to EC2 -> Load Balancers:
      - Find Application Load Balancer named `app-alb`
      - Copy its DNS name or use it as a target for Route53 Alias record

  2. Go to Route53 -> Hosted Zones:
      - Find hosted zone for `trainee-keycloack.store`

  3. Create (or update) an A record:
      - Name: `madebyme.trainee-keycloack.store`
      - Type: A (Alias)
      - Alias: enabled
      - Target: select Application Load Balancer `app-alb`


After all, application is secured with TLS and is accessible via HTTPS:

```bash
https://madebyme.trainee-keycloack.store
```

------------------------------------------------------


# Automation

All automation commands are managed from the `Automation/` directory

This folder contains a `Makefile` that provides simplified commands for local development and cloud deployment workflows

From the project root directory, move to Automation:

```bash
cd Automation
```

## Prerequisites (important)

Before using this automation, ensure that all previous requirements have been completed, specifically:

- AWS CLI is installed and configured on your local PC
- An Amazon Web Services S3 bucket already exists: made-by-me-app-state-bucket (with the correct region configured, default is us-east-1)
- An Amazon Route 53 domain is configured
- An HTTPS certificate is already set up (manually or via IaC)
- All required secret configuration files have already been created as described in the previous sections
- `Make` is installed and available in PC system PATH


## Cloud Automation

#### These commands are used for AWS deployment, Terraform infrastructure management, and Docker image publishing


### Push new release to AWS ECR
Builds the Docker image, logs in to AWS ECR, tags the image, pushes it to the private repository, and removes local temporary images after completion:

```bash
make push-image
```

Deploy a specific version (this VERSION is a docker tag):
```bash
make push-image VERSION=2.0
```

### Deploy cloud infrastructure
Runs Terraform deployment for the application infrastructure:

```bash
make deploy-app
```

### Destroy cloud infrastructure
Removes deployed AWS infrastructure using Terraform:

```bash
make destroy-app
```


### Show Terraform execution plan
Displays planned infrastructure changes without applying them:

```bash
make show-plan
```

## Local Automation

These commands are used for local Docker Compose development


### Start local environment
Builds containers and starts the local application stack in detached mode:

```bash
make local-deploy
```

### Stop local environment
Stops and removes local Docker Compose containers:

```bash
make local-destroy
```
