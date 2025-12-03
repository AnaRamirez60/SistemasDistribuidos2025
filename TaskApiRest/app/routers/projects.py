from fastapi import APIRouter, Depends, HTTPException, status, Query, Response, Request
from redis import Redis
from typing import Optional, List, Any, Dict
from app.schemas.project import ProjectResponse, ProjectCreate, ProjectUpdate
from app.services import grpc_client
from app.dependencies.cache import get_redis_connection, get_cache, set_cache, invalidate_project_cache
from app.dependencies.authentication import get_auth_dependency
import grpc
import traceback
import urllib.parse
from pydantic import ValidationError

# Configuración del router
router = APIRouter(
    prefix="/projects",
    tags=["Projects"]
)

# Dependencias de autenticación
auth_read = Depends(get_auth_dependency("read"))
auth_write = Depends(get_auth_dependency("write"))

# Función para manejar errores GRPC a HTTPException
def _handle_grpc_error(error, detail_message: str):
    if hasattr(error, 'code'):
        code = error.code()
        detail = error.details() or "grpc error"
                
        if code == grpc.StatusCode.INVALID_ARGUMENT:
            raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail=detail)
        elif code == grpc.StatusCode.NOT_FOUND:
            raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail=detail)
        elif code == grpc.StatusCode.ALREADY_EXISTS:
            raise HTTPException(status_code=status.HTTP_409_CONFLICT, detail=detail)
        elif code == grpc.StatusCode.INTERNAL:
            raise HTTPException(status_code=status.HTTP_500_INTERNAL_SERVER_ERROR, detail="Error interno del servidor")
    
    # Si es otro tipo de error
    print(f"Error: {type(error)} - {error}")
    raise HTTPException(
        status_code=status.HTTP_500_INTERNAL_SERVER_ERROR, 
        detail=f"Error interno del servidor {str(error)}"
    )

# Función para verificar que no exista el título
def ensure_title_not_conflicting(title: str, current_id: Optional[str] = None):
    if not title or title.strip() == "":
        return
    
    # Buscar proyectos con el mismo título
    try:
        existing_projects = grpc_client.listProjectsGrpc({"title": title})
        
        # Verificar si alguno coincide
        for project in existing_projects:
            if project.title.strip().lower() == title.strip().lower():
                if current_id is None:
                    raise HTTPException(
                        status_code=status.HTTP_409_CONFLICT,
                        detail=f"Un proyecto con el título '{title}' ya existe."
                    )
                if project.id != current_id:
                    raise HTTPException(
                        status_code=status.HTTP_409_CONFLICT,
                        detail=f"Un proyecto con el título '{title}' ya existe."
                    )
    except Exception as e:
        print(f"Error: {e}")

# Función para normalizar el id del proyecto
def _normalize_project_id(project_id: str) -> str:
    # sirve para limpiar espacios y caracteres extraños
    project_id = project_id.strip()
    
    # Decodificar id
    if '%' in project_id:
        project_id = urllib.parse.unquote(project_id)
    
    # Remover cualquier caracter extraño
    project_id = project_id.replace('\n', '').replace('\r', '').strip()
    
    return project_id

# Endpoint para getById
@router.get("/{project_id}", response_model=ProjectResponse)
def getProjectById(
    project_id: str,
    request: Request = None,
    redis: Redis = Depends(get_redis_connection), 
    _auth: bool = auth_read
):
    try:
        # Normaliza el id
        project_id = _normalize_project_id(project_id)
        cache_key = f"project:{project_id}"

        # Intentar obtener de caché
        cached = get_cache(cache_key, redis)
        if cached:
            return ProjectResponse.model_validate(cached)

        # Llamar al servicio GRPC
        res = grpc_client.getProjectByIdGrpc(project_id)
        
        # Construir la respuesta
        res_dict = {
            "id": res.id,
            "title": res.title,
            "summary": res.summary,
            "priority": res.priority,
            "status": res.status or "PENDING"
        }

        # Guardar en caché
        set_cache(cache_key, res_dict, redis)
        return ProjectResponse.model_validate(res_dict)
    
    # manejo de errores
    except grpc.RpcError as e:
        _handle_grpc_error(e, f"Error al obtener proyecto con id {project_id}")
    except HTTPException as he:
        raise he
    except Exception as e:
        traceback.print_exc()
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Error interno del servidor: {str(e)}"
        )

