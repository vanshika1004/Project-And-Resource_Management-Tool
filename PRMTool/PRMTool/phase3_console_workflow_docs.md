# Phase 3: Console Client — Implementation Workflow Documentation

## Overview

This document describes the Console Client application that consumes the PRM Tool backend REST APIs. The console client provides a terminal-based user interface with BRD-compliant box-drawing layouts, JWT-authenticated API communication, and forced password change enforcement on first login.

**Base URL:** `http://localhost:5144/api/`

---

## Architecture

```
ConsoleClient/
├── Program.cs                    # Entry point — launches LoginScreen
├── ConsoleUIHelper.cs            # Static helper for box-drawing, menus, prompts
├── ApiClients/
│   ├── ApiClientBase.cs          # HttpClient wrapper with JWT auth header
│   ├── AuthApiClient.cs          # Login + ChangePassword API calls
│   └── UserApiClient.cs          # CreateUser API call
├── Storage/
│   └── SessionManager.cs         # In-memory JWT token + user info storage
└── Screens/
    ├── LoginScreen.cs            # BRD Screen 1 — Login / Exit
    ├── ChangePasswordScreen.cs   # Forced password change (first login)
    ├── AdminMenuScreen.cs        # BRD Screen 3 — Admin Panel
    ├── ManagerMenuScreen.cs      # BRD Screen 4 — Manager Menu (stub)
    ├── EmployeeMenuScreen.cs     # Employee Menu (stub)
    └── Admin/
        ├── ManageUsersScreen.cs  # BRD Screen 3.4 — Manage Users submenu
        └── CreateUserScreen.cs  # BRD Screen 3.4.1 — Create User Account
```

---

## 1. API Communication Layer

### ApiClientBase

The base class that all API clients inherit from. It manages a single static `HttpClient` pointed at `http://localhost:5144/api/`.

**Key behaviors:**
- Automatically attaches `Authorization: Bearer <token>` header from `SessionManager` on every request.
- Provides `GetAsync<T>()` and `PostAsync<T>()` generic helpers.
- On HTTP errors, extracts the `message` field from the server's JSON error response and throws it as an `Exception` so screens can display user-friendly errors.

### AuthApiClient

| Method | API Endpoint | Description |
|--------|-------------|-------------|
| `LoginAsync(username, password)` | `POST /api/auth/login` | Returns `LoginResponse` with token, username, fullName, role, userId, forcePasswordChange |
| `ChangePasswordAsync(userId, currentPassword, newPassword)` | `POST /api/auth/change-password` | Changes user password, server sets `ForcePasswordChange = false` |

### UserApiClient

| Method | API Endpoint | Description |
|--------|-------------|-------------|
| `CreateUserAsync(fullName, email, username, tempPassword, role)` | `POST /api/users` | Creates user account. Server auto-sets `ForcePasswordChange = true` |

---

## 2. Session Management

`SessionManager` is a static class that holds session state in-memory for the duration of the application run:

| Property | Type | Description |
|----------|------|-------------|
| `Token` | `string?` | JWT Bearer token from login |
| `Username` | `string?` | Logged-in user's username |
| `FullName` | `string?` | Display name for menus |
| `Role` | `string?` | `ADMIN`, `MANAGER`, or `EMPLOYEE` |
| `UserId` | `int` | User's database ID |
| `ForcePasswordChange` | `bool` | If true, must change password |
| `IsLoggedIn` | `bool` | True when token is set |

**Key methods:**
- `SetSession(...)` — Called after successful login
- `ClearSession()` — Called on logout
- `ClearForcePasswordChange()` — Called after password change

---

## 3. Screen Flows

### 3.1 Login Flow (BRD Screen 1)

```
┌─────────────────────┐
│   Application Start  │
│   1. Login           │
│   2. Exit            │
└──────────┬──────────┘
           │ Option 1
           ▼
┌─────────────────────┐
│   Enter Username     │
│   Enter Password     │ (masked with *)
└──────────┬──────────┘
           │ POST /api/auth/login
           ▼
     ┌─────┴─────┐
     │ Success?   │
     └─┬───────┬─┘
       │ No    │ Yes
       ▼       ▼
   Show Error  Store Session
   Return      │
               ▼
     ┌─────────┴─────────┐
     │ ForcePasswordChange│
     └─┬───────────────┬─┘
       │ true          │ false
       ▼               ▼
   Change Password   Route to Role Menu
   Screen             │
       │               ├── Admin → AdminMenuScreen
       │               ├── Manager → ManagerMenuScreen
       └───────────────└── Employee → EmployeeMenuScreen
```

