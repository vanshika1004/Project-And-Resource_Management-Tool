# Project & Resource Management Tool (PRM) - Context

## 1. Project Summary
A client-server console application for managing resources (employees), projects, resource allocations, timesheets, and AI-powered skill matching / risk analysis in an IT services company.

| Aspect | Detail |
|---|---|
| Client | .NET Console Application (interactive menus) |
| Server | ASP.NET Core Web API (REST) |
| Database | Microsoft SQL Server (MSSQL) |
| AI Integration | LLM via REST (Google Gemini / Groq) — configurable at runtime |
| Auth | JWT-based authentication (force password change on first login) |
| Scheduler | Background hosted service (utilisation computation, health flags) |
| Target | .NET 8 (LTS) |

**Important Domain Rule**: The term **Resource** is used universally in place of "Employee" across the system (Database, UI, APIs, and documentation).

## 2. User Roles
| Role | Key Capabilities |
|---|---|
| Admin | Manage users, resources, projects, milestones, skills, allocations view, config |
| Manager | Resource dashboard, allocate/deallocate, my projects, timesheets (read), AI |
| Resource | Submit/view timesheets, view own allocations |

## 3. Key Business Rules (Quick Reference)
- Total utilisation across overlapping allocations cannot exceed 100%
- Timesheet hours per project ≤ allocation% × max_weekly_hours
- Total weekly hours ≤ max_weekly_hours (default 40, configurable)
- No duplicate timesheets for same resource + week
- No future-week timesheet submission
- Deactivation preserves all historical data; ends active allocations immediately
- First admin bootstrapped via DB seed script (default: admin / Admin@1234)
- `force_password_change` flag enforced before any menu access.

## 4. Architecture Decisions
- **AD-01: Clean Architecture (Onion/Hexagonal)**
  Use Clean Architecture with four layers: Core → Application → Infrastructure → Presentation (API + Console). The domain layer has zero framework dependencies.
- **AD-02: Separate Console Client from API Server**
  Console app is a standalone project that communicates with the API server over HTTP only. No direct DB access from Console. Server and Console live in the same git repository under `server/` and `client/` folders respectively, each with their own `.sln` file. The Console has zero compile-time project references to any server project. It owns local mirror models for API contracts.
- **AD-03: Repository Pattern**
  Use the Repository pattern with interfaces in PRM.Core and implementations in PRM.Infrastructure.
- **AD-04: Strategy Pattern for LLM Providers**
  Define an `ILlmProvider` interface with GeminiProvider and GroqProvider implementations. Provider is selected at runtime via configuration.
- **AD-05: JWT Authentication**
  Server issues JWT tokens on login. Console stores token in memory for session. Token includes role claim for authorization.
- **AD-06: EF Core with Code-First Migrations**
  Use Entity Framework Core with code-first approach and migrations targeting MSSQL.
- **AD-07: Background Scheduler as Hosted Service**
  Use BackgroundService (IHostedService) within the API project for periodic tasks.
- **AD-08: Centralized Error Handling**
  Global exception handling middleware in API. Custom domain exceptions thrown from Application layer. Standard `ApiErrorResponse` DTO with StatusCode, Message, Errors[].
- **AD-09: Database Seed for First Admin**
  Include a data seed in EF Core migrations that inserts the first Admin user with default credentials (`admin` / `Admin@1234`) and `force_password_change = true`.
- **AD-10: Single Solution, Multiple Projects**
  *Note: There was a minor conflict between AD-02 (separate .sln files) and AD-10 (single .sln). The implementation plan addresses this to ensure separation.*

## 5. Clean Code Rules
- **Meaningful names**: No abbreviations unless universally understood (ID, API, DTO, LLM). Variable name reveals intent.
- **Small focused functions**: Each method does ONE thing. Max ~30 lines as a guideline.
- **No magic numbers**: All constants defined in PRM.Core.Constants or as const/enum.
- **No dead code**: No commented-out code committed. Use Git history.
- **Single Responsibility**: Each class has one reason to change. Controllers are thin.
- **Dependency Injection**: All dependencies injected via constructor. No `new` for services.
- **Fail Fast**: Validate inputs at the boundary (API entry / Console input). Throw early with descriptive exceptions.
- **Guard Clauses**: Check preconditions at top of methods. Reduce nesting.
- **Async all the way**: All I/O operations (DB, HTTP, LLM) use async/await. No `.Result` or `.Wait()`.
- **Immutable DTOs**: Use record types for DTOs where possible.
- **No logic in Controllers**: Controllers validate request format, call service, return response. Period.
- **Consistent error responses**: Standard `ApiErrorResponse` DTO with StatusCode, Message, Errors[].

## 6. Task Guidelines
- **Guideline 1: Always Plan Before Implementing**: Create an implementation plan before writing code. Describe what will be built, files affected, DB changes, API endpoints, dependencies, and acceptance criteria. Await explicit approval.
- **Guideline 2: Clarify Before Assuming**: Never assume business logic, data types, or UI behavior. Refer to BRD and Project_Context.md. Ask questions to resolve ambiguity.
- **Guideline 3: Minimal Blast Radius**: Only modify files directly related to the task. Get approval before touching shared interfaces or unrelated areas. No out-of-scope refactoring.
