# Turbo API - CQRS con MediatR y Rx.NET

Este proyecto demuestra la implementación del patrón CQRS (Command Query Responsibility Segregation) usando MediatR y Rx.NET con un repositorio en memoria.

## Arquitectura

### CQRS (Command Query Responsibility Segregation)
- **Commands**: Operaciones de escritura (Create, Update, Delete)
- **Queries**: Operaciones de lectura (Get, GetAll, GetByEmail)

### Tecnologías Utilizadas
- **MediatR**: Para el patrón mediator y separación de comandos/consultas
- **Rx.NET**: Para programación reactiva en el repositorio
- **ASP.NET Core**: Para la API web

## Estructura del Proyecto

```
Turbo.API/
├── Commands/           # Operaciones de escritura
│   ├── CreateUserCommand.cs
│   ├── UpdateUserCommand.cs
│   └── DeleteUserCommand.cs
├── Queries/            # Operaciones de lectura
│   ├── GetUserByIdQuery.cs
│   ├── GetAllUsersQuery.cs
│   └── GetUserByEmailQuery.cs
├── Models/             # Modelos de dominio
│   └── User.cs
├── DTOs/               # Data Transfer Objects
│   └── UserDto.cs
├── Repositories/       # Capa de acceso a datos
│   ├── IUserRepository.cs
│   └── InMemoryUserRepository.cs
└── Controllers/        # Controladores de la API
    └── UsersController.cs
```

## Endpoints de la API

### Commands (Operaciones de Escritura)

#### Crear Usuario
```http
POST /api/users
Content-Type: application/json

{
  "name": "Juan Pérez",
  "email": "juan@example.com"
}
```

#### Actualizar Usuario
```http
PUT /api/users/{id}
Content-Type: application/json

{
  "name": "Juan Pérez Actualizado",
  "email": "juan.nuevo@example.com"
}
```

#### Eliminar Usuario
```http
DELETE /api/users/{id}
```

### Queries (Operaciones de Lectura)

#### Obtener Todos los Usuarios
```http
GET /api/users
```

#### Obtener Usuario por ID
```http
GET /api/users/{id}
```

#### Obtener Usuario por Email
```http
GET /api/users/email/{email}
```

## Características Destacadas

### 1. Separación CQRS Clara
- **Commands**: Manejan la lógica de negocio para modificaciones
- **Queries**: Solo retornan datos sin efectos secundarios

### 2. Uso de Rx.NET
- El repositorio utiliza `IObservable<T>` para operaciones reactivas
- Conversión a `Task<T>` usando `.ToTask()` para compatibilidad con async/await

### 3. MediatR como Mediator
- Desacoplamiento entre controladores y lógica de negocio
- Registro automático de handlers mediante reflection

### 4. Validaciones
- Validación de email único en creación y actualización
- Manejo de errores con respuestas HTTP apropiadas

## Ejemplo de Uso

### Crear un Usuario
```bash
curl -X POST "https://localhost:7001/api/users" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "María García",
    "email": "maria@example.com"
  }'
```

### Obtener Usuario Creado
```bash
curl -X GET "https://localhost:7001/api/users/{id}"
```

## Beneficios de esta Implementación

1. **Separación de Responsabilidades**: Commands y Queries están claramente separados
2. **Escalabilidad**: Fácil agregar nuevos comandos/consultas sin modificar código existente
3. **Testabilidad**: Cada handler puede ser testeado de forma independiente
4. **Reactividad**: Rx.NET permite operaciones asíncronas y reactivas
5. **Mantenibilidad**: Código organizado y fácil de entender

## Próximos Pasos

- Agregar validaciones más robustas
- Implementar logging y monitoreo
- Agregar autenticación y autorización
- Implementar persistencia en base de datos
- Agregar tests unitarios y de integración 