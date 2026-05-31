# JWT Demo API

## Overview
This is an ASP.NET Core Web API that implements JWT authentication, role-based authorization, and Redis caching for optimized performance.

## Features
- JWT Authentication
- Role-based Authorization (Admin, Supervisor, etc.)
- Rate limiting
- ASP.NET Core Identity
- Redis Caching (Distributed Cache)
- Pagination for endpoints
- DTO-based responses

## Tech Stack
- .NET Web API
- Entity Framework Core
- .Net Identity
- Postgresql Server
- Redis
- JWT Bearer Authentication

## Caching Strategy
- Uses Redis via IDistributedCache
- Cache-aside pattern implemented
- DTOs are cached instead of entities
- Cache keys include pagination parameters

Example key:
products:page:1:size:10

## API Endpoints
Full API documentation is available via Scalar UI:
-https://localhost:7113
-http://localhost:5135

### Auth
- POST /api/v1/auth/login
- POST /api/v1/auth/register

### Products
- GET /api/products?pageNumber=1&pageSize=10

## Authentication
Uses JWT Bearer tokens. Add token in header:

Authorization: Bearer {token}

## Setup
1. Clone repository
2. Copy env variables from .env.example to create and configure your .env file
3. Run dotnet restore
3. Run migrations
4. Start Redis server
5. Run application

## Notes
- Redis must be running locally or via Docker
- Cache expires after configured TTL
