Tecnologías:

- [.NET 9](https://dotnet.microsoft.com/)
- [ASP.NET Core Razor Pages](https://learn.microsoft.com/aspnet/core/razor-pages)
- [Entity Framework Core](https://learn.microsoft.com/ef/core)
- [PostgreSQL](https://www.postgresql.org/) (usando [Neon](https://neon.tech) como DBaaS)

---
## Instalación y configuración

### 1. Clonar el repositorio
```bash
git clone <url repo>
cd MicroMercado
```

### 2. Restaurar dependencias
```bash
dotnet restore
```

### 3. Configurar la cadena de conexión
```bash
dotnet user-secrets set ' '
```

### 4. Crear la base de datos con migraciones
```bash
dotnet ef database update
```

### 5. Ejecutar la aplicación
```bash
dotnet run
```