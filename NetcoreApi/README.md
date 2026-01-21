# Flutter NetCore Backend API

Professional .NET Core Web API for testing Flutter HTTP client with comprehensive scenarios including retry mechanisms, error simulation, file operations, and authentication.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Build](https://img.shields.io/badge/build-passing-brightgreen.svg)]()

---

## 📋 Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [API Endpoints](#api-endpoints)
- [Testing Features](#testing-features)
- [Database Schema](#database-schema)
- [Configuration](#configuration)
- [Usage Examples](#usage-examples)
- [Deployment](#deployment)

---

## ✨ Features

### Core Features
- ✅ **CRUD Operations** - Complete Create, Read, Update, Delete for Users & Products
- ✅ **File Upload/Download** - Single & multiple file uploads with metadata tracking
- ✅ **Authentication** - JWT-based authentication with refresh tokens
- ✅ **Pagination** - Built-in pagination support for list endpoints
- ✅ **Soft Delete** - Non-destructive delete operations

### Testing Features
- ✅ **Error Simulation** - Simulate 500, 401, 404, timeout errors via headers
- ✅ **Delay Simulation** - Add custom delays to test loading states
- ✅ **Random Error Rate** - Configure random error probability
- ✅ **Request Logging** - Comprehensive request/response logging

### Response Types
- ✅ **Generic Wrapper** - `ApiResponse<T>` for consistent responses
- ✅ **Direct Models** - Return models without wrapper
- ✅ **Primitives** - Return string, int, bool directly
- ✅ **Dynamic Maps** - Return `Dictionary<string, object>`
- ✅ **No Content** - 204 responses for void operations

---

## 🛠️ Tech Stack

- **.NET 8.0** - Latest .NET framework
- **Entity Framework Core** - ORM for database operations
- **SQL Server** - Database (LocalDB for development)
- **JWT Bearer** - Authentication tokens
- **Swagger/OpenAPI** - API documentation
- **Serilog** - Structured logging (optional)

---

## 📁 Project Structure

```
NetCoreBackend/
├── Controllers/
│   ├── UsersController.cs          # User CRUD operations
│   ├── ProductsController.cs       # Product CRUD operations
│   ├── FilesController.cs          # File upload/download
│   └── AuthController.cs           # Authentication endpoints
├── Data/
│   ├── AppDbContext.cs             # EF Core context
│   ├── User.cs                     # User entity & DTOs
│   ├── Product.cs                  # Product entity & DTOs
│   ├── Order.cs                    # Order entity & DTOs
│   └── FileMetadata.cs             # File metadata entity
├── Services/
│   ├── IFileService.cs             # File service interface
│   ├── FileService.cs              # File operations implementation
│   ├── IAuthService.cs             # Auth service interface
│   └── AuthService.cs              # JWT token generation
├── Middleware/
│   ├── ErrorSimulatorMiddleware.cs # Error simulation for testing
│   ├── DelayMiddleware.cs          # Delay simulation for testing
│   └── RequestLoggingMiddleware.cs # Request/response logging
├── Models/
│   └── ApiResponse.cs              # Generic API response wrapper
├── Migrations/                     # EF Core migrations
├── uploads/                        # Uploaded files directory
├── Program.cs                      # Application entry point
├── appsettings.json               # Configuration
└── NetCoreBackend.csproj          # Project file
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) or SQL Server LocalDB
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Postman](https://www.postman.com/) or similar API testing tool (optional)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/safauludogan/flutter-netcore-backend.git
   cd flutter-netcore-backend
   ```

2. **Install required packages**
   ```bash
   dotnet add package Microsoft.EntityFrameworkCore.SqlServer
   dotnet add package Microsoft.EntityFrameworkCore.Tools
   dotnet add package Microsoft.EntityFrameworkCore.Design
   dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
   dotnet add package System.IdentityModel.Tokens.Jwt
   dotnet add package Swashbuckle.AspNetCore
   ```

3. **Update connection string** (if needed)
   
   Edit `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FlutterNetCoreDb;Trusted_Connection=True;"
   }
   ```

4. **Create database**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access Swagger UI**
   ```
   https://localhost:5001/swagger
   ```

---

## 🎯 API Endpoints

### 👥 Users API

| Method | Endpoint | Response Type | Description |
|--------|----------|---------------|-------------|
| `GET` | `/api/users` | `ApiResponse<List<User>>` | Get all users (paginated) |
| `GET` | `/api/users/{id}` | `ApiResponse<User>` | Get user by ID |
| `GET` | `/api/users/{id}/simple` | `User` | Get user (no wrapper) |
| `GET` | `/api/users/{id}/name` | `string` | Get user name only |
| `GET` | `/api/users/count` | `int` | Get total user count |
| `GET` | `/api/users/{id}/exists` | `bool` | Check if user exists |
| `GET` | `/api/users/{id}/info` | `Dictionary<string, object>` | Get user info as map |
| `POST` | `/api/users` | `ApiResponse<User>` | Create new user |
| `PUT` | `/api/users/{id}` | `ApiResponse<User>` | Update user |
| `PATCH` | `/api/users/{id}/activate` | `void (204)` | Activate user |
| `DELETE` | `/api/users/{id}` | `ApiResponse<bool>` | Delete user (soft) |

**Query Parameters for GET /api/users:**
- `page` (default: 1) - Page number
- `pageSize` (default: 10) - Items per page
- `search` - Search by name or email

### 📦 Products API

| Method | Endpoint | Response Type | Description |
|--------|----------|---------------|-------------|
| `GET` | `/api/products` | `ApiResponse<List<Product>>` | Get all products |
| `GET` | `/api/products/{id}` | `ApiResponse<Product>` | Get product by ID |
| `POST` | `/api/products` | `ApiResponse<Product>` | Create new product |
| `PUT` | `/api/products/{id}` | `ApiResponse<Product>` | Update product |
| `DELETE` | `/api/products/{id}` | `ApiResponse<bool>` | Delete product |

### 📁 Files API

| Method | Endpoint | Response Type | Description |
|--------|----------|---------------|-------------|
| `GET` | `/api/files` | `ApiResponse<List<FileMetadata>>` | List all uploaded files |
| `GET` | `/api/files/{id}` | `ApiResponse<FileMetadata>` | Get file metadata |
| `GET` | `/api/files/{id}/download` | `File` | Download file |
| `POST` | `/api/files/upload` | `ApiResponse<FileMetadata>` | Upload single file |
| `POST` | `/api/files/upload-multiple` | `ApiResponse<List<FileMetadata>>` | Upload multiple files |
| `DELETE` | `/api/files/{id}` | `ApiResponse<bool>` | Delete file |

### 🔐 Auth API

| Method | Endpoint | Response Type | Description |
|--------|----------|---------------|-------------|
| `POST` | `/api/auth/login` | `ApiResponse<LoginResponse>` | User login |
| `POST` | `/api/auth/refresh` | `ApiResponse<RefreshTokenResponse>` | Refresh access token |
| `POST` | `/api/auth/logout` | `ApiResponse<bool>` | User logout |

---

## 🧪 Testing Features

### Error Simulation

Simulate various error scenarios using custom headers:

#### 1. Simulate 500 Internal Server Error
```bash
curl -X GET http://localhost:5000/api/users \
  -H "X-Simulate-Error: 500"
```

#### 2. Simulate Timeout
```bash
curl -X GET http://localhost:5000/api/users \
  -H "X-Simulate-Error: timeout"
```

#### 3. Simulate Network Error
```bash
curl -X GET http://localhost:5000/api/users \
  -H "X-Simulate-Error: network"
```

#### 4. Simulate 401 Unauthorized
```bash
curl -X GET http://localhost:5000/api/users \
  -H "X-Simulate-Error: unauthorized"
```

### Delay Simulation

Add artificial delay to test loading states:

```bash
# 2 second delay
curl -X GET http://localhost:5000/api/users \
  -H "X-Delay-Ms: 2000"
```

### Random Error Rate

Configure random error probability (0-100%):

```bash
# 30% chance of random error
curl -X GET http://localhost:5000/api/users \
  -H "X-Error-Rate: 30"
```

### Combined Testing

```bash
# Delay + Random errors
curl -X GET http://localhost:5000/api/users \
  -H "X-Delay-Ms: 1000" \
  -H "X-Error-Rate: 20"
```

---

## 🗄️ Database Schema

### Users Table
```sql
Id                uniqueidentifier    PRIMARY KEY
Name              nvarchar(100)       NOT NULL
Email             nvarchar(100)       NOT NULL UNIQUE
Password          nvarchar(255)       NOT NULL
ProfileImageUrl   nvarchar(500)       NULL
CreatedAt         datetime2           NOT NULL
UpdatedAt         datetime2           NULL
IsActive          bit                 NOT NULL DEFAULT 1
```

### Products Table
```sql
Id              uniqueidentifier    PRIMARY KEY
Name            nvarchar(200)       NOT NULL
Description     nvarchar(1000)      NOT NULL
Price           decimal(18,2)       NOT NULL
Stock           int                 NOT NULL
ImageUrl        nvarchar(500)       NULL
CreatedAt       datetime2           NOT NULL
UpdatedAt       datetime2           NULL
```

### FileMetadata Table
```sql
Id                  uniqueidentifier    PRIMARY KEY
FileName            nvarchar(255)       NOT NULL
OriginalFileName    nvarchar(255)       NOT NULL
ContentType         nvarchar(100)       NOT NULL
Size                bigint              NOT NULL
FilePath            nvarchar(500)       NOT NULL
UploadedAt          datetime2           NOT NULL
UploadedByUserId    uniqueidentifier    NULL FOREIGN KEY
```

### Orders Table
```sql
Id              uniqueidentifier    PRIMARY KEY
UserId          uniqueidentifier    FOREIGN KEY
ProductId       uniqueidentifier    FOREIGN KEY
Quantity        int                 NOT NULL
TotalPrice      decimal(18,2)       NOT NULL
Status          int                 NOT NULL
CreatedAt       datetime2           NOT NULL
UpdatedAt       datetime2           NULL
```

---

## ⚙️ Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FlutterNetCoreDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGeneration123456",
    "Issuer": "FlutterNetCoreBackend",
    "Audience": "FlutterApp",
    "ExpirationMinutes": 60
  },
  "FileUpload": {
    "MaxFileSize": 10485760,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".pdf", ".docx", ".xlsx"],
    "UploadPath": "uploads"
  }
}
```

### Environment Variables

```bash
# Development
ASPNETCORE_ENVIRONMENT=Development

# Production
ASPNETCORE_ENVIRONMENT=Production
```

---

## 📝 Usage Examples

### 1. Create User

```bash
curl -X POST http://localhost:5000/api/users \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Doe",
    "email": "john@example.com",
    "password": "password123"
  }'
```

**Response:**
```json
{
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "name": "John Doe",
    "email": "john@example.com",
    "createdAt": "2024-01-15T10:30:00Z",
    "isActive": true
  },
  "success": true,
  "statusCode": 200,
  "message": "User created successfully"
}
```

### 2. Login

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "password123"
  }'
```

**Response:**
```json
{
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "dGVzdHJlZnJlc2h0b2tlbg==",
    "expiresIn": 3600,
    "user": {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "name": "John Doe",
      "email": "john@example.com"
    }
  },
  "success": true,
  "statusCode": 200,
  "message": "Login successful"
}
```

### 3. Upload File

```bash
curl -X POST http://localhost:5000/api/files/upload \
  -F "file=@/path/to/image.jpg" \
  -F "userId=123e4567-e89b-12d3-a456-426614174000"
```

**Response:**
```json
{
  "data": {
    "id": "987f6543-e21c-34d5-b678-537625384950",
    "fileName": "abc123.jpg",
    "originalFileName": "image.jpg",
    "contentType": "image/jpeg",
    "size": 1048576,
    "filePath": "uploads/123e4567-e89b-12d3-a456-426614174000/abc123.jpg",
    "uploadedAt": "2024-01-15T10:35:00Z"
  },
  "success": true,
  "statusCode": 200,
  "message": "File uploaded successfully"
}
```

### 4. Get Users with Pagination

```bash
curl -X GET "http://localhost:5000/api/users?page=1&pageSize=10&search=john"
```

**Response:**
```json
{
  "data": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "name": "John Doe",
      "email": "john@example.com",
      "isActive": true
    }
  ],
  "success": true,
  "statusCode": 200,
  "metadata": {
    "totalCount": 1,
    "page": 1,
    "pageSize": 10,
    "totalPages": 1
  }
}
```

---

## 🔧 EF Core Commands

```bash
# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Rollback to previous migration
dotnet ef database update PreviousMigrationName

# Remove last migration
dotnet ef migrations remove

# Drop database
dotnet ef database drop

# Generate SQL script
dotnet ef migrations script

# Reset database (drop + recreate)
dotnet ef database drop -f
dotnet ef database update
```

---

## 🌱 Seed Data

The database is automatically seeded with initial data:

### Users (3 records)
- **john@example.com** - password123
- **jane@example.com** - password123
- **bob@example.com** - password123

### Products (3 records)
- **Laptop** - $999.99 (Stock: 10)
- **Mouse** - $29.99 (Stock: 50)
- **Keyboard** - $89.99 (Stock: 30)

---

## 🐳 Docker Deployment

### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["NetCoreBackend.csproj", "./"]
RUN dotnet restore "NetCoreBackend.csproj"
COPY . .
RUN dotnet build "NetCoreBackend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NetCoreBackend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetCoreBackend.dll"]
```

### Build & Run

```bash
# Build Docker image
docker build -t netcore-backend .

