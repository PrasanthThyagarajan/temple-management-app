# Temple API - Domain-Driven Architecture

## Overview

The Temple API has been refactored to follow Domain-Driven Design (DDD) principles with support for multiple database providers. This architecture provides flexibility to switch between different databases based on requirements while maintaining clean separation of concerns.

## 🏗️ Architecture

### Domain Layer
- **Entities**: Core business objects (Temple, Devotee, Donation, Event, Service)
- **BaseEntity**: Common properties and behavior for all entities
- **Value Objects**: Immutable objects representing concepts in the domain

### Infrastructure Layer
- **Data Access**: Entity Framework Core with multiple database provider support
- **Database Context Factory**: Factory pattern for creating database contexts
- **Migrations**: Support for both Code-First and Database-First approaches

### Application Layer
- **Services**: Business logic implementation
- **DTOs**: Data transfer objects for API communication
- **Interfaces**: Contracts for dependency injection

## 🗄️ Database Support

### Supported Providers
1. **PostgreSQL** (Default)
2. **SQLite** (File-based)
3. **SQL Server**
4. **MySQL**

### Configuration

The database provider can be configured in `appsettings.json`:

```json
{
  "Database": {
    "Provider": "PostgreSQL",
    "ConnectionString": "Host=localhost;Database=temple_management;Username=postgres;Password=password",
    "EnableMigrations": true,
    "EnableSensitiveDataLogging": false,
    "CommandTimeout": 30
  },
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Database=temple_management;Username=postgres;Password=password",
    "SQLite": "Data Source=temple_management.db",
    "SQLServer": "Server=localhost;Database=temple_management;Trusted_Connection=true;MultipleActiveResultSets=true",
    "MySQL": "Server=localhost;Database=temple_management;Uid=root;Pwd=password;"
  }
}
```

### Switching Database Providers

To switch to a different database:

1. **Update the Provider** in `appsettings.json`:
   ```json
   "Database": {
     "Provider": "SQLite"
   }
   ```

2. **Update the ConnectionString** or use the predefined connection string:
   ```json
   "Database": {
     "Provider": "SQLite",
     "ConnectionString": "Data Source=temple_management.db"
   }
   ```

3. **Restart the application**

## 🔧 Database Initialization

### Code-First Approach (Migrations)
When `EnableMigrations` is `true`:
- Entity Framework will apply pending migrations
- Database schema is created/updated based on migrations
- Supports incremental schema changes

### Database-First Approach
When `EnableMigrations` is `false`:
- Database is created if it doesn't exist
- Schema is created based on current entity models
- Useful for rapid prototyping

## 📦 NuGet Packages

### Core Packages
- `Microsoft.EntityFrameworkCore` - Core EF functionality
- `Microsoft.EntityFrameworkCore.Tools` - Migration tools
- `Microsoft.EntityFrameworkCore.Design` - Design-time tools

### Database Providers
- `Npgsql.EntityFrameworkCore.PostgreSQL` - PostgreSQL support
- `Microsoft.EntityFrameworkCore.Sqlite` - SQLite support
- `Microsoft.EntityFrameworkCore.SqlServer` - SQL Server support
- `Pomelo.EntityFrameworkCore.MySql` - MySQL support

## 🚀 Getting Started

### 1. Configure Database
Update `appsettings.json` with your preferred database provider and connection string.

### 2. Install Dependencies
```bash
dotnet restore
```

### 3. Run Migrations (if using Code-First)
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Start the Application
```bash
dotnet run
```

## 🔍 API Endpoints

### Database Information
- `GET /api/database/info` - Get current database configuration

### Health Check
- `GET /health` - Application health status

### Core Entities
- `GET /api/temples` - Get all temples
- `GET /api/devotees` - Get all devotees
- `GET /api/donations` - Get all donations
- `GET /api/events` - Get all events

## 🧪 Testing

### Unit Tests
```bash
dotnet test
```

### Integration Tests
The application includes integration tests for different database providers.

## 📝 Best Practices

### 1. Entity Design
- Use `BaseEntity` for common properties
- Implement proper validation using Data Annotations
- Use navigation properties for relationships

### 2. Database Configuration
- Keep connection strings secure
- Use appropriate timeouts for your database
- Enable retry policies for production databases

### 3. Migration Strategy
- Use migrations for production deployments
- Use `EnsureCreated` for development/testing
- Test migrations on staging environments first

## 🔄 Migration Scenarios

### Development to Production
1. Use SQLite for local development
2. Use PostgreSQL for staging
3. Use PostgreSQL for production

### Multi-Environment Support
```json
{
  "Database": {
    "Provider": "PostgreSQL",
    "ConnectionString": "${DATABASE_CONNECTION_STRING}",
    "EnableMigrations": true
  }
}
```

## 🚨 Troubleshooting

### Common Issues

1. **Connection String Errors**
   - Verify connection string format
   - Check database server accessibility
   - Validate credentials

2. **Migration Errors**
   - Ensure database exists
   - Check user permissions
   - Verify entity model consistency

3. **Provider Not Supported**
   - Install required NuGet package
   - Verify provider name spelling
   - Check .NET version compatibility

## 📚 Additional Resources

- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

## 🤝 Contributing

1. Follow DDD principles
2. Maintain clean separation of concerns
3. Add appropriate unit tests
4. Update documentation for new features

---

**Built with Domain-Driven Design principles for scalable temple management**
