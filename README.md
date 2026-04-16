# MultiTenantManagement

A modular ASP.NET Core 8 Web API for managing multi-tenant storefronts, including tenant administration, user/role management, product catalogs, order workflows, and file attachments.

## Overview

**MultiTenantManagement** is a backend system designed for applications where multiple tenants operate within the same platform while keeping their data and administration isolated. It provides a role-based API for:

- managing tenants and tenant settings
- provisioning tenant users with scoped access
- maintaining tenant-specific product catalogs
- creating and processing tenant orders
- uploading tenant-scoped attachments

### Problem It Solves

In SaaS and marketplace-style systems, organizations often need to share a platform without sharing data or administrative control. This project addresses that by combining:

- tenant-aware authorization
- role-based access control
- modular service organization
- tenant-scoped business workflows

### Who It Is For

- Backend developers building multi-tenant SaaS systems
- Teams learning layered ASP.NET Core architecture
- Recruiters/interviewers evaluating a practical .NET backend project
- Product teams that need tenant, catalog, and order management APIs

## 🏗️ Architecture

The solution follows a layered architecture with clear separation between API, business logic, domain concerns, and persistence.

### High-Level Design

```text
Client / Frontend / Swagger
          |
          v
ASP.NET Core API Controllers
          |
          v
Infrastructure Services
(Authentication, Tenant, Users, Products, Orders, Attachments)
          |
          v
Data Layer (EF Core + Identity + PostgreSQL)
          |
          v
Core Layer (Enums, interfaces, shared abstractions)
```

### Layers

| Layer | Responsibility |
|---|---|
| `MultiTenantManagement.API` | HTTP entry point, controller routing, authentication/authorization setup, Swagger, DI configuration |
| `MultiTenantManagement.Infrastructure` | Application/service logic, DTOs, mapping, JWT creation, authorization handlers, storage services |
| `MultiTenantManagement.Data` | Entity Framework Core context, entity models, migrations, seed data |
| `MultiTenantManagement.Core` | Shared abstractions and enums such as tenant and order status |

### Technologies Used

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core 8
- ASP.NET Core Identity
- JWT Bearer Authentication
- PostgreSQL via `Npgsql`
- AutoMapper
- Swagger / OpenAPI
- Amazon S3 SDK for optional cloud file storage
- Local static file storage via `wwwroot/uploads`

### Multi-Tenancy Approach

This project uses application-level multi-tenancy:

- tenant identity is stored on users and business entities
- JWT tokens include a `tenant_id` claim for tenant-scoped users
- a custom `TenantAccess` authorization policy validates route `tenantId` against the authenticated user’s tenant
- `SystemAdmin` users can bypass tenant restrictions for system-wide administration

## ✨ Features

- JWT-based authentication with role claims
- Seeded system administrator account
- Tenant CRUD with soft delete
- Tenant store settings support
- Role-based user management (`SystemAdmin`, `TenantAdmin`, `User`)
- Tenant-scoped user access restrictions
- Product catalog CRUD per tenant
- Public product listing and product detail endpoints
- Order creation with server-side pricing calculation
- Order approval, rejection, and cancellation workflow
- Order status history tracking
- Attachment upload with validation and metadata persistence
- Local file storage with optional Amazon S3 support
- Swagger UI in development
- Dockerfile for containerized deployment

## 🧰 Tech Stack

### Backend

- ASP.NET Core 8 Web API
- ASP.NET Core Identity
- JWT Bearer Authentication
- AutoMapper

### Database

- PostgreSQL
- Entity Framework Core
- EF Core Migrations

### Storage

- Local file storage (`wwwroot/uploads`)
- Amazon S3 (optional)

### Tools & Libraries

- Swashbuckle / Swagger
- Npgsql EF Core provider
- AWSSDK.S3
- Visual Studio solution structure
- Docker

### Frontend

No frontend application is included in this repository.  
The API is intended to be consumed by an external web/mobile frontend or tested through Swagger/Postman.

## 📂 Project Structure

