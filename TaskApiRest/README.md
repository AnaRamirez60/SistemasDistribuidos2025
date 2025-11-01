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

    Configurar Entorno Virtual 
    Crea y activa un entorno virtual de Python para instalar las dependencias de forma aislada. Navega a la carpeta del proyecto antes de ejecutar:
    ```bash
    python3 -m venv .venv

    Activar el entorno (Linux/macOS)
    source .venv/bin/activate
    (Windows)
    .venv\Scripts\activate

    pip install -r requirements.txt
    ```

    Ejecuta el siguiente comando para levantar todos los contenedores necesarios:
    ```bash
    docker-compose up --build
    ```

## Autenticación y scopes

- Todas las peticiones son protegidas y requieren autorización:
    - `read` — para operaciones GET
    - `write` — para crear/actualizar/eliminar (POST, PUT, PATCH, DELETE)
    
## 2. Obtener Token de Autorización en Postman

1. Ve a la pestaña "Authorization" de tu petición o workspace
2. En el menú, selecciona **OAuth 2.0**
3. Configurar un Nuevo Token:
   - **Token Name**: Hydra Token
   - **Grant Type**: Client Credentials
   - **Access Token URL**: `http://localhost:4444/oauth2/token`
   - **Client ID**: `machine-client`
   - **Client Secret**: `machinesecret`
   - **Scope**: `read, write`
   - **Client Authentication**: Basic Auth Header
4. Haz clic en "Get New Access Token"
5. Usa el token generado

### Crear cliente en Hydra (si no existe)

Si tu entorno no tiene aún un cliente OAuth para usar con Client Credentials, crea uno usando la API Admin de Hydra (escucha en el puerto 4445). 

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
    }'
```

### Obtener token (Client Credentials)

Ejemplo usando curl (Client Credentials grant). Hydra expone el endpoint de token en el puerto 4444:

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

Usa el valor de `access_token` en la cabecera `Authorization: Bearer <token>` para probar los endpoints protegidos.

### Llamada de ejemplo a la API REST con el token

```bash
TOKEN="<pon-aqui-el-access-token>"
curl -s -H "Authorization: Bearer $TOKEN" \
    "http://localhost:8000/tasks/?page=1&pageSize=5" | jq '.'
```


## 3. Endpoints y Autorización Requerida

URL del WSDL: http://localhost:8001/task?wsdl

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

## 4. Ejemplos de Peticiones en Postman
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



