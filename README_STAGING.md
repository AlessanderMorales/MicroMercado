# ?? Configuración de Staging para MicroMercado

## Requisitos Previos

- .NET 9 SDK
- PostgreSQL (Neon como DBaaS)
- Visual Studio 2022 / VS Code / Rider

---

## ?? Configuración de Secretos para Staging

### **Opción 1: Usando `launchSettings.json` (Recomendado)**

1. Crea o edita el archivo `MicroMercado/Properties/launchSettings.json`
2. Agrega tu ConnectionString en las variables de entorno:

```json
{
  "MicroMercado-Staging": {
    "commandName": "Project",
    "launchBrowser": true,
    "applicationUrl": "https://localhost:7155;http://localhost:5156",
    "environmentVariables": {
      "ASPNETCORE_ENVIRONMENT": "Staging",
      "ConnectionStrings__DefaultConnection": "TU_CONNECTION_STRING_AQUI"
    }
  }
}
```

3. **IMPORTANTE:** Este archivo está en `.gitignore` y NO se subirá a GitHub.

---

### **Opción 2: Usando archivo `.env.staging`**

1. Crea un archivo `.env.staging` en la raíz del proyecto:

```bash
ConnectionStrings__DefaultConnection=Host=...;Database=...;Username=...;Password=...;
```

2. Ejecuta el script:

```powershell
.\run-staging.ps1
```

---

## ?? Ejecutar el Servidor en Staging

```powershell
cd MicroMercado
dotnet run --launch-profile "MicroMercado-Staging"
```

Deberías ver:

```
? Entorno actual: Staging
? Now listening on: https://localhost:7155
```

---

## ?? Ejecutar Pruebas de Selenium

**Terminal 1 (Servidor Staging):**
```powershell
cd MicroMercado
dotnet run --launch-profile "MicroMercado-Staging"
```

**Terminal 2 (Pruebas):**
```powershell
cd PruebasMicroMercado
dotnet test --filter "FullyQualifiedName~BlackBoxTests"
```

---

## ?? Obtener la ConnectionString

Si eres un nuevo miembro del equipo, solicita la cadena de conexión al administrador del proyecto.

**Formato:**
```
Host=<HOST>;Database=<DB>;Username=<USER>;Password=<PASS>;SSL Mode=VerifyFull;Channel Binding=Require
```

---

## ?? Seguridad

- **NUNCA** subas archivos con contraseñas a GitHub
- Archivos protegidos en `.gitignore`:
  - `launchSettings.json`
  - `appsettings.Staging.json`
  - `.env.staging`

---

## ?? Problemas Comunes

### Error: "The ConnectionString property has not been initialized"

**Solución:** Asegúrate de que la variable de entorno esté configurada en `launchSettings.json` o `.env.staging`.

### Error: "Connection refused" al ejecutar pruebas

**Solución:** Verifica que el servidor Staging esté corriendo en `https://localhost:7155`.

```powershell
netstat -ano | findstr :7155
```