```text
MultiTenantManagement/
└── MultiTenantManagement.API/
    ├── MultiTenantManagement.API/              # ASP.NET Core Web API project
    │   ├── Controllers/                        # HTTP endpoints
    │   ├── Properties/                         # Launch settings
    │   ├── wwwroot/uploads/                    # Local uploaded files
    │   ├── Program.cs                          # App startup and DI configuration
    │   └── appsettings*.json                   # Runtime configuration
    │
    ├── MultiTenantManagement.Core/             # Shared interfaces and enums
    │   ├── Enums/
    │   ├── Extensions/
    │   └── Interfaces/
    │
    ├── MultiTenantManagement.Data/             # Persistence layer
    │   ├── Models/                             # EF Core entities
    │   ├── Migrations/                         # EF Core migration history
    │   ├── AppDbContext.cs                     # Database context
    │   └── SeedData.cs                         # Roles + SystemAdmin seeding
    │
    ├── MultiTenantManagement.Infrastructure/   # Business/application services
    │   ├── Auth/                               # Tenant access policy handler
    │   ├── Features/
    │   │   ├── Authentication/
    │   │   ├── Tenant/
    │   │   ├── Users/
    │   │   ├── Product/
    │   │   ├── Order/
    │   │   └── Attachment/
    │   ├── Helpers/                            # JWT, current user helpers, pagination
    │   ├── Mapper/                             # AutoMapper profile
    │   └── Storage/                            # Local/S3 file storage abstractions
    │
    ├── API_REFERENCE.txt                       # Generated API reference
    ├── Dockerfile                              # Container build file
    └── MultiTenantManagement.API.sln           # Solution file
```

## 🔌 API Overview

> Base URL in local development is typically `http://localhost:5178` or `https://localhost:7114`.

### Authentication

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/Authentication/Login` | Authenticate user and return JWT |
| `GET` | `/api/Authentication/GetUserTenant` | Get current authenticated user’s tenant |

### Tenants

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/Tenants/GetTenants` | Paged tenant listing (`SystemAdmin`) |
| `GET` | `/api/Tenants/Tenant/{tenantId}` | Get tenant by ID |
| `POST` | `/api/Tenants/CreateTenant` | Create a tenant |
| `PUT` | `/api/Tenants/UpdateTenant/{tenantId}` | Update tenant metadata |
| `DELETE` | `/api/Tenants/Tenant/{tenantId}` | Soft delete tenant |

### Users

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/Users/GetUsers` | List users |
| `GET` | `/api/Users/Users/{userId}` | Get user by ID |
| `POST` | `/api/Users/CreateUser` | Create user and assign role |
| `PUT` | `/api/Users/UpdateUser/{userId}` | Update user |
| `DELETE` | `/api/Users/DeleteUser/{userId}` | Soft delete user |

### Products

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/tenants/{tenantId}/products` | Public paged product list |
| `GET` | `/api/tenants/{tenantId}/products/{id}` | Public product details |
| `POST` | `/api/tenants/{tenantId}/products` | Create product (`TenantAdmin`) |
| `PUT` | `/api/tenants/{tenantId}/products/{id}` | Update product |
| `DELETE` | `/api/tenants/{tenantId}/products/{id}` | Soft delete product |

### Orders

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/tenants/{tenantId}/orders` | Create order |
| `GET` | `/api/tenants/{tenantId}/orders` | List tenant orders (`TenantAdmin`,`SystemAdmin`) |
| `GET` | `/api/tenants/{tenantId}/orders/CustomerOrders` | List current customer’s own orders |
| `GET` | `/api/tenants/{tenantId}/orders/{orderId}` | Get order details |
| `POST` | `/api/tenants/{tenantId}/orders/{orderId}/approve` | Approve order |
| `POST` | `/api/tenants/{tenantId}/orders/{orderId}/reject` | Reject order |
| `POST` | `/api/tenants/{tenantId}/orders/{orderId}/cancel` | Cancel order |

### Attachments

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/tenants/{tenantId}/Attachments/upload` | Upload file |
| `GET` | `/api/tenants/{tenantId}/Attachments/{id}` | Get attachment metadata |

### Example: Login Request

```http
POST /api/Authentication/Login
Content-Type: application/json
```

```json
{
  "email": "admin@system.com",
  "password": "Admin@12345"
}
```

### Example: Login Response

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAtUtc": "2026-04-04T13:00:00Z",
  "email": "admin@system.com",
  "userRole": "SystemAdmin",
  "tenantId": null,
  "fullName": null
}
```

### Example: Create Order Request

```json
{
  "customerId": "00000000-0000-0000-0000-000000000000",
  "deliveryAddress": "123 Main Street",
  "totalAmount": 0,
  "items": [
    {
      "productId": "00000000-0000-0000-0000-000000000000",
      "quantity": 2,
      "unitPrice": 0
    }
  ]
}
```

> `totalAmount` and item `unitPrice` are recalculated server-side from product pricing.

## 🚀 Getting Started

### Prerequisites

- .NET SDK 8
- PostgreSQL
- Git
- Optional: Docker

### 1. Clone the Repository

```bash
git clone <repository-url>
cd MultiTenantManagement
```

### 2. Restore Dependencies

```bash
dotnet restore MultiTenantManagement.API/MultiTenantManagement.API.sln
```

### 3. Configure the Database and App Settings

Update `MultiTenantManagement.API/MultiTenantManagement.API/appsettings.json` or override via environment variables.

#### Required Settings

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=MultiTenantDb3;Username=postgres;Password=your_password"
  },
  "Jwt": {
    "Key": "your-strong-secret-key",
    "Issuer": "MultiTenantManagement",
    "Audience": "MultiTenantManagement",
    "ExpiresMinutes": 60
  },
  "Seed": {
    "SystemAdmin": {
      "Email": "admin@system.com",
      "Password": "Admin@12345"
    }
  },
  "Storage": {
    "UseS3": false
  }
}
```

