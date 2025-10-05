# 📚 BookLend – Clean Architecture .NET 8 (Monolith + Event-Driven Design)

A **Book Lending Service** API built in **.NET 8**, designed using **Clean Architecture** principles within a **monolithic structure**, yet **event-driven** through AWS SQS.  
The system supports adding, listing, checking out, and returning books, with a strong focus on production-grade patterns: **idempotency**, **circuit breaker**, **caching**, and **infrastructure automation**.

---

## 🚀 Key Features

✅ **.NET 8 Minimal API** – Lightweight and fast minimal RESTful endpoints  
✅ **Clean Architecture** – Separation of concerns across Domain, Application, Infrastructure, and Presentation layers  
✅ **AWS DynamoDB** – Persistent, serverless data storage  
✅ **AWS SQS** – Event-driven design for notifications and decoupled communication  
✅ **AWS Redis (ElastiCache)** – High-speed caching with circuit breaker pattern  
✅ **Dockerized** – Fully containerized for local and cloud deployment  
✅ **Terraform IaC** – Automated provisioning of AWS infrastructure (DynamoDB, SQS, ECS, Redis, IAM Roles, etc.)  
✅ **CI/CD** – Automated build and deploy pipeline via **GitHub Actions**  
✅ **Idempotency Middleware** – Prevents duplicate POST/PUT requests  
✅ **Global Exception Middleware** – Standardized API responses  
✅ **TDD Approach** – Unit and integration tests validating use cases  
✅ **Environment Config Management** – Local dev config via `appsettings.json`, cloud config via environment variables  

---
## 🏗️ Architecture Overview

### 🧩 Clean Architecture Monolith (Event-Driven)

The solution follows **Clean Architecture** with the following boundaries:

| Layer | Responsibility |
|--------|----------------|
| **Domain** | Core entities (`Book`, `BookId`, `Isbn`) and domain events (`BookAdded`, `BookCheckedOut`, `BookReturned`) |
| **Application** | Use cases & business rules (Add, Checkout, Return, List books) |
| **Infrastructure** | AWS adapters for DynamoDB, SQS, Redis, and Idempotency storage |
| **Presentation** | Minimal API endpoints, exception middleware, and response standardization |

### 🧠 Event-Driven Extension (via AWS SQS)
Each key domain action (Add, Checkout, Return) emits an **SQS message** with structured event attributes.  
This enables future integrations (e.g., email notifications, analytics pipelines, audit systems) **without modifying core logic**.

### 🧰 Redis + Circuit Breaker Pattern
Redis is used for caching frequently accessed data (e.g., list of available books).  
A **circuit breaker** pattern protects the API from downstream cache failures — if Redis becomes unavailable, the system gracefully degrades to DynamoDB reads, maintaining service continuity.

### 🧾 Idempotency Layer
An **Idempotency Middleware** enforces that every POST/PUT request must contain an `Idempotency-Key` header.  
If a duplicate request is detected, the cached response is returned instead of reprocessing the action.  
This ensures consistent state in distributed deployments or retry scenarios.

---

## 🧱 Technology Stack

