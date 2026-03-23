# MadeByMe Web Application

## Overview

MadeByMe is a multi-layered ASP.NET Core web application designed as an online marketplace where users can create, browse, and purchase handmade products.

The project follows Clean Architecture principles, separating concerns into distinct layers (Domain, Application, Infrastructure, Web, Tests), making the system scalable, maintainable, and testable.


## Architecture

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


## Technologies
- ASP.NET Core (.NET 8)
- Entity Framework Core
- PostgreSQL
- Razor Views (MVC)
- Dependency Injection


------------------------------------------------------

# Setup & Run

### Clone repository

``` bash
git clone <repo-url>
cd MadeByMe
```

## Local run

From root directory run

``` bash
dotnet restore
dotnet build
```

1. Before running application, make sure that PostgreSQL database is running localy on PC

2. Create migrations if not exist in folder - "Infrastrucure/Migrations/"

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



## Docker Compose

1. In root directory create ".env" file:
``` bash
POSTGRES_DB=MadeByMeExam
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres12345

ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://0.0.0.0:5000

DEFAULT_CONNECTION=Host=postgres;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD};
```

there are secret environment variables

2. From root directory run:

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



## Deploy to AWS ECS

## Make sure that you are login in AWS CLI, in AWS region is created S3 bucket, and Route53 domain name with HTTPS certificated already configured by hand or IAC!


### (Part 1) Create file with secret variables

1. In folder "IAC-Terraform/" create file "terraform.tfvars":
``` bash
db_username = "postgres"
db_password = "postgres12345"
```

this is credentials for AWS PostgreSQL RDS


### (Part 2) Build and push image to ECR

1. From root directory run:
``` bash
docker build -t made-by-me-app:1.0 .
```
this will build an application image

2. Login to AWS ECR(if ECR repository is private):

``` bash
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin 971778147356.dkr.ecr.us-east-1.amazonaws.com
```

3. In AWS console -> ECR - create private repository "made-by-me-app"

4. Tag Docker Image:
``` bash
docker tag made-by-me-app:1.0 971778147356.dkr.ecr.us-east-1.amazonaws.com/made-by-me-app:1.0
```

5. Push Image to AWS ECR:
``` bash
docker push 971778147356.dkr.ecr.us-east-1.amazonaws.com/made-by-me-app:1.0
```


### (Part 3) Deploy infrastructure and run app

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

4. In AWS console -> EC2 -> Load Balancers find "app-alb" and copy its ARN

5. In AWS console -> Route53 -> Hosted zones - find or create subdomain type A "madebyme.trainee-keycloack.store" and set ARN of current created Application Load Balancer "app-alb". Set Alias to that record and find or copy-paste ARN of this Application Load Balancer

6. After that application is sucure with  HTTPS certificates and is available by domain:

```bash
https://madebyme.trainee-keycloack.store
```