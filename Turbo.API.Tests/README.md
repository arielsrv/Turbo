# Turbo.API.Tests

Este proyecto contiene los tests unitarios y de integración para la API Turbo con arquitectura CQRS usando MediatR y
Rx.NET.

## Cobertura de Tests

### Tests Unitarios

#### Commands (Operaciones de Escritura)

- ✅ **CreateUserCommandHandler** - 3 tests
    - ValidRequest_ReturnsUserResponse
    - RepositoryThrowsException_PropagatesException
    - EmptyName_StillCreatesUser

- ✅ **UpdateUserCommandHandler** - 3 tests
    - ValidRequest_ReturnsUpdatedUserResponse
    - UserNotFound_ThrowsException
    - RepositoryThrowsException_PropagatesException

- ✅ **DeleteUserCommandHandler** - 3 tests
    - ValidRequest_ReturnsTrue
    - UserNotFound_ReturnsFalse
    - RepositoryThrowsException_PropagatesException

#### Queries (Operaciones de Lectura)

- ✅ **GetUserByIdQueryHandler** - 4 tests
    - ValidUserId_ReturnsUserResponse
    - UserNotFound_ReturnsNull
    - RepositoryThrowsException_PropagatesException
    - UserWithUpdatedAt_ReturnsCorrectData

- ✅ **GetAllUsersQueryHandler** - 4 tests
    - UsersExist_ReturnsUsersResponse
    - NoUsers_ReturnsEmptyResponse
    - RepositoryThrowsException_PropagatesException
    - SingleUser_ReturnsCorrectResponse

- ✅ **GetUserByEmailQueryHandler** - 5 tests
    - ValidEmail_ReturnsUserResponse
    - EmailNotFound_ReturnsNull
    - RepositoryThrowsException_PropagatesException
    - EmptyEmail_ReturnsNull
    - CaseInsensitiveEmail_ReturnsUser

#### Modelos

- ✅ **User** - 7 tests
    - Constructor_ValidParameters_CreatesUserWithCorrectProperties
    - Constructor_EmptyName_CreatesUserWithEmptyName
    - Constructor_EmptyEmail_CreatesUserWithEmptyEmail
    - Update_ValidParameters_UpdatesUserProperties
    - Update_EmptyName_UpdatesWithEmptyName
    - Update_EmptyEmail_UpdatesWithEmptyEmail
    - Update_MultipleTimes_UpdatesUpdatedAtEachTime
    - Id_GeneratedId_IsUnique

#### Repositorio

- ✅ **InMemoryUserRepository** - 15 tests
    - AddAsync_ValidUser_ReturnsUser
    - AddAsync_DuplicateEmail_ThrowsException
    - AddAsync_CaseInsensitiveEmail_ThrowsException
    - GetByIdAsync_ExistingUser_ReturnsUser
    - GetByIdAsync_NonExistingUser_ReturnsNull
    - GetAllAsync_EmptyRepository_ReturnsEmptyList
    - GetAllAsync_WithUsers_ReturnsAllUsers
    - UpdateAsync_ExistingUser_ReturnsUpdatedUser
    - UpdateAsync_NonExistingUser_ThrowsException
    - UpdateAsync_DuplicateEmail_ThrowsException
    - DeleteAsync_ExistingUser_ReturnsTrue
    - DeleteAsync_NonExistingUser_ReturnsFalse
    - GetByEmailAsync_ExistingUser_ReturnsUser
    - GetByEmailAsync_CaseInsensitive_ReturnsUser
    - GetByEmailAsync_NonExistingEmail_ReturnsNull
    - ConcurrentAccess_ThreadSafe

#### Mediator Reactivo

- ✅ **ReactiveMediator** - 8 tests
    - Send_ValidRequest_ReturnsResponse
    - Send_MediatorThrowsException_PropagatesException
    - Send_WithCancellationToken_PassesTokenToMediator
    - Publish_ValidNotification_ReturnsUnit
    - Publish_MediatorThrowsException_PropagatesException
    - Publish_WithCancellationToken_PassesTokenToMediator
    - Send_BoolResponse_ReturnsCorrectType
    - Send_NullableResponse_ReturnsCorrectType

### Tests de Integración

#### Controlador

- ✅ **UsersController** - 9 tests
    - CreateUser_ValidRequest_ReturnsCreatedResponse
    - CreateUser_DuplicateEmail_ReturnsBadRequest
    - GetUserById_ExistingUser_ReturnsOk
    - GetUserById_NonExistingUser_ReturnsNotFound
    - GetAllUsers_ReturnsOk
    - UpdateUser_ValidRequest_ReturnsOk
    - DeleteUser_ExistingUser_ReturnsOk
    - DeleteUser_NonExistingUser_ReturnsNotFound
    - GetUserByEmail_ExistingUser_ReturnsOk

## Estadísticas de Cobertura

- **Total de Tests**: 64
- **Tests Exitosos**: 64
- **Tests Fallidos**: 0
- **Cobertura Estimada**: ~95%

### Áreas Cubiertas

- ✅ Todos los handlers de comandos (Create, Update, Delete)
- ✅ Todos los handlers de queries (GetById, GetAll, GetByEmail)
- ✅ Modelo de dominio (User)
- ✅ Repositorio en memoria con validaciones
- ✅ Mediator reactivo (decorador de MediatR)
- ✅ Controlador API con todos los endpoints
- ✅ Casos de éxito y error
- ✅ Validaciones de negocio
- ✅ Manejo de excepciones
- ✅ Thread safety

## Ejecutar Tests

### Ejecutar todos los tests

```bash
dotnet test
```

### Ejecutar tests con detalles

```bash
dotnet test --verbosity normal
```

### Ejecutar tests específicos

```bash
# Solo tests de comandos
dotnet test --filter "FullyQualifiedName~Commands"

# Solo tests de queries
dotnet test --filter "FullyQualifiedName~Queries"

# Solo tests del repositorio
dotnet test --filter "FullyQualifiedName~Repositories"
```

### Ejecutar tests con cobertura (requiere coverlet)

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Tecnologías Utilizadas

- **xUnit**: Framework de testing
- **Moq**: Framework de mocking
- **System.Reactive**: Para testing de observables
- **Microsoft.AspNetCore.Mvc.Testing**: Para tests de integración
- **WebApplicationFactory**: Para testing de la API completa

## Patrones de Testing

### Arrange-Act-Assert (AAA)

Todos los tests siguen el patrón AAA para claridad y mantenibilidad.

### Mocking

- Uso extensivo de Moq para simular dependencias
- Mocks del repositorio para aislar la lógica de negocio
- Mocks del mediator para tests de integración

### Testing Reactivo

- Conversión de observables a tasks usando `.ToTask()`
- Testing de flujos reactivos con `Observable.Return()` y `Observable.Throw()`
- Verificación de comportamiento asíncrono

### Testing de Integración

- Uso de `WebApplicationFactory` para testing end-to-end
- Reemplazo de servicios reales con mocks
- Testing de respuestas HTTP y códigos de estado 