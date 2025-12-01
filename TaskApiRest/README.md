# API REST de Gestión de Tareas (FastAPI + Hydra)

Este proyecto implementa una api rest que se conecta sobre el servicio soap del Parcial 1. Expone operaciones CRUD completas para la gestión de tareas, añadiendo caché distribuido con Redis y autenticación con OAuth2 con Ory Hydra.

Está desarrollado en Python utilizando el framework FastAPI.

## Requerimientos

* Docker instalado y en ejecución.
* El proyecto `TaskApi` (Parcial 1) debe estar en una carpeta paralela.

## 1. Cómo Levantar el Proyecto

1.  **Clonar el Repositorio**
    ```bash
    git clone https://github.com/AnaRamirez60/SistemasDistribuidos2025.git
    cd SistemasDistribuidos2025
    ```

2.  **Verificar Estructura**
    El `docker-compose.yml` espera que el proyecto del Parcial 1 (`TaskApi`) esté en la misma carpeta.

3.  **Levantar los Servicios**
    Navega a la carpeta de este proyecto:
    ```bash
    cd TaskApiRest
    ```
    Ejecuta el siguiente comando para levantar todos los contenedores necesarios:
    ```bash
    docker compose up --build -d
    ```

## Autenticación y scopes

- Todas las peticiones son protegidas y requieren autorización:
    - `read` — para operaciones GET
    - `write` — para crear/actualizar/eliminar (POST, PUT, PATCH, DELETE)

## 2. Opciones llamando al api soap:
## A. Usar el Script Automatizado

El script probar_soap.sh(para mac) o el contenedor de helper hará todo:
Creará el cliente de Hydra (si no existe).
Pedirá un token de acceso.
Hará una prueba de los endpoints con ese token.

Puedes usar el documento con las mismas peticiones y la autenticación:

    ```bash
    xattr -d com.apple.quarantine probar_soap.sh
    chmod +x probar_soap.sh
    ./probar_soap.sh
    ```

## B. Obtener Token de Autorización en Postman

1. Ve a la pestaña "Authorization" de tu petición o workspace
2. En el menú, selecciona **OAuth 2.0**
3. Configurar un Nuevo Token:
   - **Token Name**: Hydra Token
   - **Grant Type**: Client Credentials
   - **Access Token URL**: `http://localhost:4444/oauth2/token`
   - **Client ID**: `machine-client`
   - **Client Secret**: `machinesecret`
   - **Scope**: `read` y otro de `write`
   - **Client Authentication**: Basic Auth Header
4. Haz clic en "Get New Access Token"
5. Usa el token generado

### C. Crear cliente en Hydra (si no existe)

Si tu entorno no tiene aún un cliente OAuth para usar con Client Credentials, crea uno de Hydra (escucha en el puerto 4445). 

```bash
curl -s -X POST http://localhost:4445/clients \
    -H "Content-Type: application/json" \
    -d '{
        "client_id": "machine-client",
        "client_secret": "machinesecret",
        "grant_types": ["client_credentials"],
        "response_types": ["token"],
        "scope": "read write",
        "token_endpoint_auth_method": "client_secret_basic"
    }' | jq '.'
```

### Obtener token 

Hydra expone el endpoint de token en el puerto 4444:

```bash
curl -s -X POST "http://localhost:4444/oauth2/token" \
    -u "machine-client:machinesecret" \
    -d "grant_type=client_credentials&scope=read write"
```

Respuesta ejemplo (JSON):

```json
{
    "access_token": "...",
    "token_type": "bearer",
    "expires_in": 3600,
    "scope": "read write"
}
```

Copia el valor de `access_token`.

### Hacer Petición a la api
Reemplaza <TOKEN> con el token que copiaste.

```bash
TOKEN="<TOKEN>"
curl -s -H "Authorization: Bearer $TOKEN" \
    "http://localhost:8000/tasks/?page=1&pageSize=5" | jq '.'
```


## 3. Endpoints y Autorización Requerida

URL del WSDL: http://localhost:8001/task?wsdl

URL de swagger: http://localhost:8000/docs#/

URL del endpoint del servicio para realizar peticiones: http://localhost:8000/tasks

| Método | Endpoint | Autorización | Descripción |
|--------|----------|--------------|-------------|
| **GET** | `http://localhost:8000/tasks/?page=1&pageSize=5` | `read` | Obtener lista paginada de tareas |
| **GET** | `http://localhost:8000/tasks/getByTitle?title=T` | `read` | Buscar tareas por título |
| **GET** | `http://localhost:8000/tasks/1` | `read` | Obtener tarea por ID |
| **POST** | `http://localhost:8000/tasks/` | `write` | Crear nueva tarea |
| **PUT** | `http://localhost:8000/tasks/1` | `write` | Actualizar tarea completa |
| **PATCH** | `http://localhost:8000/tasks/1` | `write` | Actualizar parcialmente tarea |
| **DELETE** | `http://localhost:8000/tasks/7` | `write` | Eliminar tarea |


