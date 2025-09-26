# BlogHub - Comprehensive Blogging Platform API

[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Docker](https://img.shields.io/badge/Docker-supported-blue.svg)](https://www.docker.com/)

## Table of Contents
- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Getting Started](#getting-started)
- [Installation](#installation)
- [Configuration](#configuration)
- [API Documentation](#api-documentation)
- [Usage Examples](#usage-examples)
- [Development](#development)
- [Deployment](#deployment)
- [Contributing](#contributing)
- [Troubleshooting](#troubleshooting)
- [License](#license)

## Overview

BlogHub is a robust, enterprise-ready RESTful API backend for modern blogging platforms. Built with ASP.NET Core 8, it provides comprehensive functionality for content management, user authentication, social interactions, and administrative operations. The platform is designed with scalability, security, and performance in mind, featuring Redis caching, background job processing, and cloud-based image management.

### Key Highlights
- **RESTful Architecture**: Clean, stateless API design following REST principles
- **JWT Authentication**: Secure token-based authentication and authorization
- **Role-Based Access Control**: Flexible user permissions system
- **Real-time Features**: Comment voting and notification system
- **Cloud Integration**: Cloudinary for image management, Gmail for email services
- **High Performance**: Redis caching and efficient database queries
- **Production Ready**: Docker containerization with comprehensive logging
- **API Documentation**: Interactive Swagger/OpenAPI documentation

## Features

### 🔐 Authentication & User Management
- **User Registration & Login**: Secure JWT-based authentication
- **Email Verification**: Account confirmation via email tokens
- **Password Management**: Forgot password, reset password, and change password functionality
- **Role-Based Authorization**: Support for multiple user roles (User, Administrator)
- **Profile Management**: Update user profiles and change email addresses
- **Token Management**: JWT token refresh and validation

### 📝 Blog Post Management
- **Full CRUD Operations**: Create, read, update, and delete blog posts
- **Rich Content Support**: Text content with image upload capabilities
- **Post Categorization**: Tag-based post organization
- **Image Management**: Cloudinary integration for post cover images
- **User-specific Posts**: Users can manage their own posts
- **Pagination Support**: Efficient data retrieval with pagination

### 💬 Comment System
- **Nested Comments**: Support for hierarchical comment structures
- **Comment Voting**: Upvote and downvote functionality
- **Comment Moderation**: Users can edit and delete their comments
- **Real-time Interactions**: Instant comment updates and notifications
- **Pagination**: Efficient comment loading with metadata

### 🏷️ Tag Management
- **Tag CRUD Operations**: Full tag lifecycle management
- **Post-Tag Associations**: Many-to-many relationships between posts and tags
- **Tag-based Filtering**: Retrieve posts by specific tags
- **Administrative Control**: Tag creation restricted to administrators

### 📧 Email & Notification System
- **Template-based Emails**: Professional HTML email templates
- **Account Verification**: Automated registration confirmation emails
- **Password Reset**: Secure password reset via email tokens
- **Content Notifications**: Email alerts for new posts and comments
- **SMTP Integration**: Gmail service integration with secure authentication

### 🚀 Performance & Scalability
- **Redis Caching**: Distributed caching for improved performance
- **Background Jobs**: Hangfire integration for asynchronous processing
- **Connection Pooling**: Efficient database connection management
- **Query Optimization**: Entity Framework Core with optimized queries
- **Logging**: Comprehensive Serilog integration for monitoring

## Architecture

BlogHub follows a clean, layered architecture pattern:

```
BloggingAPI/
├── Presentation/           # Controllers and API endpoints
├── Services/              # Business logic and application services
├── Domain/                # Core entities and business models
├── Persistence/           # Data access layer and EF Core context
├── Contracts/             # DTOs and data transfer objects
└── Program.cs             # Application entry point and configuration
```

### Design Patterns
- **Repository Pattern**: Abstracted data access layer
- **Unit of Work**: Transaction management
- **Dependency Injection**: Loose coupling and testability
- **DTO Pattern**: Data transfer and API contracts
- **Service Layer Pattern**: Business logic separation

## Technology Stack

### Core Technologies
- **Framework**: ASP.NET Core 8.0
- **Language**: C# with .NET 8
- **Database**: Microsoft SQL Server
- **ORM**: Entity Framework Core 8.0
- **Authentication**: JWT Bearer tokens
- **API Documentation**: Swagger/OpenAPI 3.0

### Infrastructure & Services
- **Caching**: Redis with StackExchange.Redis
- **Background Jobs**: Hangfire for task scheduling
- **Image Storage**: Cloudinary cloud service
- **Email Service**: SMTP with MailKit and Gmail
- **Containerization**: Docker and Docker Compose
- **Logging**: Serilog with structured logging

### Development Tools
- **IDE Support**: Visual Studio, VS Code, JetBrains Rider
- **Package Management**: NuGet
- **Version Control**: Git
- **CI/CD Ready**: Docker-based deployment pipeline

## Getting Started

### Prerequisites

Before running BlogHub, ensure you have the following installed:

- **.NET SDK 8.0+**: [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Docker & Docker Compose**: [Install Docker](https://docs.docker.com/get-docker/)
- **Git**: For version control

### External Service Accounts (Required)
- **Cloudinary Account**: For image management ([Sign up](https://cloudinary.com/))
- **Gmail Account**: For email services (with app-specific password)

## Installation

### 1. Clone the Repository
```bash
git clone https://github.com/Promise30/BlogHub.git
cd BlogHub
```

### 2. Environment Configuration
Copy the example environment file and configure your settings:
```bash
cp .env.example .env
```

### 3. Configure Environment Variables
Edit the `.env` file with your specific configurations:

```bash
# Database Configuration
MSSQL_SA_PASSWORD=YourStrong!Password123
CONNECTION_STRING="Server=blogdb;Database=BlogHubDb;User=sa;Password=YourStrong!Password123;TrustServerCertificate=true;"

# JWT Configuration
JWT_VALID_ISSUER=blogHub.com
JWT_VALID_AUDIENCE=blogHub
JWT_SECRET_KEY=YourSuperSecretJWTKey123!@#
JWT_EXPIRES=30

# Cloudinary Configuration
CLOUDINARY_CLOUDNAME=your_cloudinary_name
CLOUDINARY_APIKEY=your_api_key
CLOUDINARY_APISECRET=your_api_secret

# Email Configuration
EMAIL_USERNAME=your_email@gmail.com
EMAIL_PASSWORD=your_app_specific_password
EMAIL_FROM=your_email@gmail.com
EMAIL_SMTP_SERVER=smtp.gmail.com
EMAIL_PORT=587

# Environment
ASPNETCORE_ENVIRONMENT=Development
```

### 4. Run with Docker (Recommended)
```bash
docker-compose up -d
```

This will start:
- **BlogHub API**: Available at `http://localhost:8002` and `https://localhost:8003`
- **SQL Server Database**: Internal container communication
- **Redis Cache**: Internal container communication

### 5. Alternative: Local Development Setup
If you prefer to run locally without Docker:

```bash
# Restore dependencies
dotnet restore BloggingPlatform.sln

# Update database
dotnet ef database update -p BloggingAPI

# Run the application
dotnet run --project BloggingAPI
```

## Configuration

### Database Configuration
The application uses Entity Framework Core with SQL Server. Database migrations are included and will be applied automatically on startup.

### Redis Configuration
Redis is used for distributed caching. The connection is configured in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "RedisConn": "redis_image:6379,abortConnect=false"
  }
}
```

### Hangfire Dashboard
Background jobs can be monitored via the Hangfire dashboard at `/hangfire` (requires authentication).

## API Documentation

### Swagger UI
Interactive API documentation is available at:
- **Local**: `http://localhost:8002/swagger`
- **HTTPS**: `https://localhost:8003/swagger`

### Authentication
Most endpoints require JWT authentication. Include the token in the Authorization header:
```
Authorization: Bearer <your-jwt-token>
```

### Main API Endpoints

#### Authentication Endpoints
```
POST   /api/auth/register          # User registration
POST   /api/auth/login             # User login
POST   /api/auth/refresh-token     # Refresh JWT token
POST   /api/auth/forgot-password   # Request password reset
POST   /api/auth/reset-password    # Reset password with token
GET    /api/auth/confirm-email     # Confirm email address
```

#### Post Management
```
GET    /api/posts                  # Get all posts (paginated)
GET    /api/posts/{id}             # Get specific post
POST   /api/posts                  # Create new post
PUT    /api/posts/{id}             # Update existing post
DELETE /api/posts/{id}             # Delete post
PATCH  /api/posts/{id}/image       # Update post cover image
```

#### Comment System
```
GET    /api/comments/posts/{id}    # Get comments for post
GET    /api/comments/{id}          # Get specific comment
POST   /api/comments/posts/{id}    # Create comment on post
PUT    /api/comments/{id}          # Update comment
DELETE /api/comments/{id}          # Delete comment
POST   /api/comments/{id}/vote     # Vote on comment
```

#### Tag Management
```
GET    /api/tags                   # Get all tags
GET    /api/tags/{id}              # Get specific tag
GET    /api/tags/{id}/posts        # Get posts by tag
POST   /api/tags                   # Create tag (Admin only)
PUT    /api/tags/{id}              # Update tag (Admin only)
DELETE /api/tags/{id}              # Delete tag (Admin only)
```

## Usage Examples

### User Registration and Authentication

```bash
# Register a new user
curl -X POST "http://localhost:8002/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "userName": "johndoe",
    "email": "john@example.com",
    "password": "SecurePass123!",
    "confirmPassword": "SecurePass123!",
    "roles": ["User"]
  }'

# Login
curl -X POST "http://localhost:8002/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "johndoe",
    "password": "SecurePass123!"
  }'
```

### Creating a Blog Post

```bash
# Create a new post (requires authentication)
curl -X POST "http://localhost:8002/api/posts" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "My First Blog Post",
    "content": "This is the content of my blog post...",
    "tags": ["technology", "programming"]
  }'
```

### Adding Comments

```bash
# Add a comment to a post
curl -X POST "http://localhost:8002/api/comments/posts/1" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "content": "Great post! Thanks for sharing."
  }'
```

## Development

### Project Structure
```
BloggingAPI/
├── Contracts/
│   ├── Dtos/               # Data Transfer Objects
│   └── Validations/        # Input validation rules
├── Domain/
│   ├── Entities/           # Core domain models
│   ├── Enums/             # Enumeration types
│   └── Repositories/       # Repository interfaces
├── Persistence/
│   ├── Extensions/         # EF Core extensions
│   ├── Repositories/       # Repository implementations
│   └── ApplicationDbContext.cs
├── Presentation/
│   └── Controllers/        # API controllers
└── Services/
    ├── Implementation/     # Service implementations
    └── Interface/          # Service interfaces
```

### Running Tests
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Database Migrations
```bash
# Create new migration
dotnet ef migrations add MigrationName -p BloggingAPI

# Update database
dotnet ef database update -p BloggingAPI

# Remove last migration
dotnet ef migrations remove -p BloggingAPI
```

### Code Quality
The project includes:
- **Code Analysis**: Built-in .NET analyzers
- **Nullable Reference Types**: Enhanced null safety
- **XML Documentation**: Comprehensive API documentation
- **Structured Logging**: Serilog with structured output

## Deployment

### Docker Production Deployment
1. **Build Production Image**:
```bash
docker build -t bloghub-api:prod -f BloggingAPI/Dockerfile .
```

2. **Production Docker Compose**:
```yaml
version: '3.8'
services:
  bloghub-api:
    image: bloghub-api:prod
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=${PROD_CONNECTION_STRING}
    ports:
      - "80:8080"
      - "443:8081"
```

### Environment-Specific Configurations
- **Development**: Enhanced logging, Swagger UI enabled
- **Staging**: Reduced logging, basic monitoring
- **Production**: Minimal logging, security hardened

### Health Checks
The API includes health check endpoints for monitoring:
- `/health` - Basic health check
- `/health/ready` - Readiness probe
- `/health/live` - Liveness probe

## Contributing

We welcome contributions! Please follow these steps:

1. **Fork the Repository**
2. **Create Feature Branch**: `git checkout -b feature/AmazingFeature`
3. **Commit Changes**: `git commit -m 'Add AmazingFeature'`
4. **Push to Branch**: `git push origin feature/AmazingFeature`
5. **Open Pull Request**

### Coding Standards
- Follow C# coding conventions
- Include XML documentation for public APIs
- Write unit tests for new features
- Update README for significant changes

## Troubleshooting

### Common Issues

#### Docker Connection Issues
```bash
# Reset Docker containers
docker-compose down -v
docker-compose up --build
```

#### Database Migration Issues
```bash
# Reset database
docker-compose down -v
docker volume rm bloghub_mssql_data
docker-compose up -d blogdb
dotnet ef database update -p BloggingAPI
```

#### Email Service Issues
- Ensure Gmail app-specific password is used
- Verify SMTP settings in environment variables
- Check firewall settings for port 587

#### Redis Connection Issues
```bash
# Check Redis container status
docker-compose logs redis_image

# Test Redis connectivity
docker exec -it blog-cache redis-cli ping
```

### Logging
Application logs are available in:
- **Console Output**: Real-time logging
- **File Logs**: `./logs/webapi-*.log`
- **Structured Logging**: JSON format for log aggregation

### Support
For additional support:
- Create an issue in the GitHub repository
- Check existing issues for similar problems
- Review the API documentation at `/swagger`

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**BlogHub** - Building the future of content management, one API at a time. 🚀
