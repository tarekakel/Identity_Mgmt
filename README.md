# Identity Management Backend

Standalone .NET 8 API for identity management: users, roles, permissions, multi-tenancy, JWT and refresh tokens.

## Structure

- **IdentityManagement.Domain** – Entities (Tenant, User, Role, Permission, etc.) and domain interfaces
- **IdentityManagement.Application** – DTOs, application interfaces, paging/API response types, mapping functions
- **IdentityManagement.Infrastructure** – EF Core (MSSQL), repositories, JWT/refresh token service, password hashing, seeding
- **IdentityManagement.Api** – Web API, controllers, global exception handler, JWT auth, Seq logging

## Prerequisites

- .NET 8 SDK
- SQL Server or LocalDB (connection string in `appsettings.json`)
- Optional: [Seq](https://datalust.co/docs/seq) for log aggregation (configure `Seq:ServerUrl`)

## Run

```bash
cd src/IdentityManagement.Api
dotnet run
```

- API: http://localhost:5252 (or port in launchSettings)
- Swagger: http://localhost:5252/swagger

On first run, migrations are applied and the DB is seeded with:

- Tenant: **DEFAULT**
- Admin user: **admin@example.com** / **Admin@123**
- Permissions: users.read, users.write, roles.read, roles.write, tenants.manage, permissions.read

## Migrations

From solution root:

```bash
dotnet ef migrations add <MigrationName> --project src/IdentityManagement.Infrastructure --startup-project src/IdentityManagement.Api
dotnet ef database update --project src/IdentityManagement.Infrastructure --startup-project src/IdentityManagement.Api
```

## API Usage

1. **Login** (no auth):  
   `POST /api/auth/login`  
   Body: `{ "email": "admin@example.com", "password": "Admin@123", "tenantCode": "DEFAULT" }`  
   Returns `accessToken` and `refreshToken`.

2. **Tenant-scoped endpoints** (Users, Roles):  
   Send `Authorization: Bearer <accessToken>`. Tenant is taken from the JWT (`tenant_id` claim).  
   Or send header `X-Tenant-Id` (tenant Guid) when no token is used.

3. **Refresh token**:  
   `POST /api/auth/refresh`  
   Body: `{ "refreshToken": "<refresh_token>" }`

4. **Revoke token**:  
   `POST /api/auth/revoke` (with Bearer token)  
   Body: `{ "refreshToken": "<refresh_token>" }`

## Configuration

- **ConnectionStrings:DefaultConnection** – MSSQL connection string.
  - **LocalDB** (default): `Server=(localdb)\\mssqllocaldb;Database=IdentityManagement;Trusted_Connection=True;...` – works without a separate SQL Server (e.g. with Visual Studio / .NET SDK).
  - **SQL Server**: `Server=localhost,1433;Database=IdentityManagement;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;...` – use when SQL Server is running and listening on TCP port 1433 (start the service or Docker container and ensure TCP/IP is enabled).
- **Jwt:Secret** – At least 32 characters; used for signing tokens
- **Jwt:Issuer**, **Jwt:Audience** – Token validation
- **Seq:ServerUrl** – Seq server (e.g. http://localhost:5341); omit to disable Seq
