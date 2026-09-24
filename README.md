# EcomAPI

A production-grade **ASP.NET Core Web API** for an e-commerce application, built with **.NET 8**, **Entity Framework Core**, **SQL Server**, **JWT Authentication**, and several enterprise-oriented API practices.

The project demonstrates a clean, maintainable backend architecture suitable for real-world development and technical interview preparation.

---

## 🚀 Tech Stack

* .NET 8
* ASP.NET Core Web API
* C#
* Entity Framework Core
* SQL Server / LocalDB
* JWT Authentication
* AutoMapper
* FluentValidation
* Swagger / OpenAPI
* Serilog
* xUnit
* Moq
* API Versioning

---

# 🏗️ Architecture

The application follows a layered architecture:

```text
Client
   ↓
Controller
   ↓
Service
   ↓
Unit of Work
   ↓
Repository
   ↓
Entity Framework Core
   ↓
SQL Server
```

### Main Layers

```text
EcomAPI
│
├── Controllers
├── Services
│   ├── Interfaces
│   └── Implementations
├── Repositories
│   ├── Interfaces
│   └── Implementations
├── Entities
├── DTOs
├── Data
│   └── Configurations
├── Validators
├── Mapping
├── Middleware
├── Common
│   ├── Constants
│   ├── Exceptions
│   ├── Pagination
│   └── Responses
└── Logs
```

---

# ✨ Implemented Functionalities

## 1. User Registration

Users can register using their email and password.

During registration:

```text
Register
   ↓
Create User
   ↓
Hash Password
   ↓
Create Customer
   ↓
Database Transaction
   ↓
Commit
```

User and Customer creation are performed inside a database transaction to maintain consistency.

If the operation fails, the transaction is rolled back.

---

## 2. User Login

Users can authenticate using their email and password.

```text
Email + Password
       ↓
Find User
       ↓
Verify Password
       ↓
Generate JWT
       ↓
Return Token
```

Invalid credentials return:

```text
401 Unauthorized
```

The API does not expose the stored password hash.

---

# 🔐 Authentication & Authorization

JWT Bearer authentication is implemented.

JWT contains information such as:

* User ID
* Email
* Role

Supported roles:

```csharp
Admin
Customer
```

Roles are centralized using constants:

```csharp
Roles.Admin
Roles.Customer
```

---

## Role-Based Authorization

Product management operations are restricted to administrators.

### Customer

```text
GET Products        ✅
GET Product         ✅
Create Product      ❌
Update Product      ❌
Delete Product      ❌
```

### Admin

```text
GET Products        ✅
GET Product         ✅
Create Product      ✅
Update Product      ✅
Delete Product      ✅
```

Unauthorized users receive:

```text
401 Unauthorized
```

Authenticated users without the required role receive:

```text
403 Forbidden
```

---

# 📦 Product Management

The API supports:

* Create Product
* Get Product
* Get Products
* Update Product
* Soft Delete Product

Product deletion is implemented as a **soft delete**.

Instead of physically deleting the database record:

```text
IsActive = false
```

Inactive products are excluded from normal product listing queries.

---

# 🔎 Product Pagination

Product listing supports pagination.

Example:

```text
GET /api/v1/Products?pageNumber=1&pageSize=20
```

The API limits the maximum page size to:

```text
100
```

This prevents clients from requesting unnecessarily large result sets.

---

# 🔍 Product Filtering

Products can be filtered using:

* Search
* Minimum price
* Maximum price

Example:

```text
GET /api/v1/Products?search=phone&minPrice=500&maxPrice=5000
```

Filtering is performed at the database level.

---

# ↕️ Product Sorting

Products can be sorted by:

* Name
* Price
* Stock
* ID

Example:

```text
GET /api/v1/Products?sortBy=price&sortDescending=true
```

---

# ⚡ EF Core Query Optimization

Read-only queries use:

```csharp
AsNoTracking()
```

This avoids unnecessary EF Core change tracking.

DTO projection is also used:

```csharp
ProjectTo<ProductResponse>()
```

This allows AutoMapper to project required DTO fields directly at the database level instead of loading unnecessary entity data.

---

# 🗃️ Entity Relationships

The application demonstrates multiple EF Core relationships.

