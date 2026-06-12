# Project Context: Project & Resource Management Tool (PRMTool)

## Overview
The PRMTool is an Enterprise-grade Project and Resource Management application designed to track employees, manage their skills, assign them to projects (allocations), and track their weekly timesheets.

## Tech Stack
- **Backend:** ASP.NET Core 10 Web API
- **Database:** SQL Server (accessed via Entity Framework Core)
- **Frontend:** C# Console Application (`ConsoleClient`)
- **Architecture:** N-Tier Architecture (Domain, Application, Infrastructure, WebAPI, ConsoleClient)

## Project Structure
- **Domain:** Contains Core Entities (`Employee`, `Project`, `User`, `Skill`, `Allocation`, etc.) and Enums (`UserRole`, `EmployeeStatus`).
- **Application:** Contains business logic (Services like `EmployeeService`, `ProjectService`), Interfaces (Repositories, Services), and DTOs.
- **Infrastructure:** Contains EF Core `AppDbContext`, Migrations, and Repository implementations.
- **WebAPI:** Contains the REST Controllers, JWT Authentication, and Dependency Injection configuration.
- **ConsoleClient:** The user interface for interacting with the API. It features a robust role-based menu system (Admin, Manager, Employee) and handles API communication via typed `ApiClients`.
- **Tests:** Contains unit and integration tests (e.g., `E2ETestRunner`).

## Current State & Recent Fixes
- **Employee Management:** Fixed the "Deactivate Employee" functionality. The system now correctly utilizes the `IsActive` flag instead of attempting to parse an invalid "Inactive" enum value.
- **Project Management:** Fixed validation to strictly require `ManagerId` to map to a valid `Manager` role. Total Story Points are now successfully saved during project creation.
- **UI Enhancements:** The Console UI lists (e.g., View All Employees, View All Projects) now include smart text truncation to prevent column alignment bleeding.
- **QA Automation:** An End-to-End Test Runner script was created to generate 10+ Employees, Managers, Projects, and Skills for load/UI testing.

## Running the Application
1. **Database:** Ensure SQL Server is running and the connection string in `appsettings.json` is correct.
2. **WebAPI:** Navigate to `PRMTool/WebAPI` and run `dotnet run`. It will launch on `http://localhost:5144` and `https://localhost:7026`.
3. **ConsoleClient:** Navigate to `PRMTool/ConsoleClient` and run `dotnet run`. Use `admin` / `Admin@5678` for initial admin access.