## 4. Ejemplos en terminal
- Get By Title:
```bash
curl -s -H "Authorization: Bearer $TOKEN" \
    "http://localhost:8000/tasks/getByTitle?title=T" | jq '.'
```

- Get By Id:
```bash
curl -s -H "Authorization: Bearer $TOKEN" \
    "http://localhost:8000/tasks/31" | jq '.'
```
- Create Task:
```bash
curl -s -X POST "http://localhost:8000/tasks/" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"title":"T5","description":"Descripción","endDate":"2025-12-31"}' | jq '.'
```

- Put Task:
```bash
curl -s -X PUT "http://localhost:8000/tasks/31" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"title":"Actualizado","description":"Descripción nueva","isCompleted":true,"endDate":"2026-01-20"}' | jq '.'
```

- Patch Task:

```bash
curl -s -X PATCH "http://localhost:8000/tasks/31" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"isCompleted":false}' | jq '.'
```

- Delete Task:

```bash
curl -s -X DELETE "http://localhost:8000/tasks/7" \
    -H "Authorization: Bearer $TOKEN" | jq '.'
```

## 5. Ejemplos en Postman
- Get By Title:

```bash
 GET "http://localhost:8000/tasks/getByTitle?title=T" \
```

- Get By Id:

```bash
GET "http://localhost:8000/tasks/1" \
```

- Create Task:

```bash
 POST "http://localhost:8000/tasks/" 

'{"title":"T5","description":"Descripción","endDate":"2024-12-31"}'
```

- Put Task:

```bash
PUT "http://localhost:8000/tasks/1" \
    
'{"title":"Título Actualizado","description":"Descripción nueva","isCompleted":true,"endDate":"2024-01-20"}'
```

- Patch Task:

```bash
PATCH "http://localhost:8000/tasks/1" \

'{"isCompleted":false}'
```

- Delete Task:

```bash
DELETE "http://localhost:8000/tasks/7" \
```

## 6. Servicios desplegados

| Servicio        | URL                         | Puerto | Descripción                  |
|-----------------|-----------------------------|--------|------------------------------|
| API REST        | http://localhost:8000       | 8000   | API principal (FastAPI)      |
| API SOAP   | http://localhost:8001       | 8001   | Servicio SOAP original       |
| Redis           | redis://localhost:6379      | 6379   | Caché distribuido            |
| Hydra (OAuth2)  | http://localhost:4444       | 4444   | Servidor OAuth2 (Hydra)      |
| Hydra Admin     | http://localhost:4445       | 4445   | API Admin de Hydra           |
| BD     | http://localhost:3307      | 3307   | Base de datos           |


## 3. Opciones llamando al api GRPC:
## A. Usar el Script Automatizado

El script probar_grpc.sh(para mac) o el contenedor de helper hará todo:
Creará el cliente de Hydra (si no existe).
Pedirá un token de acceso.
Hará una prueba de los endpoints con ese token.

Puedes usar el documento con las mismas peticiones y la autenticación:

    ```bash
    xattr -d com.apple.quarantine probar_grpc.sh
    chmod +x probar_grpc.sh
    ./probar_grpc.sh
    ```

## B. Obtener Token de Autorización en Postman

1. Ve a la pestaña "Authorization" de tu petición o workspace
2. En el menú, selecciona **OAuth 2.0**
3. Configurar un Nuevo Token:
   - **Token Name**: Hydra Token
   - **Grant Type**: Client Credentials
   - **Access Token URL**: `http://localhost:4444/oauth2/token`
   - **Client ID**: `machine-client`
   - **Client Secret**: `machinesecret`
   - **Scope**: `read` y otro de `write`
   - **Client Authentication**: Basic Auth Header
4. Haz clic en "Get New Access Token"
5. Usa el token generado

### C. Crear cliente en Hydra (si no existe)

Si tu entorno no tiene aún un cliente OAuth para usar con Client Credentials, crea uno de Hydra (escucha en el puerto 4445). 

```bash
curl -s -X POST http://localhost:4445/clients \
    -H "Content-Type: application/json" \
    -d '{
        "client_id": "machine-client",
        "client_secret": "machinesecret",
        "grant_types": ["client_credentials"],
        "response_types": ["token"],
        "scope": "read write",
        "token_endpoint_auth_method": "client_secret_basic"
    }' | jq '.'
```

### Obtener token 

Hydra expone el endpoint de token en el puerto 4444:

```bash
curl -s -X POST "http://localhost:4444/oauth2/token" \
    -u "machine-client:machinesecret" \
    -d "grant_type=client_credentials&scope=read write"
```

Respuesta ejemplo (JSON):

```json
{
    "access_token": "...",
    "token_type": "bearer",
    "expires_in": 3600,
    "scope": "read write"
}
```

Copia el valor de `access_token`.

### Hacer Petición a la api
Reemplaza <TOKEN> con el token que copiaste.

```bash
TOKEN="<TOKEN>"
curl -s -H "Authorization: Bearer $TOKEN" \
    "http://localhost:8000/tasks/?page=1&pageSize=5" | jq '.'
```