### One-to-One

```text
User
  │
  └── Customer
```

```text
Customer
   │
   └── CustomerProfile
```

### One-to-Many

```text
Customer
   │
   └── Orders
```

```text
Order
   │
   └── OrderItems
```

### Many-to-Many

```text
Product
   ↕
ProductCategory
   ↕
Category
```

---

# 🧩 Repository Pattern

Generic repository functionality is implemented through:

```text
IRepository<T>
      ↓
Repository<T>
```

Common operations include:

* Query
* GetById
* GetAll
* Find
* Add
* Update
* Delete
* Exists

Read-only operations use `AsNoTracking()` where appropriate.

---

# 🔄 Unit of Work

The application uses the Unit of Work pattern to coordinate repositories and database operations.

Example:

```text
UnitOfWork
 ├── Users
 ├── Customers
 ├── Products
 ├── Categories
 ├── Orders
 ├── OrderItems
 └── CustomerProfiles
```

Database changes are committed using:

```csharp
SaveChangesAsync()
```

Transactions are also supported through the Unit of Work.

---

# 🔄 Database Transactions

Registration is a good example.

Creating a user and customer must both succeed.

```text
Begin Transaction
      ↓
Create User
      ↓
Save User
      ↓
Create Customer
      ↓
Save Customer
      ↓
Commit
```

If any operation fails:

```text
Rollback
```

This prevents partially created user/customer data.

---

# 🔐 Password Hashing

Passwords are never stored as plain text.

The application uses a password hashing service:

```text
Password
   ↓
PasswordHasher
   ↓
PasswordHash
   ↓
Database
```

During login:

```text
Password
   ↓
Verify against PasswordHash
   ↓
Success / Failure
```

---

# 🛡️ Global Exception Handling

A centralized exception handler is implemented using:

```text
IExceptionHandler
```

The application handles exceptions such as:

```text
NotFoundException       → 404
ConflictException       → 409
UnauthorizedException   → 401
ForbiddenException      → 403
Other exceptions        → 500
```

Errors are returned using:

```text
ProblemDetails
```

Internal exception details are not exposed for `500` errors.

---

# 📊 Standard API Responses

Successful API responses use:

```csharp
ApiResponse<T>
```

Example:

```json
{
  "success": true,
  "message": "Product retrieved successfully.",
  "data": {
    "id": 1,
    "name": "Laptop"
  }
}
```

Error responses use ASP.NET Core `ProblemDetails`.

---

# 📝 Request Logging

Custom middleware logs every HTTP request.

Logged information includes:

* HTTP Method
* Request Path
* Status Code
* Execution Time
* Request ID

Example:

```text
HTTP GET /api/v1/Products responded 200 in 35 ms.
RequestId: ...
```

---

# 🆔 Request / Correlation ID

The application supports:

```text
X-Request-ID
```

If the client provides a request ID, it is reused.

Otherwise, the application's request identifier is used.

The same request ID is returned in the response header and included in logging.

This helps trace a request across application logs.

---

# 📁 File Logging with Serilog

Serilog is integrated with ASP.NET Core's `ILogger`.

The logging flow is:

```text
Application
     ↓
ILogger
     ↓
Serilog
     ↓
Log File
```

Logs are written to:

```text
logs/
```

with daily rolling files.

Example:

```text
logs/
└── log-20260924.txt
```

Log files are retained for a limited number of days.

---

# 🚦 Rate Limiting

ASP.NET Core built-in rate limiting is configured.

Current configuration:

```text
100 requests
per 1 minute
per client IP
```

Requests exceeding the limit receive:

```text
429 Too Many Requests
```

This helps protect the API from excessive requests.

---

# ❤️ Health Check

A database health check is configured.

Endpoint:

```text
GET /health
```

It verifies connectivity to the application database.

Example:

```text
Healthy
```

---

# 🔢 API Versioning

URL-based API versioning is implemented.

Example:

```text
/api/v1/Products
```

This allows future breaking changes to be introduced through a new API version while keeping older clients compatible.

Example:

```text
/api/v1/Products
/api/v2/Products
```

---

# 🧪 FluentValidation

Request validation is implemented using FluentValidation.