#### Optional S3 Settings

```json
{
  "S3Storage": {
    "BucketName": "your-bucket",
    "Region": "us-east-1",
    "AccessKey": "your-access-key",
    "SecretKey": "your-secret-key",
    "PublicBaseUrl": ""
  }
}
```

### 4. Apply Database Migrations

```bash
dotnet ef database update --project MultiTenantManagement.API/MultiTenantManagement.Data --startup-project MultiTenantManagement.API/MultiTenantManagement.API
```

### 5. Run the API

```bash
dotnet run --project MultiTenantManagement.API/MultiTenantManagement.API
```

### 6. Open Swagger

Development launch settings expose Swagger at:

- `https://localhost:7114/swagger`
- `http://localhost:5178/swagger`

### 7. Run with Docker

```bash
docker build -f MultiTenantManagement.API/Dockerfile -t multitenantmanagement .
docker run -p 10000:10000 multitenantmanagement
```

## 🧭 Usage

A typical workflow looks like this:

1. Login as the seeded `SystemAdmin`
2. Create a new tenant
3. Create one or more users for that tenant
4. Assign roles such as `TenantAdmin` or `User`
5. Upload tenant assets if needed
6. Create tenant products
7. Let authenticated users create orders within their tenant
8. Approve or reject orders as `TenantAdmin`
9. Allow end users to view or cancel their own orders when permitted

### Roles

| Role | Main Capabilities |
|---|---|
| `SystemAdmin` | Cross-tenant administration, tenant creation, user oversight |
| `TenantAdmin` | Manage users/products within a single tenant, approve/reject orders |
| `User` | Place and view own orders, cancel eligible orders |

### Order Workflow

```text
PendingApproval -> Approved
PendingApproval -> Rejected
PendingApproval -> Cancelled
Approved -> Cancelled
```

Each transition is stored in order status history for traceability.

## 🖥️ Screens / Demo

This repository does not include a frontend UI, but it does include:

- Swagger UI for interactive API exploration
- HTTP endpoints structured for external frontend consumption
- Static upload support through `wwwroot/uploads` when local storage is enabled

If you want to demo the project quickly, Swagger is the primary interface.

## 🧠 Key Design Decisions

### 1. Layered Separation

The solution separates API, business logic, shared abstractions, and data access into distinct projects. This keeps the codebase easier to maintain and extend.

### 2. Application-Level Multi-Tenancy

Tenant isolation is handled explicitly in business entities, JWT claims, and authorization rules rather than through separate databases per tenant.

### 3. Role + Policy Security

The system combines:

- ASP.NET Core Identity roles
- JWT bearer authentication
- custom `TenantAccess` policy

This provides both role-based and tenant-scoped authorization.

### 4. Soft Deletes

Tenants, users, and products are soft deleted instead of physically removed, which is useful for auditability and safer administration.

### 5. Pluggable File Storage

Attachments are abstracted behind `IFileStorageService`, allowing the app to switch between:

- local storage
- Amazon S3

without changing controller or service behavior.

### 6. Server-Controlled Pricing for Orders

Order totals and item prices are computed from stored product data, which helps prevent client-side price tampering.

## 🔮 Future Improvements

- Add a dedicated frontend dashboard
- Introduce refresh tokens and logout/session management
- Add global exception handling for cleaner API error responses
- Add automated tests for services and controllers
- Add audit logging for admin actions
- Implement stricter tenant query filters at the data layer
- Add payment processing endpoints to match the existing payment entity
- Improve environment configuration by moving sensitive defaults out of `appsettings.json`

## 👤 Author

**abdalkareem**  
`abdelkarimshar99@gmail.com`

## 📄 Notes

- The solution targets .NET 8
- Swagger is enabled in Development
- The repository currently contains backend/API code only
- The project includes migrations and seed logic for initial setup
- A local build attempt in the current environment surfaced a package audit warning for `AutoMapper 16.0.0`, so dependency review would be a good next maintenance step
