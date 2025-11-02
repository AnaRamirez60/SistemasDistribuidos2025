from fastapi import APIRouter, Depends, HTTPException, status, Query, Response, Request
from redis import Redis
from typing import Optional, List
from app.schemas.task import TaskResponse, PaginatedTaskResponse, TaskCreate, TaskUpdate, TaskPatch
from app.services import soap_client
from app.dependencies.cache import get_redis_connection, get_cache, set_cache, invalidate_task_cache
from app.dependencies.authentication import get_auth_dependency
from zeep.helpers import serialize_object

# Configuración del router
router = APIRouter(
    prefix="/tasks",
    tags=["Tasks"]
)

# Dependencias de autenticación
auth_read = Depends(get_auth_dependency("read"))
auth_write = Depends(get_auth_dependency("write"))


def _normalize_existing_list(existing_res):
    #Normaliza la respuesta del SOAP a una lista de dicts
    existing_dict = serialize_object(existing_res)
    if existing_dict is None:
        return []
    if isinstance(existing_dict, list):
        return existing_dict
    return [existing_dict]


def ensure_title_not_conflicting(title: str, current_id: Optional[int] = None):
    # Normalizar entrada
    if not title:
        return

# Consulta el SOAP por title y lanza HTTPException 409 si existe otra tarea con ese título.
    existing = soap_client.getTaskByTitleSoap(title)
    existing_list = _normalize_existing_list(existing)
    # Normalizar título
    title_norm = title.strip().lower()
    for et in existing_list:
        try:
            # Obtener título e id de la tarea 
            et_title = et.get("title") if isinstance(et, dict) else getattr(et, "title", None)
            et_id = et.get("id") if isinstance(et, dict) else getattr(et, "id", None)
        # manejo de excepciones al obtener atributos
        except Exception:
            et_title = None
            et_id = None

        if et_title and et_title.strip().lower() == title_norm:
            # si current_id es none cualquier match es conflicto
            if current_id is None:
                raise HTTPException(status_code=status.HTTP_409_CONFLICT,
                                    detail=f"Una tarea con el título '{title}' ya existe.")
            # si existe otra tarea con distinto id es un conflicto
            try:
                if et_id is None or int(et_id) != int(current_id):
                    raise HTTPException(status_code=status.HTTP_409_CONFLICT,
                                        detail=f"Una tarea con el título '{title}' ya existe.")
            except ValueError:
                # si no podemos parsear id se considera conflicto por seguridad
                raise HTTPException(status_code=status.HTTP_409_CONFLICT,
                                    detail=f"Una tarea con el título '{title}' ya existe.")

# Endpoint para get all con paginación
@router.get(
    "/", 
    response_model=PaginatedTaskResponse)
def getAllTask(
    page: int = Query(1, ge=1),
    pageSize: int = Query(10, ge=1, le=100),
    filter: Optional[str] = Query(None),
    sortBy: Optional[str] = Query(None),
    sortOrder: str = Query("asc", pattern="^(asc|desc)$"),
    redis: Redis = Depends(get_redis_connection),
    request: Request = None,
    _auth: bool = auth_read 
):
    # Crear clave de caché para esta consulta
    cache_key = f"tasks_list:p{page}:ps{pageSize}:f{filter}:s{sortBy}:so{sortOrder}"
    
    # Intentar obtener de caché
    cached_data = get_cache(cache_key, redis)
    if cached_data:
        # datos de Pydantic desde el caché
        return PaginatedTaskResponse.model_validate(cached_data)

    # llamar al soap
    response = soap_client.getAllTasksSoap(page, pageSize, filter, sortBy, sortOrder)
    
    # Guardar en caché
    response_dict = serialize_object(response)

    # hacer que tasks sea siempre una lista
    def _ensure_list(obj):
        # si ya es una lista, devolverla
        if isinstance(obj, list):
            return obj
        # Si es un dict y contiene una lista interna bajo una clave común, extraerla
        if isinstance(obj, dict):
            # Buscar la lista de tareas
            for k in ("TaskModel", "task", "taskModel", "Tasks"):
                if k in obj:
                    inner = obj[k]
                    return _ensure_list(inner)
            # Si no encuentra clave específica, tratar como item único
            return [obj]
        # Para cualquier otro tipo, lista vacía
        return []

    # Normalizar la lista de tareas, buscando claves comunes y asegurando que es una lista
    if "tasks" in response_dict:
        tasks_value = response_dict.get("tasks")
    elif "TaskModel" in response_dict:
        tasks_value = response_dict.get("TaskModel")
    else:
        tasks_value = response_dict
        
    normalized_tasks = _ensure_list(tasks_value)

    # paginación
    paginated = {
        "tasks": normalized_tasks,
        "page": response_dict.get("page", response_dict.get("Page", 1)),
        "pageSize": response_dict.get("pageSize", response_dict.get("PageSize", response_dict.get("page_size", 10))),
        "totalTasks": response_dict.get("totalTasks", response_dict.get("TotalTasks", response_dict.get("total", len(normalized_tasks)))),
        "totalPages": response_dict.get("totalPages", response_dict.get("TotalPages", 1)),
    }

    # Guardar en caché la paginación
    response_pydantic = PaginatedTaskResponse.model_validate(paginated)
    set_cache(cache_key, response_pydantic.model_dump(), redis)
    # Devolver la respuesta
    return response_pydantic

