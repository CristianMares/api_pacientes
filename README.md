# api_pacientes

API REST para el catálogo de pacientes (ASP.NET Core 8 + SQLite).

## Endpoints

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/pacientes?buscar=&estatus=` | Lista con búsqueda por nombre/apellidos y filtro A/C |
| GET | `/api/pacientes/{id}` | Obtener uno |
| POST | `/api/pacientes` | Crear |
| PUT | `/api/pacientes/{id}` | Editar |
| PATCH | `/api/pacientes/{id}/cancelar` | Estatus a C |
| PATCH | `/api/pacientes/{id}/reactivar` | Estatus a A |

Cada paciente regresa además `imc` y `categoriaImc`. Peso en kg, altura en cm.

## Correr local

```
dotnet run --project ApiPacientes
```

Swagger en `http://localhost:5080/swagger`.

## Instalar en IIS

1. Descargar `api_pacientes_iis.zip` de la sección Releases y descomprimir.
2. Abrir PowerShell como administrador en esa carpeta y ejecutar:

```
powershell -ExecutionPolicy Bypass -File .\instalar-iis.ps1
```

El script activa IIS, instala el Hosting Bundle de .NET 8, crea el sitio en el puerto 5080, da permisos a la carpeta de la base y abre el puerto en el firewall. Al final muestra la IP que se usa en la app.
