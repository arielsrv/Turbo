# Turbo - CQRS API

A modern, high-performance CQRS (Command Query Responsibility Segregation) API built with .NET 9, featuring reactive programming patterns and clean architecture principles.

## 🚀 Features

- **CQRS Pattern**: Clear separation between commands (write operations) and queries (read operations)
- **Reactive Programming**: Built with System.Reactive for efficient event handling
- **Clean Architecture**: Well-structured layers with dependency injection
- **In-Memory Repository**: Fast development and testing with in-memory data storage
- **OpenAPI Support**: Auto-generated API documentation
- **Docker Support**: Containerized deployment ready
- **Comprehensive Testing**: Full test coverage with unit tests

## 🏗️ Architecture

The project follows CQRS pattern with the following structure:

```
Turbo.API/
├── Commands/          # Write operations (Create, Update, Delete)
├── Queries/           # Read operations (Get, GetAll)
├── Controllers/       # API endpoints
├── Models/            # Domain entities
├── DTOs/              # Data Transfer Objects
├── Repositories/      # Data access layer
├── Mediation/         # Command/Query handlers
└── Tests/             # Unit tests
```

## 🛠️ Technology Stack

- **.NET 9**: Latest .NET framework
- **ASP.NET Core**: Web API framework
- **MediatR**: Mediator pattern implementation
- **System.Reactive**: Reactive programming support
- **OpenAPI**: API documentation
- **Docker**: Containerization

## 📋 Prerequisites

- .NET 9 SDK
- Docker (optional, for containerized deployment)
- Your favorite IDE (Visual Studio, VS Code, Rider)

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd Turbo
```

### 2. Run the Application

#### Option A: Using .NET CLI

```bash
cd Turbo.API
dotnet restore
dotnet run
```

#### Option B: Using Docker

```bash
docker-compose up --build
```

### 3. Access the API

- **API Base URL**: `https://localhost:7001` (or `http://localhost:5001`)
- **OpenAPI Documentation**: `https://localhost:7001/openapi` (in development)

## 📚 API Documentation

### Users Endpoints

#### Create User
```http
POST /api/users
Content-Type: application/json

{
  "name": "John Doe",
  "email": "john.doe@example.com"
}
```

#### Get User by ID
```http
GET /api/users/{id}
```

#### Get User by Email
```http
GET /api/users/email/{email}
```

#### Get All Users
```http
GET /api/users
```

#### Update User
```http
PUT /api/users/{id}
Content-Type: application/json

{
  "name": "John Updated",
  "email": "john.updated@example.com"
}
```

#### Delete User
```http
DELETE /api/users/{id}
```

## 🧪 Testing

Run the test suite:

```bash
cd Turbo.API.Tests
dotnet test
```

## 🏗️ Project Structure

### Commands
- `CreateUserCommand`: Creates a new user
- `UpdateUserCommand`: Updates an existing user
- `DeleteUserCommand`: Deletes a user

### Queries
- `GetUserByIdQuery`: Retrieves a user by ID
- `GetUserByEmailQuery`: Retrieves a user by email
- `GetAllUsersQuery`: Retrieves all users

### Models
- `User`: Domain entity representing a user

### DTOs
- `UserDto`: Data transfer object for user operations
- `UserResponse`: Response model for user operations
- `UsersResponse`: Response model for multiple users

## 🔧 Configuration

The application uses standard ASP.NET Core configuration:

- `appsettings.json`: Production settings
- `appsettings.Development.json`: Development settings

## 🐳 Docker

The project includes Docker support with:

- `Dockerfile`: Multi-stage build for optimized container
- `compose.yaml`: Docker Compose configuration
- `.dockerignore`: Excludes unnecessary files from build context

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

If you encounter any issues or have questions:

1. Check the [Issues](../../issues) page
2. Create a new issue with detailed information
3. Contact the maintainers

## 🔮 Roadmap

- [ ] Database integration (Entity Framework Core)
- [ ] Event sourcing implementation
- [ ] Message queue integration
- [ ] Authentication and authorization
- [ ] API rate limiting
- [ ] Caching layer
- [ ] Performance monitoring

---

**Built with ❤️ using .NET 9 and CQRS patterns** 