@router.get(
    "/getByTitle",
    response_model=List[TaskResponse])
def getTaskByTitle(
    title: str = Query(..., min_length=1), # El título lo dan en un query param
    request: Request = None,
    _auth: bool = auth_read # necesita la autorización de read
):
    # Llamar al soap
    response_list = soap_client.getTaskByTitleSoap(title)
    
    # Convierte la lista de objetos Zeep a una lista de Pydantic
    response_list_dict = serialize_object(response_list)
    # normalizar a lista
    return [TaskResponse.model_validate(task) for task in response_list_dict]

@router.get(
    "/{task_id}", 
    response_model=TaskResponse)
def getTaskById(
    # El id de la tarea va en la ruta
    task_id: int,
    # conexión con Redis
    redis: Redis = Depends(get_redis_connection),
    request: Request = None,
    _auth: bool = auth_read # necesita la autorización de read
):
    # Crea su clave para caché
    cache_key = f"task:{task_id}"

    # Obtener de caché
    cached_data = get_cache(cache_key, redis)
    if cached_data:
        return TaskResponse.model_validate(cached_data)

    # Llamar al soap
    response = soap_client.getTaskByIdSoap(task_id)

    # Guardar en caché
    response_dict = serialize_object(response)
    response_pydantic = TaskResponse.model_validate(response_dict)
    set_cache(cache_key, response_pydantic.model_dump(), redis)
    # Devolver la respuesta
    return response_pydantic

@router.post(
    "/", 
    response_model=TaskResponse, 
    status_code=status.HTTP_201_CREATED)
def createTask(
    # La petición para crear la tarea
    task: TaskCreate,
    request: Request,
    # Respuesta de la petición
    response: Response,
    # Conexión a Redis
    redis: Redis = Depends(get_redis_connection),
    _auth: bool = auth_write #necesita la autorización de write
):
    # si existe otra tarea con el mismo título
    ensure_title_not_conflicting(task.title)
    # Llamar al SOAP para crear la tarea
    new_task = soap_client.createTaskSoap(task)

    # borra caché de la listas
    invalidate_task_cache(redis) 

    # Convertimos a dict para Pydantic
    new_task_dict = serialize_object(new_task)
    url = request.url_for("getTaskById", task_id=new_task_dict['id'])
    # Añadir header Location
    response.headers["Location"] = str(url)
    # Devolver la nueva tarea creada
    return TaskResponse.model_validate(new_task_dict)

@router.put(
    # Actualiza una tarea según su id en la url
    "/{task_id}", 
    response_model=TaskResponse)
def update_task_put(
    # El id de la tarea va en la ruta
    task_id: int,
    # La petición para actualizar la tarea
    task: TaskUpdate,
    # Conexión a Redis
    redis: Redis = Depends(get_redis_connection),
    _auth: bool = auth_write # necesita la autorización de write
):
    # Llamar al soap
    # si existe otra tarea con el mismo título
    ensure_title_not_conflicting(task.title, current_id=task_id)

    updated_task = soap_client.updateTaskSoap(task_id, task)

    #Invalidar cache
    invalidate_task_cache(redis, task_id=task_id)

    # Convertimos a dict para Pydantic
    updated_task_dict = serialize_object(updated_task)
    try:
        request_obj = Request.__call__
    except Exception:
        request_obj = None
    if isinstance(updated_task_dict, dict) and updated_task_dict.get("id") is not None:
         pass
    return TaskResponse.model_validate(updated_task_dict)

@router.patch(
    # Patch según su id en la url
    "/{task_id}", 
    response_model=TaskResponse)
def updateTask(
    # El id de la tarea va en la ruta
    task_id: int,
    # La petición para actualizar la tarea
    task: TaskPatch,
    # Conexión a Redis
    redis: Redis = Depends(get_redis_connection),
    _auth: bool = auth_write # necesita la autorización de write
):
    # llamar al soap para control de errores
    task_data = task.model_dump(exclude_unset=True)

    # Si se quiere actualizar el título, comprobar conflictos
    if "title" in task_data and task_data.get("title"):
        title_val = task_data.get("title").strip()
        ensure_title_not_conflicting(title_val, current_id=task_id)

    # Creamos un TaskPatch object para que zeep maneje nones
    patch_obj = TaskPatch(**task_data)

    #llamar al soap
    updated_task = soap_client.patchTaskSoap(task_id, patch_obj)

    #Invalidar caché
    invalidate_task_cache(redis, task_id=task_id)

    # Convertimos a dict para Pydantic
    updated_task_dict = serialize_object(updated_task)
    return TaskResponse.model_validate(updated_task_dict)

@router.delete(
    # Elimina una tarea según su id en la url
    "/{task_id}", 
    # No devuelve contenido
    status_code=status.HTTP_204_NO_CONTENT)
def delete_task(
    # El id de la tarea va en la ruta
    task_id: int,
    # Conexión a Redis
    redis: Redis = Depends(get_redis_connection),
    _auth: bool = auth_write # necesita la autorización de write
):
    # Llamar al SOAP 
    soap_client.deleteTaskSoap(task_id)

    # Invalidar caché
    invalidate_task_cache(redis, task_id=task_id)
    # Devolver que la petición se completo, no envia contenido
    return Response(status_code=status.HTTP_204_NO_CONTENT)