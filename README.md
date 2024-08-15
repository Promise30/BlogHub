# BloggingPlatformAPI
This project is a blogging platform API built with C# and .NET, inspired by popular blogging sites like Hashnode. It provides a comprehensive set of functionalities for user authentication, account management, and CRUD operations for posts and comments.

## Features
<ul>
  <li>User Authentication: Secure user registration and login using JWT.</li>
  <li>Account Management: Users can create, update, and manage their accounts.</li>
  <li>Posts CRUD Operations: Endpoints for creating, reading, updating, and deleting blog posts.</li>
  <li>Comments CRUD Operations: Endpoints for adding, updating, and deleting comments on posts.</li>
  <li>Image Upload: Integration with Cloudinary for image uploads.</li>
  <li>Mailing System: Gmail service integration for sending emails (e.g., account verification, password reset, new post and comment notification).</li>
</ul>

## Technologies Used
<ul>
  <li>C# and .NET 8: Backend development framework.</li>
  <li>SQL Server: Database management system.</li>
  <li>Cloudinary: Cloud-based image and video management service.</li>
  <li>Gmail API: Service for sending emails.</li>
</ul>

## Getting Started
### Prerequisites
<ul>
  <li>.NET SDK (version 8.0)</li>
  <li>SQL Server</li>
  <li>Cloudinary Account for image management.</li>
  <li>Gmail Account with less secure apps enabled.</li>
</ul>

## Installation
1. Clone the repository
2. Set up the database:
    - Create a new database in SQL Server.
    - Update the connection string in appsettings.json:
      <code>
          "ConnectionStrings": {
          "DefaultConnection": "Server=your_server_name;Database=your_database_name;User Id=your_user_id;Password=your_password;"
          }
      </code>
3. Configure Cloudinary:
    - Add your Cloudinary credentials to appsettings.json:
    <code>
    "Cloudinary": {
        "CloudName": "your_cloud_name",
        "ApiKey": "your_api_key",
        "ApiSecret": "your_api_secret"
    }
    </code>
4. Configure Gmail Service:
    - Add your Gmail credentials to appsettings.json:
    <code>
    "Gmail": {
        "Email": "your_email@gmail.com",
        "Password": "your_email_password"
    }
    </code>
5. Run the application:
    <code>dotnet run</code>
