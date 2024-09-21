# Blog API Project

## Overview
This Blog API project is a robust backend solution for managing a blogging platform. Built with ASP.NET Core, it provides a RESTful API for creating, reading, updating, and deleting blog posts, managing users, and handling comments and post tags. It also provides a comprehensive set of functionalities for user authentication and account management. The project uses SQL Server for data persistence and Redis for caching, all containerized with Docker for easy deployment and scalability.

## Features
<ul>
  <li>User Authentication: Secure user registration and login using JWT.</li>
  <li>Account Management: Users can create, update, and manage their accounts.</li>
  <li>Posts CRUD Operations: Endpoints for creating, reading, updating, and deleting blog posts.</li>
  <li>Comments CRUD Operations: Endpoints for adding, updating, and deleting comments on posts.</li>
  <li>Tags CRUD Operations: Endpoints for creating, reading, updating and deleting tags.</li>
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
   ```
   https://github.com/Promise30/BlogApiService.git
   cd BlogApiService
   ```
2. Set up the project environment variables. Copy the .env.example file to a new file named .env and provide your own values:
   ```
   cp .env.example .env
   ```
3. Run the project
   ```
   docker-compose up
   ```

