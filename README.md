# BlogHub 

BlogHub is a C#/.NET REST API for creating and managing blog content. It provides full CRUD operations for blog posts, comments, tags, and user management with JWT authentication.


## Features

- **Blog Posts**: Create, read, update, and delete blog posts with cover images
- **Comments**: Add comments to posts with voting system (upvote/downvote)
- **Tags**: Organize posts with tagging system
- **User Management**: Complete authentication and authorization system
- **File Upload**: Image upload support via Cloudinary integration
- **Caching**: Redis-based caching for improved performance
- **Background Jobs**: Hangfire integration for email notifications
- Pagination, sorting, and filtering on list endpoints

## Tech Stack

- **.NET 8.0** - Framework
- **ASP.NET Core Web API** - Web framework
- **Entity Framework Core** - ORM
- **SQL Server** - Database
- **JWT Bearer** - Authentication
- **Redis** - Caching
- **Cloudinary** - Image storage
- **Hangfire** - Background job processing
- **Serilog** - Logging
- **Swagger/OpenAPI** - API documentation
- **Docker** - Containerization

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)
- Redis Server
- Cloudinary Account (for image upload)
- SMTP Server configuration (for email notifications)

### Clone

```bash
git clone https://github.com/Promise30/BlogHub.git
cd BlogHub
```

### Configure

Update `appsettings.json` (and/or `appsettings.Development.json`) with your connection string and any other settings.

Example (adjust to your environment):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=BlogHub;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### Database

If using EF Core, apply migrations:

```bash
dotnet ef database update
```
If you are not using EF Core, remove this section and document your actual data setup.

### Run

From the project directory that contains your web API entry point:

```bash
dotnet restore
dotnet build
dotnet run
```

By default, ASP.NET Core listens on:
- http://localhost:5174 and https://localhost:7156

## Using the API

Below are some of the API endpoints:
### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login

### Posts
- `GET /api/posts/user-posts` - Get user's posts
- `POST /api/posts` - Create new post

### Comments
- `POST /api/comments/posts/{id}` - Create comment
- `PUT /api/comments/{id}` - Update comment


## Docker Support

Build and run with Docker:
```bash
docker build -t blogging-api . docker run -p 8080:80 blogging-api
```
Or use Docker Compose:
```bash
docker-compose up -d
```


## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.
