# WellTimes Appointment Booking System

A simple microservices-based online appointment booking system built with **ASP.NET Core (.NET 10)**, **Entity Framework Core** and **SQL Server**. Doctors publish time slots, patients book and cancel them, and admins manage doctor accounts. Access is protected by **JWT authentication** and **role-based access control (RBAC)**.

> Built as an internship project. The goal is a clean, understandable design, deliberately without over-engineering.

---

## Table of contents

1. [Architecture](#architecture)
2. [Tech stack](#tech-stack)
3. [Project structure](#project-structure)
4. [Getting started](#getting-started)
5. [Roles and permissions](#roles-and-permissions)
6. [API reference](#api-reference)
7. [How authentication works](#how-authentication-works)
8. [Error handling and crash handling](#error-handling-and-crash-handling)
9. [Double-booking protection](#double-booking-protection)
10. [SOLID principles in this codebase](#solid-principles-in-this-codebase)
11. [Manual test walkthrough](#manual-test-walkthrough)
12. [Production notes](#production-notes)
13. [Known limitations](#known-limitations)
14. [Future improvements](#future-improvements)

---

## Architecture

Two small services, each with **its own database**. They never read each other's tables and never call each other. The only thing they share is the JWT configuration (issuer, audience and secret key).

```
                    +-----------------------+
   Register/Login   |     AuthService       |     WellTimes_AuthDb
  ----------------> |  users, passwords,    | --> (Users)
   Admin: create    |  JWT issuing, admin   |
   doctors          +-----------+-----------+
                                |
                      JWT token (sub, name, role)
                                |
                                v
                    +-----------------------+
   Slots, booking,  |  AppointmentService   |     WellTimes_AppointmentDb
   cancelling       |  validates JWT,       | --> (Slots, Appointments)
  ----------------> |  enforces roles       |
                    +-----------------------+
```

**Key design decisions**

- **Database per service.** `Appointments` stores `PatientId` and `Slots` stores `DoctorId` as plain Guids copied from the token's `sub` claim. There is no foreign key to a users table, because users live in another service's database.
- **Stateless authorization.** AppointmentService validates the token signature locally using the shared secret. It never calls AuthService, so there is no runtime coupling between the two.
- **Layered design inside each service:** Controller -> Service -> Repository -> Database, with interfaces between the layers.
- **Small duplicated files instead of a shared library** (`AppException`, `ExceptionHandlingMiddleware`, `Roles`, the OpenAPI Bearer transformer). A shared project would force both services to be rebuilt and released together.

## Tech stack

| Area | Choice |
|---|---|
| Framework | ASP.NET Core Web API (.NET 10), controllers |
| Database | SQL Server (LocalDB for development) |
| ORM | Entity Framework Core with migrations |
| Authentication | JWT bearer tokens (HMAC-SHA256) |
| Password hashing | BCrypt (`BCrypt.Net-Next`) |
| API documentation | OpenAPI plus Swagger UI (`Swashbuckle.AspNetCore.SwaggerUI`) |
| Health checks | `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` |

## Project structure

```
WellTimes.Appointments/
├── AuthService/
│   ├── Controllers/        AuthController, AdminController
│   ├── Services/           IAccountService / AccountService, IAdminService / AdminService
│   ├── Repositories/       IUserRepository / UserRepository
│   ├── Security/           IPasswordHasher, ITokenService, JwtSettings, SeedAdminSettings, ...
│   ├── Data/               AuthDbContext, AdminSeeder
│   ├── Models/             User, Roles
│   ├── DTOs/               RegisterRequest, LoginRequest, LoginResponse, UserResponse
│   ├── Exceptions/         AppException, EmailAlreadyExistsException, InvalidCredentialsException
│   └── Middleware/         ExceptionHandlingMiddleware
└── AppointmentService/
    ├── Controllers/        SlotsController, AppointmentsController
    ├── Services/           ISlotService / SlotService, IBookingService / BookingService
    ├── Repositories/       ISlotRepository / SlotRepository, IAppointmentRepository / AppointmentRepository
    ├── Security/           JwtSettings, UserClaimsExtensions, BearerSecuritySchemeTransformer
    ├── Data/               AppointmentDbContext, UTC date converters
    ├── Models/             Slot, Appointment, AppointmentStatus, Roles
    ├── DTOs/               CreateSlotRequest, SlotResponse, BookAppointmentRequest, AppointmentResponse
    ├── Exceptions/         AppException and the business exceptions
    └── Middleware/         ExceptionHandlingMiddleware
```

**What each layer does**

| Layer | Responsibility | Must not |
|---|---|---|
| Controller | Receive the HTTP request, read identity from the token, return the HTTP response | Contain business rules or database code |
| Service | Business rules (for example, "a slot must start in the future") | Know about HTTP or SQL |
| Repository | All database access | Contain business decisions |
| DTO | Shape of data entering and leaving the API | Contain logic |
| Model | Shape of data stored in the database | Contain logic |

## Getting started

### Prerequisites

- .NET 10 SDK
- Visual Studio (with the **ASP.NET and web development** workload)
- SQL Server LocalDB (installed with Visual Studio) or a full SQL Server instance

### 1. Configure the databases

Each service has a connection string in its `appsettings.json`. The defaults use LocalDB:

```json
"ConnectionStrings": {
  "AuthDb": "Server=(localdb)\\MSSQLLocalDB;Database=WellTimes_AuthDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

```json
"ConnectionStrings": {
  "AppointmentDb": "Server=(localdb)\\MSSQLLocalDB;Database=WellTimes_AppointmentDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

If you use a full SQL Server instance, change `Server=` accordingly. Note that SSMS must connect to the **same** server that the connection string points to, otherwise the databases will appear to be missing.

### 2. Configure JWT

Both services need the **identical** `Jwt` section. If any value differs, every token is rejected with `401`.

```json
"Jwt": {
  "Issuer": "WellTimes.AuthService",
  "Audience": "WellTimes.Services",
  "SecretKey": "dev-only-secret-key-change-me-at-least-32-chars!!",
  "ExpiryMinutes": 60
}
```

The secret key must be at least 32 characters. The value above is for local development only. See [Production notes](#production-notes).

### 3. Configure the seeded admin (development)

The first Admin account is created automatically at AuthService startup. Add this to `AuthService/appsettings.Development.json`:

```json
"SeedAdmin": {
  "FullName": "System Admin",
  "Email": "admin@welltime.com",
  "Password": "ChangeMe-Admin-123"
}
```

The seeder is idempotent: if the email already exists, it does nothing. If the section is empty, no admin is created.

### 4. Create the databases

In Visual Studio, open **Tools -> NuGet Package Manager -> Package Manager Console** and run each command **with explicit project flags**. EF uses the startup project's `appsettings.json`, so without the flags it can pick the wrong service.

```
Update-Database -Project AuthService -StartupProject AuthService
Update-Database -Project AppointmentService -StartupProject AppointmentService
```

The migrations are already in the repository. If you change a model, add a new migration first, for example:

```
Add-Migration <MigrationName> -Project AppointmentService -StartupProject AppointmentService
```

### 5. Run both services

Right-click the solution -> **Properties** -> **Startup Project** -> **Multiple startup projects**, and set both `AuthService` and `AppointmentService` to **Start**. Then press F5.

Ports are defined in each project's `Properties/launchSettings.json`. In the default development setup:

| Service | HTTPS | HTTP |
|---|---|---|
| AuthService | https://localhost:7023 | http://localhost:5097 |
| AppointmentService | https://localhost:7259 | http://localhost:5146 |

Swagger UI is available at `/swagger` on each service (Development environment only). Health checks are at `/health`.

## Roles and permissions

| Capability | Anonymous | Patient | Doctor | Admin |
|---|:---:|:---:|:---:|:---:|
| Register (always creates a Patient) | yes | | | |
| Log in | yes | yes | yes | yes |
| Create a doctor account | | | | yes |
| Create availability slots | | | yes | |
| View available slots | | yes | yes | yes |
| Book an appointment | | yes | | |
| View my appointments | | yes | | |
| Cancel my appointment | | yes | | |
| View bookings on my slots | | | yes | |

Roles can never be chosen by the caller. Public registration always produces a `Patient`, the doctor role is assigned only by the Admin endpoint, and the first Admin comes from the seeder.

## API reference

### AuthService

| Method | Route | Access | Description |
|---|---|---|---|
| POST | `/api/auth/register` | Public | Create a Patient account |
| POST | `/api/auth/login` | Public | Returns a JWT and the user's details |
| POST | `/api/admin/doctors` | Admin | Create a Doctor account |
| GET | `/health` | Public | Service and database health |

**Register / create doctor body**

```json
{
  "fullName": "Test Patient",
  "email": "patient@welltime.com",
  "password": "Passw0rd123"
}
```

Validation: full name required (max 100), valid email (max 256), password 8 to 100 characters. Emails are trimmed and lowercased, and are unique at the database level.

**Login body**

```json
{
  "email": "patient@welltime.com",
  "password": "Passw0rd123"
}
```

**Login response**

```json
{
  "accessToken": "<jwt>",
  "expiresAtUtc": "2026-10-15T06:18:06Z",
  "user": { "id": "...", "fullName": "Test Patient", "email": "patient@welltime.com", "role": "Patient" }
}
```

### AppointmentService

All endpoints require the header `Authorization: Bearer <token>`.

| Method | Route | Role | Description |
|---|---|---|---|
| POST | `/api/slots` | Doctor | Create an availability slot |
| GET | `/api/slots/available` | Any authenticated user | List future, unbooked slots |
| POST | `/api/appointments` | Patient | Book a slot |
| GET | `/api/appointments/my` | Patient | List my appointments |
| POST | `/api/appointments/{id}/cancel` | Patient | Cancel my appointment |
| GET | `/api/appointments/doctor` | Doctor | List bookings on my slots |
| GET | `/health` | Public | Service and database health |

**Create slot body**

```json
{
  "startTime": "2026-10-25T09:00:00Z",
  "endTime": "2026-10-25T09:30:00Z"
}
```

The doctor's identity comes from the token, never from the body.

**Book appointment body**

```json
{ "slotId": "<slot guid>" }
```

The patient's identity comes from the token, never from the body.

### Status codes

| Code | Meaning in this API |
|---|---|
| 200 / 201 | Success / created |
| 400 | Invalid input (validation, bad time range, slot in the past) |
| 401 | Missing, expired, tampered, or wrong-issuer token, or wrong credentials at login |
| 403 | Authenticated, but the role is not allowed |
| 404 | Slot or appointment not found (also returned for someone else's appointment) |
| 409 | Conflict: email exists, slot overlap, slot already booked, appointment already cancelled |
| 500 | Unexpected error (details are logged, not returned) |

**Error response shape**

```json
{
  "status": 409,
  "message": "This slot has already been booked.",
  "traceId": "0HNG..."
}
```

Input validation failures (`400`) use the standard ASP.NET `ProblemDetails` format with an `errors` object.

## How authentication works

1. The user logs in at AuthService. The password is verified against the BCrypt hash.
2. AuthService returns a signed JWT containing: `sub` (user id), `email`, `name`, `role`, `jti`, `nbf`, `exp`, `iss`, `aud`.
3. The client sends the token in the `Authorization: Bearer` header.
4. The receiving service validates the **signature, issuer, audience and lifetime** (30-second clock skew) using the shared secret, then reads `role` and enforces `[Authorize(Roles = ...)]`.

A JWT is **signed, not encrypted**: anyone can read the payload, so no secrets are ever placed in it. Changing any character of the payload invalidates the signature, so a user cannot promote themselves to another role.

Login uses a single generic message for both "unknown email" and "wrong password", so an attacker cannot discover which emails have accounts.

## Error handling and crash handling

- **Custom exceptions.** Business errors derive from `AppException`, which carries its own HTTP status code (for example `SlotAlreadyBookedException` -> 409).
- **Global exception middleware**, registered first in the pipeline so it wraps everything:
  - `AppException` -> logged as a warning, and the exact message and status are returned.
  - Any other exception -> logged as an error with the full stack trace, and the client receives only a generic 500 message.
- **Trace ids.** Every error response contains a `traceId`, and the same id appears in the log line, so a reported error can be found in the logs.
- **Transactions.** Booking and the slot-flag update succeed or fail together, so a crash between the steps cannot leave inconsistent data.
- **Health endpoints.** `/health` returns `200 Healthy` when the service can reach its own database and `503 Unhealthy` otherwise.
- **UTC everywhere.** All stored times are UTC, and a value converter marks every `DateTime` read from the database as UTC, so responses carry a `Z` suffix.

Adding a new business error requires only a new class that inherits `AppException`. The middleware never changes.

## Double-booking protection

Two patients booking the same slot at the same instant is the main concurrency risk. It is handled in three layers:

1. **Friendly check** in `BookingService`: if the slot is already marked booked, return `409` immediately.
2. **Atomic claim** in `AppointmentRepository.TryCreateAsync`, inside a transaction:
   ```sql
   UPDATE Slots SET IsBooked = 1 WHERE Id = @id AND IsBooked = 0
   ```
   Exactly one concurrent request sees "1 row affected" and wins. The others get `409`.
3. **Filtered unique index** on `Appointments(SlotId) WHERE Status = 'Booked'`. Even if the application code had a bug, the database refuses two active bookings for one slot, while cancelled history stays allowed.

Code checks give good messages, and database constraints give guarantees. The system needs both.

## SOLID principles in this codebase

| Principle | Where it appears | What to point out |
|---|---|---|
| **S** Single Responsibility | Controller / Service / Repository split in both services | Controllers translate HTTP, services hold rules, repositories hold SQL. `JwtTokenService` only creates tokens, `BCryptPasswordHasher` only hashes, `ExceptionHandlingMiddleware` only converts exceptions. |
| **O** Open/Closed | `AppException` hierarchy and `ExceptionHandlingMiddleware` | A new error type is one new class, and the middleware is never edited. Adding a booking rule is a new `if` in the service without touching controllers or repositories. |
| **L** Liskov Substitution | `IPasswordHasher`, `ITokenService`, `AppException` subclasses | Any implementation can replace another without breaking callers. For example, `Verify(x, Hash(x))` is always true for any hasher, and any `AppException` subclass is handled correctly by the middleware. |
| **I** Interface Segregation | `IAccountService` vs `IAdminService`, `ISlotRepository` vs `IAppointmentRepository`, `IUserRepository` | Small, focused interfaces. Public registration does not depend on admin-only capabilities, and the slot feature does not depend on appointment operations. |
| **D** Dependency Inversion | Constructors everywhere, `Program.cs` as the composition root | Services depend on interfaces (`IUserRepository`, `IPasswordHasher`, `ITokenService`, `ISlotRepository`, `TimeProvider`), never on EF Core, BCrypt, or the system clock directly. Only `Program.cs` knows the concrete classes. |

**Not strictly SOLID, but related good practice:** the options pattern (`JwtSettings`, `SeedAdminSettings`) for typed configuration, DTOs separate from database models, and constants for role names.

## Manual test walkthrough

Run both services, open Swagger on each, and follow this chain (use the **Authorize** button and paste the token without the word `Bearer`):

1. **Admin**: log in as the seeded admin and confirm the token's `role` is `Admin`.
2. **Create doctor**: `POST /api/admin/doctors` with the Admin token -> `201`.
3. **Doctor**: log in as the doctor and `POST /api/slots` with a future time -> `201`.
4. **Patient**: register and log in, then `GET /api/slots/available` -> the slot is listed.
5. **Book**: `POST /api/appointments` as the patient -> `201`.
6. **Conflict checks**: book the same slot again -> `409`. Book it as a second patient -> `409`.
7. **Security checks**:
   - no token -> `401`
   - Patient token on `POST /api/slots` -> `403`
   - Doctor token on `POST /api/appointments` -> `403`
   - tampered token -> `401`
   - Patient cancelling someone else's appointment -> `404`
8. **Cancel**: cancel the appointment -> `200`. The slot reappears in `/available`, and a second cancel returns `409`.
9. **Doctor view**: `GET /api/appointments/doctor` shows the bookings, including the cancelled one.
10. **Health**: open `/health` on both services -> `Healthy`.

## Production notes

This project is built for development and learning. Before any real deployment:

- **Secrets.** Never commit the JWT secret key or the seed admin password. Provide them through environment variables or a secrets manager (for local work, .NET user secrets), and use a long random key.
- **Seed admin.** Remove or rotate the seeded admin password after the first login, and keep `SeedAdmin` out of production configuration.
- **HTTPS and SQL.** Remove `TrustServerCertificate=True` and use properly trusted certificates and least-privilege SQL logins instead of Windows trusted connections where appropriate.
- **API docs.** Swagger and OpenAPI are enabled in Development only. Keep it that way.
- **Dependencies.** Keep NuGet packages updated and resolve security warnings (for example `NU1902`) by upgrading the affected package.
- **Rate limiting** on login and register to slow brute-force attempts.

## Known limitations

- **Roles live inside the token.** If a user's role changes in the database, existing tokens keep the old role until they expire (default 60 minutes). Refresh tokens and revocation are not implemented.
- **Doctor name is a snapshot.** `DoctorName` is copied from the token when a slot is created and does not update if the user's name changes later.
- **Orphaned references.** Because users live in another database, deleting a user does not clean up their slots or appointments, and no user deletion endpoint exists.
- **No pagination** on list endpoints.
- **Single time zone model.** Times are stored in UTC, and clients are responsible for converting to local time.
- **No automated tests yet.**

## Future improvements

Deliberately left out to keep the project simple:

- **Unit tests** for `SlotService`, `BookingService` and `AccountService` using fake repositories, which the interface-based design is built for.
- **Message queue** (for example RabbitMQ with MassTransit) to publish an `AppointmentBooked` event, so a Notification service can send confirmations without slowing or breaking the booking.
- **Docker and Docker Compose** to start both services and SQL Server with one command. The existing `/health` endpoints and configuration-based settings are already container-friendly.
- **API Gateway** (for example YARP) as a single entry point for clients.
- **Refresh tokens** and token revocation.
- **Doctor features:** list own slots, delete unbooked slots. **Admin features:** list and deactivate users.
- **Structured logging** (Serilog) to a central log store, using the existing `traceId`.
