import grpc
from google.protobuf.empty_pb2 import Empty
from app.core.config import settings
from fastapi import HTTPException, status
from typing import List, Dict, Optional
import re

# importar los stubs generados a partir del proto
from app.grpc_stubs.project_pb2 import (
    CreateProjectRequest,
    UpdateProjectRequest,
    GetProjectByIdRequest,
    ListProjectsRequest,
    Project
)
from app.grpc_stubs.project_pb2_grpc import ProjectServiceStub


# Configurar la conexionel canal GRPC y el cliente
try:
    channel = grpc.insecure_channel(settings.GRPC_SERVICE_URL)
    grpc_client = ProjectServiceStub(channel) #llamar al cliente GRPC
except Exception as e:
    raise RuntimeError(f"No se pudo conectar con gRPC {e}")

# Función para mapear errores GRPC a HTTPException
def _map_grpc_error(err: grpc.RpcError, context: str = ""):
    code = err.code()
    detail = err.details() or "grpc error"
    
    # Log del error
    if code == grpc.StatusCode.NOT_FOUND:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail=detail)
    elif code == grpc.StatusCode.INVALID_ARGUMENT:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail=detail)
    elif code == grpc.StatusCode.ALREADY_EXISTS:
        raise HTTPException(status_code=status.HTTP_409_CONFLICT, detail=detail)
    elif code == grpc.StatusCode.INTERNAL:
        raise HTTPException(status_code=status.HTTP_500_INTERNAL_SERVER_ERROR, detail="Error interno del servidor")
    else:
        raise HTTPException(status_code=status.HTTP_502_BAD_GATEWAY, detail=f"Error en servicio gRPC: {detail}")

# GetById
def getProjectByIdGrpc(project_id: str):
    try:
        # busca el proyecto por id        
        if not project_id:
            # si no viene id se marca como bad request
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="id requerido"
            )
        # buscar id con el cliente GRPC
        req = GetProjectByIdRequest(id=str(project_id))
        response = grpc_client.GetProjectById(req) #llamar al cliente GRPC
        return response

    # manejo de errores   
    except grpc.RpcError as err:
        _map_grpc_error(err, f"getProjectById({project_id})")
    except Exception as e:
        print(f"Error: {e}")
        raise

# Get All
def listProjectsGrpc(filters: Dict[str, str]):
    try:
        # Listar proyectos con filtros opcionales        
        filters_proto = {}
        for key, value in filters.items():
            # si el valor del filtro no es None, agregar al filtro
            if value is not None:
                filters_proto[key] = str(value)
        
        # crear la request y llamar al servicio GRPC
        req = ListProjectsRequest(filters=filters_proto)
        stream = grpc_client.ListProjects(req) #llamar al cliente GRPC
        projects = [proj for proj in stream]
        # devolver la lista de proyectos
        return projects

    # manejo de errores    
    except grpc.RpcError as err:
        _map_grpc_error(err, "listProjects")
    except Exception as e:
        print(f"Error: {e}")
        raise

#createProject
def createProjectGrpc(project):
    try:        
        # Validaciones para el tipo de datos que envia
        if not project.title or project.title.strip() == "":
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="title es obligatorio"
            )
            
        if not (1 <= project.priority <= 5):
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="priority debe ser entre 1 y 5"
            )

        # crear la request y llamar al servicio GRPC
        req = CreateProjectRequest(
            title=project.title.strip(),
            # summary puede estar vacio
            summary=project.summary or "",
            priority=project.priority,
        )
        
        # Manda la solicitud al servicio GRPC
        response = grpc_client.CreateProject(req) #llamar al cliente GRPC
        return response
    
    # manejo de errores
    except grpc.RpcError as err:
        _map_grpc_error(err, "createProject")
    except Exception as e:
        print(f"Error: {e}")
        raise

#updateProject
def updateProjectGrpc(project_id: str, project):
    try:        
        # Validaciones previas para la base de datos
        if not project_id:
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="id requerido"
            )
            
        if not project.title or project.title.strip() == "":
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="title es obligatorio"
            )
            
        if not (1 <= project.priority <= 5):
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="priority debe ser entre 1 y 5"
            )

        # crear la request y llamar al servicio GRPC
        req = UpdateProjectRequest(
            id=str(project_id),
            title=project.title.strip(),
            # summary puede estar vacio
            summary=project.summary or "",
            priority=project.priority,
            # valor por defecto si no viene status
            status=project.status or "PENDING",
        )
        
        # Manda la solicitud al servicio GRPC
        response = grpc_client.UpdateProject(req) #llamar al cliente GRPC
        return response
    # manejo de errores  
    except grpc.RpcError as err:
        _map_grpc_error(err, f"updateProject({project_id})")
    except Exception as e:
        print(f"Error: {e}")
        raise

#deleteProject
def deleteProjectGrpc(project_id: str):
    try:
        # Validaciones previas para la base de datos        
        if not project_id:
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="id requerido"
            )
        
        # crear la request y llamar al servicio GRPC    
        req = GetProjectByIdRequest(id=str(project_id))
        # GRPC devuelve empty
        response = grpc_client.DeleteProject(req) #llamar al cliente GRPC
        return True
        #manejo de errores
    except grpc.RpcError as err:
        _map_grpc_error(err, f"deleteProject({project_id})")
    except Exception as e:
        print(f"Error: {e}")
        raise

#bulkCreateProjects
def bulkCreateProjectsGrpc(projects: List):
    try:        
        # Validaciones para la base de datos
        for i, project in enumerate(projects):
            if not project.title or project.title.strip() == "":
                raise HTTPException(
                    status_code=status.HTTP_400_BAD_REQUEST,
                    detail=f"Proyecto {i+1} debe tener título"
                )
                
                # asignar prioridad por defecto si no es válida
            if not (1 <= project.priority <= 5):
                project.priority = 1

        # Para bulk create, crear un generador de requests
        def project_generator():
            for i, project in enumerate(projects):
                yield CreateProjectRequest(
                    title=project.title.strip(),
                    # summary puede estar vacio
                    summary=project.summary or "",
                    priority=project.priority,
                )
                
        # Llamar al servicio GRPC
        response = grpc_client.BulkCreateProjects(project_generator()) #llamar al cliente GRPC
           
        return response

       # manejo de errores 
    except grpc.RpcError as err:
        # Manejar específicamente el error de duplicados
        if err.code() == grpc.StatusCode.ALREADY_EXISTS:
            print(f"Proyecto duplicado {err.details()}")
            raise HTTPException(
                status_code=status.HTTP_409_CONFLICT,
                detail=err.details()
            )
        _map_grpc_error(err, "bulkCreateProjects")
    except Exception as e:
        print(f"Error: {e}")
        raise