### 3.2 Forced Password Change

Triggered when `LoginResponse.ForcePasswordChange == true`. This screen **cannot be skipped** — the user must set a new password before accessing any menu.

**Client-side validation rules (before sending to server):**
- Passwords must match
- Minimum 8 characters
- At least one uppercase letter
- At least one digit

**API call:** `POST /api/auth/change-password` with `{ UserId, CurrentPassword, NewPassword }`

After success, `SessionManager.ClearForcePasswordChange()` is called and the user proceeds to their role menu.

### 3.3 Admin Menu (BRD Screen 3)

Shows the admin's full name and current date/time in the header. Menu options:

| Option | Status | Screen |
|--------|--------|--------|
| 1. Manage Employees | 🟡 Coming Soon | — |
| 2. Manage Projects | 🟡 Coming Soon | — |
| 3. View All Allocations | 🟡 Coming Soon | — |
| 4. Manage Users | ✅ Active | ManageUsersScreen |
| 5. System Configuration | 🟡 Coming Soon | — |
| 6. Logout | ✅ Active | Clears session → Login |

### 3.4 Manage Users (BRD Screen 3.4)

| Option | Status | Screen |
|--------|--------|--------|
| 1. Create User Account | ✅ Active | CreateUserScreen |
| 2. View All Users | 🟡 Coming Soon | — |
| 3. Reset User Password | 🟡 Coming Soon | — |
| 4. Deactivate User | 🟡 Coming Soon | — |
| 5. Back | ✅ Active | Returns to Admin Menu |

### 3.5 Create User Account (BRD Screen 3.4.1)

Prompts for:
1. Full Name (required)
2. Email (required)
3. Username (required)
4. Temporary Password (masked, required)
5. Role: (1) Admin, (2) Manager, (3) Employee

**Client-side validations:**
- All fields must be non-empty
- Password: 8+ chars, uppercase, digit

**Role mapping:** Menu choice → `UserRole` enum:
- `1` → `0` (Admin)
- `2` → `1` (Manager)
- `3` → `2` (Employee)

**Server behavior on create:**
- Sets `ForcePasswordChange = true`
- If role is Manager or Employee, also creates an `Employee` record with Bench status
- Rejects duplicate usernames and emails

---

## 4. Backend Changes Made for Console Client

### LoginResponseDto (Modified)

Added two new properties so the console client has the data it needs:
- `FullName` — displayed in menu headers
- `UserId` — sent with change-password requests

### AuthService (Modified)

1. **Populates `FullName` and `UserId`** in the login response
2. **Added `IsActive` check** — deactivated users get: `"Account is deactivated. Contact your administrator."`

---

## 5. How to Run

1. **Start the WebAPI** (in one terminal):
   ```bash
   cd WebAPI
   dotnet run
   ```
   Server starts on `http://localhost:5144`

2. **Start the ConsoleClient** (in another terminal):
   ```bash
   cd ConsoleClient
   dotnet run
   ```

3. **Login** with the seeded admin account:
   - Username: `admin`
   - Password: `Admin@1234`

4. On first login, you'll be forced to change the password.

5. After password change, the Admin menu appears where you can create new user accounts.

---

## 6. What is Left to Build (Future Screens)

### Admin Screens
- Manage Employees (View All, Update, Deactivate, Manage Skills, Assign Manager)
- Manage Projects (Create, View All, Update, Manage Milestones)
- View All Allocations
- View All Users, Reset Password, Deactivate User, Reactivate User
- System Configuration

### Manager Screens
- Resource Dashboard
- Allocate Resource (AI search + direct allocation)
- My Projects (project health view)
- Timesheets (team view)
- AI Assistant

### Employee Screens
- Submit Timesheet (with activity tags)
- My Timesheets (history)
- My Allocations (history)

### Backend Phase 2 (Remaining)
- 5 remaining FluentValidation validators
- [Authorize(Roles)] on all controllers
- Swagger JWT integration
- ILogger audit logging
- DailyMaintenanceJob background scheduler