# Run container
docker run -d -p 5000:80 --name netcore-api netcore-backend

# View logs
docker logs netcore-api

# Stop container
docker stop netcore-api
```

---

## 🧪 Testing with Postman

Import the Postman collection:

1. Open Postman
2. Click Import
3. Import the provided `NetCoreBackend.postman_collection.json`
4. Set environment variable `baseUrl` to `http://localhost:5000`

---

## 🔍 Debugging

### Visual Studio
1. Press `F5` to start debugging
2. Set breakpoints in code
3. Make API requests
4. Inspect variables and step through code

### VS Code

Create `.vscode/launch.json`:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (web)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/bin/Debug/net8.0/NetCoreBackend.dll",
      "args": [],
      "cwd": "${workspaceFolder}",
      "stopAtEntry": false,
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  ]
}
```

---

## 📚 Additional Resources

- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [ASP.NET Core Web API](https://docs.microsoft.com/aspnet/core/web-api/)
- [JWT Authentication](https://jwt.io/)

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the project
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👤 Author

**Your Name**
- GitHub: [@yourusername](https://github.com/yourusername)
- Email: your.email@example.com

---

## 🙏 Acknowledgments

- Built with .NET 8.0
- Entity Framework Core for data access
- Swagger for API documentation
- JWT for authentication

---

## 📞 Support

For support, email your.email@example.com or create an issue in this repository.

---

**Made with ❤️ for Flutter developers**