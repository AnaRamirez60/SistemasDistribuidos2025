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

## 2. Tres opciones:
## A. Usar el Script Automatizado

El script probar_api.sh(para mac) o el contenedor de helper hará todo:
Creará el cliente de Hydra (si no existe).
Pedirá un token de acceso.
Hará una prueba de los endpoints con ese token.

puedes usar para el contenedor con las peticiones y la autenticación:

    ```bash
    docker-compose run --rm helper
    ```

o puedes usar el documento con las mismas peticiones y la autenticación:

    ```bash
    chmod +x probar_api.sh
    ./probar_api.sh
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
    "http://localhost:8000/tasks/1" | jq '.'
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
curl -s -X PUT "http://localhost:8000/tasks/1" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"title":"Título Actualizado","description":"Descripción nueva","isCompleted":true,"endDate":"2025-01-20"}' | jq '.'
```

- Patch Task:

```bash
curl -s -X PATCH "http://localhost:8000/tasks/1" \
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

## 7. Problemas con BuildKit en macOS (cache corrupta)

- Builds fallan con errores relacionados con la cache o BuildKit.
- Docker Desktop muestra mensajes de "cache corrupt" o fallos repetidos al construir imágenes.
- Contenedores que no arrancan por errores de BuildKit.

1) Hacer backup opcional (si hay imágenes/volúmenes importantes):
```bash
docker save -o myimage.tar myimagen:tag
```

2) Limpiar caches de BuildKit y builder:
```bash
docker buildx prune --all --force

docker builder prune --all --force

# elimina imágenes, contenedores detenidos y volúmenes (opcional)
docker system prune --all --volumes --force
```

3) Forzar reconstrucción sin caché / desactivar BuildKit temporalmente:
```bash
# Desactivar BuildKit solo para esta ejecución
DOCKER_BUILDKIT=0 docker compose build --no-cache

docker buildx build --no-cache --load .
```

4) Reiniciar Docker Desktop y volver a levantar:

```bash
docker compose up --build -d
```