# Endpoint para GetAll con filtros
@router.get("/", response_model=List[ProjectResponse])
def getProjects(
    # Filtros opcionales
    title: Optional[str] = Query(None, description="Filtrar por título exacto"),
    status_filter: Optional[str] = Query(None, alias="status", description="Filtrar por status"),
    priority: Optional[int] = Query(None, ge=1, le=5, description="Filtrar por prioridad"),
    request: Request = None,
    _auth: bool = auth_read
):
    try:
        # Construir filtros explícitos para mandarlos GRPC
        parsed_filters = {}
        if title:
            parsed_filters["title"] = title.strip()
        if status_filter:
            parsed_filters["status"] = status_filter.strip()
        if priority is not None:
            parsed_filters["priority"] = str(priority)

        # Llamar al servicio GRPC
        grpc_items = grpc_client.listProjectsGrpc(parsed_filters)
        # Construir la lista de respuestas
        return [
            ProjectResponse(
                id=p.id,
                title=p.title,
                summary=p.summary,
                priority=p.priority,
                status=(p.status or "PENDING")
            )
            for p in grpc_items
        ]
    
    # manejo de errores
    except grpc.RpcError as e:
        _handle_grpc_error(e, "Error al imprimir proyectos")
    except HTTPException as he:
        raise he
    except Exception as e:
        traceback.print_exc()
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Error interno del servidor: {str(e)}"
        )

# endpoint para createProject 
@router.post("/", response_model=ProjectResponse, status_code=status.HTTP_201_CREATED)
def createProject(
    project_data: Dict[str, Any], # Recibir como dict
    request: Request = None,
    response: Response = None,
    redis: Redis = Depends(get_redis_connection),
    _auth: bool = auth_write # Dependencia de autenticación
):
    try:
        # Validación del titulo
        if not project_data or 'title' not in project_data:
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="El título es obligatorio"
            )
        
        # validación de Pydantic
        try:
            project = ProjectCreate(**project_data)
        except ValidationError as pydantic_error:
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail=f"Error: {pydantic_error.errors()}"
            )
        
        # Validaciones 
        if not project.title or project.title.strip() == "":
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="El título es obligatorio"
            )
        
        if project.priority and not (1 <= project.priority <= 5):
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="La prioridad debe ser entre 1 y 5"
            )

        # Verificar conflicto de título
        ensure_title_not_conflicting(project.title)

        # Llamar al servicio GRPC
        res = grpc_client.createProjectGrpc(project)
        
        # Invalidar caché porque hay un nuevo proyecto
        invalidate_project_cache(redis)
        
        # Configurar header Location
        if response and request:
            # Construir la url del nuevo recurso
            url = request.url_for("getProjectById", project_id=res.id)
            response.headers["Location"] = str(url)
            
        # Devolver respuesta
        return ProjectResponse(
            id=res.id,
            title=res.title,
            summary=res.summary,
            priority=res.priority,
            status=(res.status or "PENDING")
        )
    
    # manejo de errores
    except HTTPException as he:
        raise he
    except Exception as e:
        traceback.print_exc()
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Error interno del servidor: {str(e)}"
        )