## 3. Endpoints y Autorización Requerida

URL del endpoint del servicio para realizar peticiones: http://localhost:8000/projects/

| Método | Endpoint | Autorización | Descripción |
|--------|----------|--------------|-------------|
| **GET** | `http://localhost:8000/projects/` | `read` | Obtener lista completa de proyectos |
| **GET** | `http://localhost:8000/projects/<ID>` | `read` | Buscar proyectos por id |
| **GET** | `http://localhost:8000/projects/?title=y` | `read` | Obtener proyecto por titulo |
| **POST** | `http://localhost:8000/projects/` | `write` | Crear nuevo proyecto |
| **PUT** | `http://localhost:8000/projects/<ID>` | `write` | Actualizar proyecto completa |
| **POST** | `http://localhost:8000/projects/bulk` | `write` | Crea grupo de proyectos |
| **DELETE** | `http://localhost:8000/projects/<ID>` | `write` | Eliminar proyecto |


## 4. Ejemplos en terminal
- Get All:

```bash
curl -s -H "Authorization: Bearer $TOKEN" \
    "http://localhost:8000/projects/" | jq '.'
```

- Get By Id:

```bash
curl -s -H "Authorization: Bearer $TOKEN" \
    "http://localhost:8000/projects/692ca9cbb001b4b9ced4b95b" | jq '.'
```

- Get por Filtro:

```bash
curl -s -H "Authorization: Bearer $TOKEN" \
    "http://localhost:8000/projects/?title=y" | jq '.'
```

- Create Project:

```bash
curl -s -X POST "http://localhost:8000/projects/" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $TOKEN" \
    -d '{"title":"Test12","summary":"desc","priority":3}' | jq '.'
```

- Put Project:

```bash
curl -s -X PUT "http://localhost:8000/projects/692ca973b001b4b9ced4b959" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $TOKEN" \
    -d '{"title": "Nuevo", "summary": "Resumen", "priority": 2, "status": "IN_PROGRESS"}' | jq '.'
```

- Create Group of Projects:

```bash
curl -s -X POST "http://localhost:8000/projects/bulk" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $TOKEN" \
    -d '[
        {
            "title": "Proyecto 13",
            "summary": "Primer proyecto del lote.",
            "priority": 5
        },
        {
            "title": "Proyecto 14",
            "summary": "Segundo proyecto del lote.",
            "priority": 3,
            "status": "PENDING"
        }
    ]' | jq '.'
```

- Delete Task:

```bash
curl -s -X DELETE "http://localhost:8000/projects/692ca9cbb001b4b9ced4b95b" \
    -H "Authorization: Bearer $TOKEN" | jq '.'
```


## 5. Ejemplos en Postman
- Get All:

```bash
 GET "http://localhost:8000/projects/" \
```

- Get By Id:

```bash
GET "http://localhost:8000/projects/692c18df418730ee8309f888" \
```

- Get por Filtro:

```bash
GET "http://localhost:8000/projects/?title=y" \
```

- Create Project:

```bash
 POST "http://localhost:8000/projects/" 

'{"title":"Test12","summary":"desc","priority":3}'
```

- Put Project:

```bash
PUT "http://localhost:8000/projects/692c18df418730ee8309f888" \
    
'{"title": "Nuevo Título Actualizado", "summary": "Resumen revisado para el proyecto X.", "priority": 2, "status": "IN_PROGRESS"}'
```

- Create Group of Projects:

```bash
POST "http://localhost:8000/projects/bulk" \

'[
    {
        "title": "Proyecto Masivo 12",
        "summary": "Primer proyecto del lote.",
        "priority": 5
    },
    {
        "title": "Proyecto Masivo 13",
        "summary": "Segundo proyecto del lote.",
        "priority": 3,
        "status": "PENDING"
    }
]'
```

- Delete Task:

```bash
DELETE http://localhost:8000/projects/692c18df418730ee8309f888 \
```

## 6. Servicios desplegados

| Servicio        | URL                         | Puerto | Descripción                  |
|-----------------|-----------------------------|--------|------------------------------|
| API REST        | http://localhost:8000       | 8000   | API principal (FastAPI)      |
| API SOAP        | http://localhost:8001       | 8001   | Servicio SOAP original       |
| Redis           | redis://localhost:6379      | 6379   | Caché distribuido            |
| Hydra (OAuth2)  | http://localhost:4444       | 4444   | Servidor OAuth2 (Hydra)      |
| Hydra Admin     | http://localhost:4445       | 4445   | API Admin de Hydra           |
| MySQL (SOAP)    | localhost:3307              | 3307   | Base de datos SOAP           |
| PostgreSQL (Hydra) | localhost:5433           | 5433   | Base de datos Hydra          |
| MongoDB (gRPC)  | localhost:27017             | 27017  | Base de datos gRPC           |
| API gRPC        | localhost:8086              | 8086   | Servicio gRPC                |

