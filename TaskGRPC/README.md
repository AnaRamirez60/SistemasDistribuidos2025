# gRPC Project Service (Node.js + MongoDB)

Este proyecto implementa una API gRPC para gestionar proyectos (title, id, priority, status y summary). Desarrollado con Node.js y MongoDB.

## Requerimientos
- Docker y docker compose instalado y en ejecución.

## Cómo levantar el proyecto
1. Clonar el repositorio

En tu terminal:

`git clone https://github.com/AnaRamirez60/SistemasDistribuidos2025.git`

`cd SistemasDistribuidos2025`

`cd TaskGRPC`

2. Levantar los servicios

Antes de ejecutar docker compose (la red es externa):

```
docker network create microservices_net
```

Luego:

`docker compose up --build -d`

## Instalación de grpcurl
macOS (Homebrew):

```
brew install grpcurl
```

Linux (bash):

```
curl -L https://github.com/fullstorydev/grpcurl/releases/latest/download/grpcurl_$(uname -s)_$(uname -m).tar.gz \
  | tar -xz grpcurl && sudo mv grpcurl /usr/local/bin/
```

Windows (PowerShell, usar binario de Releases) o WSL (usar comando Linux).

## Endpoints gRPC
- CreateProject (Unary)
- UpdateProject (Unary)
- DeleteProject (Unary)
- GetProjectById (Unary)
- ListProjects (Server streaming)
- BulkCreateProjects (Client streaming)

Proto: `proto/project.proto`

## Ejemplos con grpcurl (CLI)
CreateProject:
```
grpcurl -plaintext \
  -import-path proto \
  -proto project.proto \
  -d '{"title":"Test1","summary":"desc","priority":3}' \
  localhost:8086 project.ProjectService/CreateProject

```

GetProjectById (reemplaza <id> con ObjectId real):
```
grpcurl -plaintext \
  -import-path proto \
  -proto project.proto \
  -d '{"id":"<ID>"}' \
  localhost:8086 project.ProjectService/GetProjectById

```

UpdateProject:
```
grpcurl -plaintext \
  -import-path proto \
  -proto project.proto \
  -d '{
    "id":"692b9b0613b32752c88d87c8",
    "title":"NuevoTítulo",
    "summary":"Nueva descripción",
    "priority":5
  }' \
  localhost:8086 project.ProjectService/UpdateProject

```

DeleteProject:
```
grpcurl -plaintext \
  -import-path proto \
  -proto project.proto \
  -d '{"id":"<ID>"}' \
  localhost:8086 project.ProjectService/DeleteProject
```

ListProjects (todas):
```
grpcurl -plaintext \
  -import-path proto \
  -proto project.proto \
  -d '{}' \
  localhost:8086 project.ProjectService/ListProjects
```

ListProjects filtrando:
```
grpcurl -plaintext \
  -import-path proto \
  -proto project.proto \
  -d '{"filters": { "title": "p" }}' \
  localhost:8086 project.ProjectService/ListProjects
```

BulkCreateProjects (client streaming) creando archivo NDJSON:
```
grpcurl -plaintext \
  -import-path proto \
  -proto project.proto \
  -d @ \
  localhost:8086 project.ProjectService/BulkCreateProjects << 'EOF'
{"title":"Bulk 1","summary":"Desc A","priority":1}
{"title":"Bulk 2","summary":"Desc B","priority":2}
{"title":"Bulk 3","summary":"Desc C","priority":3}
EOF
```

## Ejemplos en Postman (gRPC)
- Importa el proto: abre Postman > New > gRPC Request > "Import a .proto file" y selecciona `proto/project.proto`. Configura el servidor como `localhost:8086`.

- CreateProject
  - Método: project.ProjectService/CreateProject
  - Body (JSON):
```
{"title":"p20","summary":"hbj","priority":3}
```

- GetProjectById
  - Método: project.ProjectService/GetProjectById
  - Body:
```
{ "id": <ID> }
```

- ListProjects (Server streaming)
  - Método: project.ProjectService/ListProjects
  - Body:
```
{
  "filters": { "title": "p" }
}
```

- UpdateProject
  - Método: project.ProjectService/UpdateProject
  - Body:
```
{"id": "<ID>","title":"p8","summary":"desc","priority":3, "status": "COMPLETED"}
```

- DeleteProject
  - Método: project.ProjectService/DeleteProject
  - Body:
```
{"id":"<ID>"}
```

- BulkCreateProjects (Client streaming)
  - Método: project.ProjectService/BulkCreateProjects
  - En Postman, usa "Client Streaming" y añade varias entradas CreateProjectRequest:
```
{ "title":"B1","summary":"s1","priority":1 }
```