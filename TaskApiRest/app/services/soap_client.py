from zeep import Client
from zeep.exceptions import Fault
from app.core.config import settings
from app.schemas.task import TaskCreate, TaskUpdate, TaskPatch
from fastapi import HTTPException, status
from typing import Optional

# Excepciones
class soapError(HTTPException):
    def __init__(self, detail: str):
        # usa 502 para errores de gateway 
        super().__init__(
            status_code=status.HTTP_502_BAD_GATEWAY,
            detail=f"soap service error {detail}"
        )


class soapFailedDependency(HTTPException):
    def __init__(self, detail: str):
        # usa 424 para errores de dependencia SOAP
        super().__init__(
            status_code=status.HTTP_424_FAILED_DEPENDENCY,
            detail=f"soap failed dependency {detail}"
        )

class soapNotFound(HTTPException):
    # usa 404 para recursos no encontrados
    def __init__(self, detail: str = "Task not found in soap "):
        super().__init__(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=detail
        )


def _fault_text(f: Fault) -> str:
    # Devuelve un texto de fallo de las excepciones 
    for attr in ("message", "faultstring", "detail"):
        val = getattr(f, attr, None)
        if val:
            try:
                return str(val)
            except Exception:
                pass
    # string presentación del error
    return str(f)


def _fault_code(f: Fault) -> str:
    #retorna el codigo de fallo
    return getattr(f, "faultcode", "") or ""


def _map_and_raise(fault_text: str, fault_code: str):
    ft = (fault_text or "").lower()
    fc = (fault_code or "").lower()

    # contiene palabras que sugieren una falla para 424.
    if "faileddependency" in fc or "failed dependency" in ft or "depend" in ft or "dependencia" in ft:
        raise soapFailedDependency(fault_text)

    #gateway error
    raise soapError(fault_text)

try:
    # Conectar con soap usando la URL del WSDL de la configuración
    client = Client(settings.SOAP_WSDL_URL)
except Exception as e:
    # Si el WSDL no carga
    raise RuntimeError(f"No se pudo conectar{e}")

#getAllTasksSoap
def getAllTasksSoap(page: int, pageSize: int, filter: Optional[str], sortBy: Optional[str], sortOrder: str):
    try:
        # respuesta del soap
        response = client.service.getAllTasks(
            page=page,
            pageSize=pageSize,
            filter=filter,
            sortBy=sortBy,
            sortOrder=sortOrder
        )
        return response
    #Excepcion del servicio soap
    except Fault as f:
        fault_text = _fault_text(f)
        fault_code = _fault_code(f)
        _map_and_raise(fault_text, fault_code)

#getTaskById
def getTaskByIdSoap(task_id: int):
    try:
        #obtener respuesta según el id
        response = client.service.getTaskById(task_id=task_id)
        return response
        #excepciones que vienen tambien de soap
    except Fault as f:
        fault_text = _fault_text(f)
        fault_code = _fault_code(f)
        # si no se encuentra la tarea
        if "ResourceNotFound" in fault_code or "Tarea no encontrada" in fault_text:
            raise soapNotFound(detail=fault_text)
        _map_and_raise(fault_text, fault_code)

#createTask
def createTaskSoap(task: TaskCreate):
    try:
        #crear tarea 
        response = client.service.createTask(
            title=task.title,
            description=task.description,
            endDate=task.endDate
        )
        return response
    except Fault as f:
        #excepciones que vienen tambien de soap
        fault_text = _fault_text(f)
        # si da el titulo vacio o la fecha es en el pasado
        if ("El título es obligatorio." in fault_text or
                "La fecha de finalización no puede ser en el pasado." in fault_text):
            raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail=fault_text)
        fault_code = _fault_code(f)
        _map_and_raise(fault_text, fault_code)

#updateTask
def updateTaskSoap(task_id: int, task: TaskUpdate):
    try:
        # Reemplazo los datos de la tarea
        response = client.service.updateTask(
            task_id=task_id,
            title=task.title,
            description=task.description,
            isCompleted=task.isCompleted,
            endDate=task.endDate
        )
        return response
    #excepciones que vienen tambien de soap
    except Fault as f:
        fault_text = _fault_text(f)
        fault_code = _fault_code(f)
        # si no se encuentra la tarea
        if "ResourceNotFound" in fault_code or "Tarea no encontrada" in fault_text:
            raise soapNotFound(detail=fault_text)
        # si da el titulo vacio o la fecha es en el pasado
        if ("El título es obligatorio." in fault_text or
                "La fecha de finalización no puede ser en el pasado." in fault_text):
            raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail=fault_text)
        _map_and_raise(fault_text, fault_code)

#patchTask
def patchTaskSoap(task_id: int, task: TaskPatch):
    try:
        # cambia los datos que se quieren modificar y no enviará campos nulos a los que no se les haga cambio
        response = client.service.patchTask(
            task_id=task_id,
            title=task.title,
            description=task.description,
            isCompleted=task.isCompleted,
            endDate=task.endDate
        )
        return response
        #excepciones que vienen tambien de soap
    except Fault as f:
        fault_text = _fault_text(f)
        fault_code = _fault_code(f)
        # si no se encuentra la tarea
        if "ResourceNotFound" in fault_code or "Tarea no encontrada" in fault_text:
            raise soapNotFound(detail=fault_text)
        # si la fecha es en el pasado
        if ("La fecha de finalización no puede ser en el pasado." in fault_text):
            raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail=fault_text)
        _map_and_raise(fault_text, fault_code)

#deleteTask
def deleteTaskSoap(task_id: int):
    #elimina la tarea según el id
    try:
        response_message = client.service.deleteTask(task_id=task_id)
        return response_message
    #excepciones que vienen tambien de soap
    except Fault as f:
        fault_text = _fault_text(f)
        fault_code = _fault_code(f)
        # si no se encuentra la tarea
        if "ResourceNotFound" in fault_code or "Tarea no encontrada" in fault_text:
            raise soapNotFound(detail=fault_text)
        _map_and_raise(fault_text, fault_code)

#getTaskByTitle
def getTaskByTitleSoap(title: str):
    try:
        # Llama al método getTaskByTitle de soap
        response = client.service.getTaskByTitle(title=title)
        return response
    except Fault as f:
        # si el soap falla
        fault_text = _fault_text(f)
        fault_code = _fault_code(f)
        _map_and_raise(fault_text, fault_code)