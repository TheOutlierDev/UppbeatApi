# Uppbeat Library API

A secure, scalable RESTful API built with .NET for managing an uppbeat library. Features include JWT authentication, role-based authorization, and rate limiting.

## Features

- JWT-based authentication
- Role-based authorization (Artist and Regular users)
- RESTful API endpoints
- Rate limiting
- PostgreSQL database
- Docker support
- Swagger documentation

## Prerequisites

- Docker and Docker Compose
- .NET 9.0 SDK (for local development)
- Docker Desktop (for local development)

## Getting Started

### Running with Docker

1. Clone the repository:
```bash
git clone <repository-url>
cd UppbeatLibraryAPI
```

2. Build and run the containers:
```bash
docker-compose up --build
```

The API will be available at `http://localhost:5000` and Swagger documentation at `http://localhost:5000/swagger`.

### Local Development

1. Update the connection string in `appsettings.json` if needed
2. Run the following commands:
```bash
dotnet restore
dotnet build
dotnet run
```

## Docker Setup

The application is containerized using Docker and includes both the API and PostgreSQL database.

### Docker Files

#### Dockerfile
```dockerfile
# Use the official ASP.NET Core runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# Use the official ASP.NET Core SDK as a build image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["UppbeatLibraryAPI.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Build runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UppbeatLibraryAPI.dll"]
```

#### docker-compose.yml
```yaml
version: '3.8'

services:
  uppbeatlibraryapi:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - DOCKER_ENVIRONMENT=true
      - ASPNETCORE_URLS=http://+:80
      - ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=uppbeatapi;Username=uppbeatapp;Password=your_strong_password;
    depends_on:
      db:
        condition: service_healthy
    networks:
      - uppbeatlib-network

  db:
    image: postgres:16
    environment:
      POSTGRES_DB: uppbeatapi
      POSTGRES_USER: uppbeatapp
      POSTGRES_PASSWORD: your_strong_password
    ports:
      - "5433:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./scripts:/docker-entrypoint-initdb.d
      - ./pg_hba.conf:/etc/postgresql/pg_hba.conf:ro
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U uppbeatapp -d uppbeatapi"]
      interval: 5s
      timeout: 5s
      retries: 5
    networks:
      - uppbeatlib-network

networks:
  uppbeatlib-network:
    driver: bridge

volumes:
  postgres_data:
```

### Running with Docker

1. Make sure Docker and Docker Compose are installed on your system.

2. Clone the repository:
```bash
git clone <repository-url>
cd UppbeatLibraryAPI
```

3. Build and start the containers:
```bash
docker-compose up --build
```

The API will be available at:
- API: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger
- PostgreSQL: localhost:5433

### Environment Variables

The following environment variables can be configured in the docker-compose.yml:

#### API Service
- `ASPNETCORE_ENVIRONMENT`: Set to Development/Production
- `DOCKER_ENVIRONMENT`: Set to true when running in Docker
- `ConnectionStrings__DefaultConnection`: PostgreSQL connection string
- `Jwt__Key`: Your JWT secret key
- `Jwt__Issuer`: Token issuer (default: UppbeatLibraryAPI)
- `Jwt__Audience`: Token audience (default: UppbeatLibraryAPI)

#### Database Service
- `POSTGRES_DB`: Database name
- `POSTGRES_USER`: Database user
- `POSTGRES_PASSWORD`: Database password

### Database Initialization

The database is automatically initialized with required tables and seed data through scripts in the `/scripts` directory. The PostgreSQL instance is configured with a health check to ensure it's ready before starting the API.

### Persistence

Database data is persisted using a named volume (`postgres_data`). This ensures your data survives container restarts.

## API Endpoints

### Authentication

#### Token
```http
POST /Auth/token
Content-Type: application/json

{
    "userId": "00000000-0000-0000-0000-000000000000"
}
```

Response:
```json
{
    "token": "eyJhbGc..."
}
```

### Tracks

All track endpoints require authentication. Include the JWT token in the Authorization header:
```
Authorization: Bearer your-token-here
```

#### Get All Tracks
```http
GET /api/Track?genre=rock&search=love&page=1&pageSize=10
```

Response:
```json
[
    {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "title": "Love Song",
        "artist": "Artist Name",
        "genre": ["Rock"],
        "duration": 180
    }
]
```

Headers:
```
X-Total-Count: 1
```

#### Get Track by ID
```http
GET /api/Track/{id}
```

Response:
```json
{
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "Love Song",
    "artist": "Artist Name",
    "genre": ["Rock"],
    "duration": 180
}
```

#### Add New Track (Artist Role Required)
```http
POST /api/Track
Content-Type: application/json

{
    "title": "New Song",
    "artist": "Artist Name",
    "genre": "Pop",
    "duration": 240,
}
```

Response:
```json
{
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "New Song",
    "artist": "Artist Name",
    "genre": ["Pop"],
    "duration": 240
}
```

Headers:
```
Location: /api/Track/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

#### Update Track (Artist Role Required)
```http
PUT /api/Track/{id}
Content-Type: application/json

{
    "title": "Updated Song",
    "artist": "Artist Name",
    "genre": ["Pop"],
    "duration": 240
}
```

#### Delete Track (Artist Role Required)
```http
DELETE /api/Track/{id}
```

#### Download Track (Artist or Regular Role Required)
```http
GET /api/Track/{id}/download
```

## Authorization Roles

- **Artist**: Can perform all operations (CRUD)
- **Regular**: Can view and download tracks

## Rate Limiting

The API implements rate limiting with the following default rules:
- 10 requests per minute per IP address
- Configurable in `appsettings.json`

Rate Limit Response Headers:
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 99
X-RateLimit-Reset: 2025-01-26T20:27:46.7117172Z 
```

## Error Responses

The API returns standard HTTP status codes:

- 200: Success
- 201: Created (with Location header)
- 204: No Content
- 400: Bad Request
- 401: Unauthorized - "You must be logged in"
- 403: Forbidden - "You must be an Artist/Regular user"
- 404: Not Found - "Track not found"
- 429: Too Many Requests
- 500: Internal Server Error

Error Response Example:
```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "Bad Request",
    "status": 400,
    "detail": "The request is invalid",
    "errors": {
        "title": [
            "The Title field is required."
        ],
        "duration": [
            "The field Duration must be between 1 and 3600."
        ]
    }
}
```

## Environment Variables

The following environment variables can be configured in Docker:

```env
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Host=db;Database=uppbeatlibrary;Username=your_username;Password=your_password
Jwt__Key=your_secret_key
Jwt__Issuer=UppbeatLibraryAPI
Jwt__Audience=UppbeatLibraryAPI
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request