| Category | Technology |
|-----------|-------------|
| Language & Framework | .NET 8 (C#) Minimal API |
| Cloud & Services | AWS DynamoDB, AWS SQS, AWS ElastiCache (Redis) |
| Infrastructure as Code | Terraform |
| Messaging | AWS SQS FIFO Queue |
| Persistence | DynamoDB Local (for local dev) / DynamoDB AWS (for deployment) |
| Caching | AWS Redis (via StackExchange.Redis) |
| Build & Deploy | GitHub Actions (CI/CD) |
| Containerization | Docker & ECS (Fargate) |
| Patterns | CQRS, Idempotency, Circuit Breaker, Event-Driven |

---


## ⚙️ How to Run the App Locally

### 🧩 Prerequisites
Ensure you have the following installed and configured on your machine:

| Requirement | Description |
|--------------|-------------|
| [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download) | For building and running the API |
| [Docker & Docker Compose](https://docs.docker.com/get-docker/) | To run local dependencies (DynamoDB, Redis) |
| [Terraform CLI](https://developer.hashicorp.com/terraform/downloads) | For provisioning AWS resources |
| [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html) | For AWS authentication |

---

### 🧾 Step 1: Configure AWS CLI
Authenticate with your AWS account by running:

```bash
aws configure
````

Then provide:

* **AWS Access Key ID**
* **AWS Secret Access Key**
* **Default region** (e.g. `us-east-1`)
* **Output format** (e.g. `json`)

---

### 🧱 Step 2: Start Local Dependencies (DynamoDB + Redis)

From the root folder, run:

```bash
docker-compose up -d
```

This spins up the dependencies
---

### ⚙️ Step 3: Configure Local Settings

In your `appsettings.Development.json` file, verify all the configuration values are passed:


These settings ensure the app connects to local services during development.

---

### ▶️ Step 4: Run the Application

From the terminal:

```bash
dotnet run --project FictionalBookLending/FictionalBookLending
```

Then open the API documentation at:

```
https://localhost:44346/swagger/index.html
```

You can now:

* Add books (`POST /api/books`)
* List books (`GET /api/books`)
* Check out books (`POST /api/books/{id}/checkout`)
* Return books (`POST /api/books/{id}/return`)

---

## ☁️ How to Deploy to AWS

This project uses **Infrastructure as Code (IaC)** via **Terraform** to automate the creation of all necessary AWS resources.

### 🧩 Step 1: Provision AWS Infrastructure

Navigate to the Terraform folder:

```bash
cd iac/terraform
```

Then execute:

```bash
terraform init
terraform plan
terraform apply
```

This creates:

* **ECR Repository** (for Docker images)
* **ECS Cluster & Service (Fargate)**
* **DynamoDB Table** (for book and idempotency records)
* **Redis (ElastiCache)** for caching
* **SQS Queue** for event-driven messaging
* **IAM Roles & Policies**
* **CloudWatch Logs**

Once complete, Terraform outputs:

```
ecr_repository_url = <url>
ecs_cluster_name   = booklend-cluster
ecs_service_name   = booklend-service
redis_endpoint     = <url>
```

These values are automatically referenced in the deployment pipeline.

---

### ⚙️ Step 2: Configure CI/CD (GitHub Actions)

The file `.github/workflows/deploy.yml` handles:

1. Building the Docker image
2. Pushing the image to **Amazon ECR**
3. Updating the ECS Task Definition
4. Deploying to the **ECS Fargate Cluster**

#### Pipeline Trigger

The pipeline runs on:

* Push to `master`
* Manual workflow dispatch from GitHub UI

#### Environment Variables

```yaml
env:
  AWS_REGION: us-east-1
  ECR_REPOSITORY_NAME: demo-service
  ECS_CLUSTER: fiction_book-cluster
  ECS_SERVICE: fiction_book-service
```

To trigger manually:

```bash
gh workflow run deploy.yml
```

---

## 🧠 Important Design Decisions & Trade-Offs

### 1️⃣ Clean Architecture Monolith with Event-Driven Design

I used a **monolithic structure** but applied **Clean Architecture** boundaries.
All business logic resides in the `Application` layer, while infrastructure details (like AWS SQS or DynamoDB) are abstracted via interfaces.

This approach simplifies **deployment and observability**, while keeping it modular enough to evolve into microservices if needed.

---

### 2️⃣ Event-Driven via AWS SQS

All domain events (Book Added, Checked Out, Returned) are published to **AWS SQS**.
This allows new services (like notification or analytics processors) to subscribe asynchronously — achieving **decoupling and scalability** without introducing a full microservice architecture.

---

### 3️⃣ Idempotency Implementation

A custom **Idempotency Middleware** ensures that each POST/PUT request includes an `Idempotency-Key` header.
Requests with the same key are only processed once — preventing duplicate records during retries or network failures.

This is especially valuable in distributed systems where API calls may be retried automatically by clients or load balancers.

---

### 4️⃣ Redis with Circuit Breaker

Caching is powered by **AWS ElastiCache (Redis)** for speed and performance.
To ensure resilience, a **Circuit Breaker pattern** was implemented — so if Redis becomes unavailable, the app gracefully falls back to DynamoDB reads instead of failing requests.

This trade-off prioritizes **availability over consistency** for reads, which is ideal in high-traffic APIs.

---

### 5️⃣ Infrastructure as Code (IaC)

Every AWS resource (ECR, ECS, DynamoDB, Redis, SQS, IAM roles) is provisioned through **Terraform** — ensuring consistent, repeatable deployments.
This also simplifies version control and rollback processes.

---

### 6️⃣ CI/CD Pipeline via GitHub Actions

I implemented a full **GitHub Actions pipeline** that automates build, containerization, and deployment to AWS ECS.
This minimizes manual deployment errors and allows for continuous delivery on every merge to `master`.

---

### 7️⃣ Time Trade-Offs

I was given a **3-hour time constraint** for this task, but I chose to **trade some of that time** to achieve a more **production-grade design** that showcases real-world architectural best practices, including:

* Event-driven SQS integration
* Terraform IaC
* Circuit breaker and idempotency layers
* Proper exception handling and structured API responses

This trade-off ensures the solution is robust, extensible, and representative of modern cloud application standards.

---

### 8️⃣ Simplification Trade-Off

Although AWS Lambda and Step Functions could further decouple the workflow, I intentionally kept it monolithic for simplicity and faster iteration, while still maintaining **event-driven extensibility** for future scalability.

---

## 🧾 Summary

| Category            | Description                                              |
| ------------------- | -------------------------------------------------------- |
| **Architecture**    | Clean Architecture Monolith with Event-Driven extensions |
| **Persistence**     | AWS DynamoDB                                             |
| **Caching**         | AWS ElastiCache (Redis) with Circuit Breaker             |
| **Messaging**       | AWS SQS (FIFO + DLQ)                                     |
| **Infra**           | Terraform (IaC)                                          |
| **DevOps**          | Docker + GitHub Actions CI/CD                            |
| **Design Patterns** | Idempotency, Circuit Breaker, CQRS, Event-Driven         |
| **Testing**         | TDD across domain and application layers                 |

---

**✨ In short:**
BookLend demonstrates a **production-ready, cloud-native .NET 8 solution** that applies **Clean Architecture**, **event-driven design**, **AWS services**, and **modern DevOps practices** — deployable with a single `terraform apply` and continuously delivered through GitHub Actions.
