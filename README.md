# 🚀 Leave Management System - Web API

A robust and secure RESTful API built with .NET Core for managing employee leave requests, approvals, and balances. This enterprise-grade backend provides comprehensive leave management functionality with role-based authorization.

## 📋 Table of Contents
- [Features](#features)
- [Technologies Used](#technologies-used)
- [Installation](#installation)
- [Usage](#usage)
- [API Endpoints](#api-endpoints)
- [Database Schema](#database-schema)
- [Authentication](#authentication)
- [Project Structure](#project-structure)
- [Configuration](#configuration)
- [Contributing](#contributing)

## ✨ Features

### 🔐 **Authentication & Authorization**
- JWT-based secure authentication
- Role-based access control (Admin/Employee)
- Password hashing with bcrypt
- Token expiration handling

### 👥 **User Management**
- Employee registration with role assignment
- User profile management
- Leave balance initialization
- Admin user management

### 📅 **Leave Management**
- Apply for leave requests
- Leave approval/rejection workflow
- Leave cancellation (with business rules)
- Leave history tracking
- Leave balance management

### 📊 **Admin Features**
- Employee registration and management
- Leave request approval system
- Leave reports and filtering
- Excel export functionality
- Dashboard metrics and analytics

### 🛡️ **Security & Validation**
- Input validation and sanitization
- CORS configuration
- Secure password handling
- Data transfer objects (DTOs)
- Authorization filters

## 🛠️ Technologies Used

- **Framework:** .NET 8.0 / ASP.NET Core Web API
- **Database:** SQL Server with Entity Framework Core
- **Authentication:** JWT (JSON Web Tokens)
- **ORM:** Entity Framework Core (Code First)
- **Mapping:** AutoMapper
- **Architecture:** Repository Pattern with Dependency Injection
- **Documentation:** Swagger/OpenAPI
- **Testing:** xUnit (ready for implementation)

## 📦 Installation

### Prerequisites
- .NET 8.0 SDK or later
- SQL Server (LocalDB or full version)
- Visual Studio 2022 or VS Code
- Postman (for API testing)

### Setup Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/bhavikpatel025/leave-management-api.git
   cd leave-management-api
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Update database connection string**
   Edit `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LeaveManagementDB;Trusted_Connection=true;"
     }
   }
   ```

4. **Create and migrate database**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access API Documentation**
   Navigate to `https://localhost:7000/swagger`

## 🚀 Usage

The API will be running on `https://localhost:7000` with Swagger documentation available for testing endpoints.

## 📚 API Endpoints

### Authentication
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/auth/login` | User login | ❌ |
| POST | `/api/auth/register` | Register employee | ✅ Admin |

### Users Management
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/users` | Get all employees | ✅ Admin |
| GET | `/api/users/{id}` | Get employee by ID | ✅ |
| PUT | `/api/users/{id}/leave-balance` | Update leave balance | ✅ Admin |

### Leave Management
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/leaves/apply` | Apply for leave | ✅ Employee |
| GET | `/api/leaves/my-leaves` | Get personal leaves | ✅ Employee |
| GET | `/api/leaves/all` | Get all leave requests | ✅ Admin |
| PUT | `/api/leaves/{id}/cancel` | Cancel leave request | ✅ Employee |
| PUT | `/api/leaves/update-status` | Approve/reject leave | ✅ Admin |
| GET | `/api/leaves/types` | Get leave types | ✅ |
| GET | `/api/leaves/export` | Export leaves to Excel | ✅ Admin |

## 🗄️ Database Schema

### Core Models

**User Model:**
```csharp
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public int RoleId { get; set; }
    public Role Role { get; set; }
    public List<Leave> Leaves { get; set; }
    public List<LeaveBalance> LeaveBalances { get; set; }
}
```

**Leave Model:**
```csharp
public class Leave
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int LeaveTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public string Reason { get; set; }
    public string Status { get; set; }
    public DateTime AppliedDate { get; set; }
}
```

**Leave Types:**
- Annual Leave
- Sick Leave
- Personal Leave
- Maternity/Paternity Leave

## 🔐 Authentication

### JWT Configuration
```json
{
  "JwtSettings": {
    "Key": "your-super-secret-jwt-signing-key-here",
    "Issuer": "LeaveManagementAPI",
    "Audience": "LeaveManagementClient",
    "ExpiryInHours": 24
  }
}
```

### Authorization Levels
- **Employee:** Can apply for leaves, view own leaves, cancel own leaves
- **Admin:** Full access to all endpoints, user management, leave approvals

## 📁 Project Structure

```
LeaveManagementAPI/
├── Controllers/
│   ├── AuthController.cs        # Authentication endpoints
│   ├── UsersController.cs       # User management
│   └── LeavesController.cs      # Leave management
├── Models/
│   ├── User.cs                  # User entity
│   ├── Role.cs                  # Role entity
│   ├── Leave.cs                 # Leave entity
│   ├── LeaveType.cs            # Leave type entity
│   └── LeaveBalance.cs         # Leave balance entity
├── Data/
│   └── ApplicationDbContext.cs  # EF DbContext
├── DTOs/
│   ├── LoginDto.cs             # Login data transfer objects
│   ├── UserDto.cs              # User DTOs
│   └── LeaveDto.cs             # Leave DTOs
├── Services/
│   ├── IJwtService.cs          # JWT interface
│   ├── JwtService.cs           # JWT implementation
│   ├── IUserService.cs         # User service interface
│   ├── UserService.cs          # User service implementation
│   ├── ILeaveService.cs        # Leave service interface
│   └── LeaveService.cs         # Leave service implementation
├── Program.cs                   # Application startup
├── appsettings.json            # Configuration
└── LeaveManagementAPI.csproj   # Project file

## 🧪 Testing

### Manual Testing
1. Use Swagger UI at `/swagger`
2. Test with Postman collection
3. Verify authentication flows
4. Test role-based access control

### Unit Testing (Ready to implement)
```bash
# Create test project
dotnet new xunit -n LeaveManagementAPI.Tests

# Add test dependencies
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package Moq
```

## 🚀 Deployment

### Production Configuration
1. Update connection strings for production database
2. Configure JWT settings with secure keys
3. Enable HTTPS redirection
4. Configure CORS for production domains
5. Set up logging and monitoring

### Docker Support
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["LeaveManagementAPI.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LeaveManagementAPI.dll"]
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines
- Follow C# coding conventions
- Write unit tests for new features
- Update API documentation
- Ensure all migrations are included

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Microsoft** for .NET Core and Entity Framework
- **JWT.io** for JWT implementation guidance
- **Swagger** for API documentation
- **AutoMapper** for object-to-object mapping

## 📞 Contact

- **Developer:** Bhavik Patel
- **Email:** pb3721700@gmail.com.com
- **LinkedIn:** https://www.linkedin.com/in/bhavik-patel-217a402a5/?utm_source=share&utm_campaign=share_via&utm_content=profile&utm_medium=android_app

---