Validation is separated from controller/business logic.

```text
Request
   ↓
FluentValidation
   ↓
Valid → Continue
Invalid → 400 Bad Request
```

---

# 🔄 AutoMapper

AutoMapper is used to map between:

```text
Entity ↔ DTO
```

For example:

```text
CreateProductRequest
        ↓
     Product
        ↓
 ProductResponse
```

This keeps API contracts separate from database entities.

---

# 🧵 CancellationToken

Asynchronous database and service operations support:

```csharp
CancellationToken
```

This allows operations to be cancelled when the HTTP request is aborted.

---

# 🐢 Optimistic Concurrency

Product concurrency is handled using a SQL Server row-version column.

```text
Product
   ↓
RowVersion
```

This helps detect when multiple requests attempt to modify the same product concurrently.

---

# 🗄️ Database Migrations

EF Core migrations are used to maintain database schema changes.

Migration workflow:

```text
Entity Change
     ↓
Add-Migration
     ↓
Review Migration
     ↓
Build/Test
     ↓
Update-Database
```

Current database schema is managed through EF Core migrations.

---

# 📚 Swagger / OpenAPI

Swagger UI is enabled in the development environment.

It provides:

* API endpoint documentation
* Request/response documentation
* JWT authorization
* HTTP status codes
* XML documentation

Example:

```text
/swagger
```

JWT can be supplied through Swagger's **Authorize** button.

---

# 🧪 Unit Testing

A separate xUnit test project is used for important business logic.

Current tests focus on important ProductService behavior such as:

```text
Create Product
Delete Existing Product
Delete Non-existing Product
```

The goal is to test important business behavior rather than every line of code.

---

# 🔑 Configuration & Secrets

Application configuration is separated from sensitive secrets.

JWT configuration contains:

```text
Issuer
Audience
Expiry
```

The sensitive JWT signing key is kept outside `appsettings.json` during local development using **User Secrets**.

This prevents sensitive credentials from being committed to source control.

---

# 🛠️ Running the Project

## Prerequisites

* .NET 8 SDK
* Visual Studio 2022 or later
* SQL Server LocalDB
* SQL Server
* Git

---

## Database

The project uses SQL Server / LocalDB.

Example connection:

```text
(localdb)\MSSQLLocalDB
```

Database:

```text
ECommerceDb
```

---

## Apply Migrations

Run:

```powershell
Update-Database
```

or:

```bash
dotnet ef database update
```

---

## Run the API

From Visual Studio:

```text
Run → Start Debugging
```

or from the command line:

```bash
dotnet run
```

---

# 🔐 Sample Admin Account

The application seeds an administrator account through the database seeder.

Admin email:

```text
admin@ecom.com
```

The password is configured by the project's seeding configuration and should not be committed to source control.

---

# 📌 API Examples

## Authentication

```text
POST /api/v1/Auth/register
POST /api/v1/Auth/login
```

## Products

```text
GET    /api/v1/Products
GET    /api/v1/Products/{id}

POST   /api/v1/Products
PUT    /api/v1/Products/{id}
DELETE /api/v1/Products/{id}
```

## Health

```text
GET /health
```

---

# 🔒 HTTP Status Codes

The API uses standard HTTP status codes.

| Status Code | Meaning                                  |
| ----------- | ---------------------------------------- |
| 200         | Successful request                       |
| 201         | Resource created                         |
| 204         | Successful request with no response body |
| 400         | Invalid request                          |
| 401         | Authentication required/failed           |
| 403         | Insufficient permissions                 |
| 404         | Resource not found                       |
| 409         | Conflict                                 |
| 429         | Rate limit exceeded                      |
| 500         | Unexpected server error                  |

---

# 🎯 Project Goals

This project demonstrates practical implementation of:

* RESTful Web APIs
* Clean layered architecture
* Authentication and authorization
* Database design
* EF Core
* Repository and Unit of Work patterns
* Transactions
* Validation
* Exception handling
* Logging
* API versioning
* Rate limiting
* Health monitoring
* Concurrency handling
* Pagination
* Filtering and sorting
* API documentation
* Unit testing

The project intentionally focuses on practical production-grade backend concepts without introducing unnecessary complexity.