# endpoint para updateProject
@router.put("/{project_id}", response_model=ProjectResponse)
def updateProject(
    project_id: str,
    project_data: Dict[str, Any], 
    request: Request = None,
    redis: Redis = Depends(get_redis_connection),
    _auth: bool = auth_write # Dependencia de autenticación
):
    try:
        # Normaliza el Id
        project_id = _normalize_project_id(project_id)
        
        # Validaciones 
        if not project_id:
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="ID del proyecto es requerido"
            )
        
        if not project_data or 'title' not in project_data:
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="El título es obligatorio"
            )
        
        # validación de Pydantic
        try:
            project = ProjectUpdate(**project_data)
        except ValidationError as pydantic_error:
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail=f"Error de validación: {pydantic_error.errors()}"
            )
        
        # Validaciones 
        if not project.title or project.title.strip() == "":
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="El título es obligatorio"
            )
        
        if project.priority and not (1 <= project.priority <= 5):
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="La prioridad debe ser entre 1 y 5"
            )

        # Verificar conflicto de título
        ensure_title_not_conflicting(project.title, project_id)

        # Llamar al servicio GRPC
        res = grpc_client.updateProjectGrpc(project_id, project)
        
        # Invalidar caché por la actualización
        invalidate_project_cache(redis, project_id)
        
        # Devolver respuesta
        return ProjectResponse(
            id=res.id,
            title=res.title,
            summary=res.summary,
            priority=res.priority,
            status=(res.status or "PENDING")
        )
    
    # manejo de errores
    except grpc.RpcError as e:
        _handle_grpc_error(e, f"Error: {project_id}")
    except HTTPException as he:
        raise he
    except Exception as e:
        traceback.print_exc()
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Error interno del servidor: {str(e)}"
        )

# endpoint para deleteProject
@router.delete("/{project_id}", status_code=status.HTTP_204_NO_CONTENT)
def deleteProject(
    project_id: str,
    request: Request = None,
    redis: Redis = Depends(get_redis_connection),
    _auth: bool = auth_write
):
    try:
        # Normaliza el Id
        project_id = _normalize_project_id(project_id)
        
        if not project_id:
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="ID del proyecto es requerido"
            )

        # Llamar al servicio GRPC
        grpc_client.deleteProjectGrpc(project_id)
        
        # Invalidar caché por la eliminación
        invalidate_project_cache(redis, project_id)
        
        # Devolver respuesta sin contenido
        return Response(status_code=status.HTTP_204_NO_CONTENT)
    
    # manejo de errores
    except grpc.RpcError as e:
        print(f"Error: {e}")
        _handle_grpc_error(e, f"Error al eliminar proyecto con id {project_id}")
    except HTTPException as he:
        print(f"HTTPException: {he.detail}")
        raise he
    except Exception as e:
        print(f"Error: {type(e).__name__}: {e}")
        traceback.print_exc()
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Error interno del servidor: {str(e)}"
        )

# endpoint para bulkCreateProjects
@router.post("/bulk", response_model=dict, status_code=status.HTTP_201_CREATED)
def bulkCreateProjects(
    # Lista de proyectos a crear
    projects: List[ProjectCreate],
    redis: Redis = Depends(get_redis_connection),
    _auth: bool = auth_write # Dependencia de autenticación
):    
    try:
        
        # Validar cada proyecto antes del bulk
        for i, project in enumerate(projects):
            if not project.title or project.title.strip() == "":
                raise HTTPException(
                    status_code=status.HTTP_400_BAD_REQUEST,
                    detail=f"El proyecto {i+1} debe tener título"
                )
            
            if project.priority and not (1 <= project.priority <= 5):
                raise HTTPException(
                    status_code=status.HTTP_400_BAD_REQUEST,
                    detail=f"La prioridad del proyecto {i+1} debe ser entre 1 y 5"
                )

        # Verificar conflictos de título
        for project in projects:
            ensure_title_not_conflicting(project.title)

        # Llamar al servicio GRPC
        result = grpc_client.bulkCreateProjectsGrpc(projects)
        
        # Invalidar caché por los nuevos proyectos
        invalidate_project_cache(redis)
        
        # Devolver respuesta
        return {
            "projects_created": result.projects_created,
            "projects": [
                ProjectResponse(
                    id=p.id,
                    title=p.title,
                    summary=p.summary,
                    priority=p.priority,
                    status=p.status or "PENDING"
                )
                for p in result.projects
            ]
        }
    
    # manejo de errores
    except HTTPException as he:
        print(f"HTTPException: {he.detail}")
        raise he
    except Exception as e:
        print(f"Error {type(e).__name__}: {e}")
        traceback.print_exc()
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Error interno del servidor: {str(e)}"
        )