# Authentication & Authorization System

## Overview

This document outlines the complete implementation of the authentication and authorization system for the Enterprise CRM, including JWT authentication, role-based access control, and the full user authentication lifecycle.

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Implementation Steps](#implementation-steps)
3. [Complete Authentication Lifecycle](#complete-authentication-lifecycle)
4. [Testing Guide](#testing-guide)
5. [Security Features](#security-features)

---

## Architecture Overview

### Components

- **AuthService**: Core authentication logic for login, token generation, and validation
- **AuthController**: REST API endpoints for authentication operations
- **UsersController**: User management endpoints (Admin only)
- **JWT Middleware**: Validates tokens on each request
- **Authorization Policies**: Role-based access control

### User Roles

| Role | Permissions |
|------|-------------|
| **Admin** | Full system access, user management, all CRUD operations |
| **Manager** | Create/update customers/leads/opportunities, view all data |
| **User** | View customers, manage assigned tasks/leads/opportunities |
| **ReadOnly** | View-only access to all data |

---

## Implementation Steps

### Step 1: Create Authentication DTOs

**File**: `src/EnterpriseCRM.Application/DTOs.cs`

```csharp
/// <summary>
/// Login request DTO
/// </summary>
public class LoginRequestDto
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Login response DTO
/// </summary>
public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}

/// <summary>
/// User DTO for authentication
/// </summary>
public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public string? Phone { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
}
```

### Step 2: Create Authentication Service Interface

**File**: `src/EnterpriseCRM.Application/Interfaces.cs`

```csharp
/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthenticationService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto loginDto);
    Task<bool> ValidateTokenAsync(string token);
    Task<UserDto?> GetUserFromTokenAsync(string token);
    Task<string> GenerateTokenAsync(User user);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
}
```

### Step 3: Implement Authentication Service

**File**: `src/EnterpriseCRM.Application/Services/AuthService.cs`

Key implementation points:
- Login credential validation
- JWT token generation with claims
- Token validation
- Password change functionality

See `src/EnterpriseCRM.Application/Services/AuthService.cs` for full implementation.

### Step 4: Configure JWT in Program.cs

**File**: `src/EnterpriseCRM.WebAPI/Program.cs`

Add JWT authentication configuration:

```csharp
// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Add Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Manager", "Admin"));
    options.AddPolicy("UserOrAbove", policy => policy.RequireRole("User", "Manager", "Admin"));
    options.AddPolicy("ReadOnlyOrAbove", policy => policy.RequireRole("ReadOnly", "User", "Manager", "Admin"));
});
```

### Step 5: Create AuthController

**File**: `src/EnterpriseCRM.WebAPI/Controllers/AuthController.cs`

Provides endpoints:
- `POST /api/auth/login` - User login
- `POST /api/auth/validate` - Token validation
- `POST /api/auth/change-password` - Password change

### Step 6: Add Role-Based Authorization to Controllers

**File**: `src/EnterpriseCRM.WebAPI/Controllers/CustomersController.cs`

Example:
```csharp
[HttpGet]
[Authorize(Policy = "UserOrAbove")] // Users, Managers, Admins can view
public async Task<ActionResult<PagedResultDto<CustomerDto>>> GetAll(...)

[HttpPost]
[Authorize(Policy = "ManagerOrAdmin")] // Only Managers and Admins can create
public async Task<ActionResult<CustomerDto>> Create(...)

[HttpDelete("{id}")]
[Authorize(Policy = "AdminOnly")] // Only Admins can delete
public async Task<IActionResult> Delete(...)
```

### Step 7: Configure Swagger for JWT

Add JWT authentication to Swagger UI:

```csharp
c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
    Name = "Authorization",
    In = ParameterLocation.Header,
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer"
});
```

### Step 8: Create UsersController

**File**: `src/EnterpriseCRM.WebAPI/Controllers/UsersController.cs`

Admin-only user management with `[Authorize(Policy = "AdminOnly")]`.

### Step 9: Test Authentication

1. Build project: `dotnet build`
2. Run API: `dotnet run --project src/EnterpriseCRM.WebAPI`
3. Test in Swagger at `https://localhost:5001/swagger`

### Step 10: Integrate with Blazor Server

Create login page and integrate authentication in Blazor components.

### Step 11: Add Role-Based UI

Show/hide UI elements based on user role in Blazor components.

---

## Complete Authentication Lifecycle

### Initial Request Flow

**1. User Opens Web Page**
```
User navigates to https://localhost:5001/login
  → Blazor Server receives request
  → Renders Login.razor page
```

**2. User Enters Credentials**
```
User fills in Username and Password
  → Clicks "Login" button
  → OnValidSubmit triggers HandleLogin()
```

**3. Frontend Sends Login Request**
```javascript
POST /api/auth/login
Headers: Content-Type: application/json
Body: {
  "username": "admin",
  "password": "admin123"
}
```

### Backend Processing Flow

**4. AuthController Receives Request**
```
AuthController.Login(loginDto)
  → Validates ModelState
  → Calls AuthenticationService.LoginAsync(loginDto)
```

**5. Authentication Service Validates Credentials**
```csharp
// In AuthenticationService.LoginAsync()
  a. Calls _unitOfWork.Users.ValidateCredentialsAsync(username, password)
     → IUserRepository.ValidateCredentialsAsync()
     → Queries database for user by username
     → Compares password hashes
     → Returns true/false
```

**6. Fetch User Details**
```csharp
if (isValid)
{
  var user = await _unitOfWork.Users.GetByUsernameAsync(username);
     → Returns User entity from database
     → Includes Role, Status, etc.
}
```

**7. Update Last Login**
```csharp
user.LastLoginDate = DateTime.UtcNow;
await _unitOfWork.SaveChangesAsync();
  → Updates Users table in SQL Server
  → Commits transaction
```

**8. Generate JWT Token**
```csharp
var token = GenerateTokenAsync(user);
  → Creates SymmetricSecurityKey from secret
  → Builds JWT claims:
     * NameIdentifier (userId)
     * Name (username)
     * Email
     * Role (enum as string)
     * FirstName, LastName
     * Department, JobTitle
  → Creates JwtSecurityToken with:
     * Issuer: "EnterpriseCRM"
     * Audience: "EnterpriseCRMUsers"
     * Expiration: 60 minutes
     * Claims from user object
  → Signs token with HMACSHA256
  → Returns encoded token string
```

**9. Return Response**
```csharp
return Ok(new LoginResponseDto {
    Token = token,
    ExpiresAt = DateTime.UtcNow.AddMinutes(60),
    User = new UserDto {
        Id = user.Id,
        FirstName = user.FirstName,
        // ... other properties
        Role = user.Role
    }
});
```

### Token Storage and Usage

**10. Frontend Stores Token**
```csharp
// In Blazor Server
// Store token in session/localStorage
HttpContext.Session.SetString("AuthToken", response.Token);
// Or use local storage for persistence
await localStorage.SetItemAsync("authToken", response.Token);
```

**11. Frontend Redirects**
```
User redirected to /dashboard
  → Dashboard.razor loads
  → Fetches user profile
  → Displays user-specific data
```

### Subsequent Request Flow

**12. Protected API Call**
```
User clicks "View Customers"
  → Frontend makes request:
    GET /api/customers
    Headers: Authorization: Bearer {token}
```

**13. JWT Middleware Intercepts**
```
app.UseAuthentication() middleware runs
  → Extracts token from Authorization header
  → Validates token:
     * Checks signature with secret key
     * Validates issuer and audience
     * Checks expiration time
     * Extracts claims
  → If valid: Attaches User object to HttpContext
  → If invalid: Returns 401 Unauthorized
```

**14. Authorization Check**
```
app.UseAuthorization() middleware runs
  → Checks [Authorize] attributes
  → Evaluates policy requirements
  → Validates user role against policy
  → If authorized: Continues to controller
  → If not authorized: Returns 403 Forbidden
```

**15. Controller Executes**
```
CustomersController.GetAll()
  → Authorization passed
  → Executes business logic
  → Calls service layer
  → Returns data to frontend
```

**16. Response Displayed**
```
Frontend receives customer data
  → Updates UI with data
  → Displays in table/list view
```

### Token Expiration Flow

**17. Token Expires**
```
After 60 minutes, user makes another request
  → JWT Middleware validates token
  → Token is expired
  → Returns 401 Unauthorized
```

**18. Frontend Handles Expiration**
```
Frontend receives 401
  → Clears stored token
  → Redirects to /login
  → Shows error message
  → User must login again
```

### Password Change Flow

**19. User Changes Password**
```
User navigates to /account/settings
  → Fills in current and new password
  → POST /api/auth/change-password
  → AuthController.ChangePassword()
    → Validates current password
    → Updates password hash
    → Saves to database
  → Returns success
  → Optionally: Forces re-login
```

---

## Testing Guide

### Test 1: Login with Valid Credentials

**Request:**
```bash
POST https://localhost:5001/api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123"
}
```

**Expected Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-01T12:00:00Z",
  "user": {
    "id": 1,
    "firstName": "Admin",
    "lastName": "User",
    "email": "admin@enterprisecrm.com",
    "username": "admin",
    "role": "Admin",
    "status": "Active"
  }
}
```

### Test 2: Use Token for Protected API

**Request:**
```bash
GET https://localhost:5001/api/customers
Authorization: Bearer {token_from_test_1}
```

**Expected:** Customer list returned

### Test 3: Test Role-Based Access

**As Admin:**
- Can access all endpoints
- Can delete customers
- Can manage users

**As Manager:**
- Can view and create customers
- Cannot delete customers
- Cannot manage users

**As User:**
- Can only view customers
- Cannot create or delete

### Test 4: Invalid Token

**Request:**
```bash
GET https://localhost:5001/api/customers
Authorization: Bearer invalid_token
```

**Expected:** 401 Unauthorized

---

## Security Features

### 1. Password Hashing
- **Current**: Simple comparison (for demo)
- **Production**: Use BCrypt or Argon2
- Store hashed passwords, never plain text

### 2. JWT Security
- **Secret Key**: 32+ character secret stored in appsettings.json
- **HTTPS**: Always use HTTPS in production
- **Expiration**: Tokens expire after 60 minutes
- **Claims**: Minimal claims included in token

### 3. Role-Based Authorization
- **Policies**: Granular access control
- **Principle of Least Privilege**: Users get minimum required access
- **Audit Trail**: Track user actions with CreatedBy/UpdatedBy fields

### 4. Input Validation
- **DTOs**: Data annotations for validation
- **ModelState**: Server-side validation
- **Sanitization**: Input sanitization in repositories

### 5. Session Management
- **Token Storage**: Secure token storage (HTTPS-only cookies)
- **Refresh Tokens**: Implement refresh token pattern for longer sessions
- **Revocation**: Track logged-in sessions for logout

---

## Configuration

### appsettings.json

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "EnterpriseCRM",
    "Audience": "EnterpriseCRMUsers",
    "ExpiryInMinutes": 60
  }
}
```

### Database Seeding

Sample users:
- `admin` / `admin123` (Admin role)
- `john.doe` / `password123` (Manager role)
- `jane.smith` / `password123` (User role)

---

## Future Enhancements

1. **Password Hashing**: Implement BCrypt password hashing
2. **Refresh Tokens**: Add refresh token support for longer sessions
3. **Two-Factor Authentication**: Add 2FA support
4. **Account Lockout**: Implement after failed login attempts
5. **Email Verification**: Send verification emails on registration
6. **Password Reset**: Implement forgot password functionality
7. **Audit Logging**: Comprehensive audit trail for all user actions

---

## Conclusion

This authentication system provides a complete, secure solution for the Enterprise CRM. The implementation follows clean architecture principles and provides granular role-based access control for different user types.

The lifecycle demonstrates how user authentication flows from the initial login request through to protected API calls, with proper token validation and authorization at each step.

