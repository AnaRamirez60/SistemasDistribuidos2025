import logging
from spyne import Application, rpc, ServiceBase, Unicode, Integer, Boolean, Date, ComplexModel, Array
from spyne.protocol.soap import Soap11
from spyne.server.wsgi import WsgiApplication
from werkzeug.wrappers import Request, Response
from werkzeug.routing import Map, Rule
from werkzeug.middleware.dispatcher import DispatcherMiddleware
import json
from sqlalchemy.orm import Session
from sqlalchemy import or_
from .database import SessionLocal
from . import models
from .validators import validate_title, validate_end_date, validate_task_id, validate_task_exists
import math

# Configuración del logger para modificar el nombre de la ruta
logging.basicConfig(level=logging.DEBUG)
logger = logging.getLogger(__name__)

# Define la estructura para las respuestas y el wsdl
class TaskModel(ComplexModel):
    id = Integer
    title = Unicode
    description = Unicode
    isCompleted = Boolean
    endDate = Date

# Define el modelo de respuesta paginada
class PaginatedTaskResponse(ComplexModel):
    tasks = Array(TaskModel)
    page = Integer
    pageSize = Integer
    totalTasks = Integer
    totalPages = Integer

# Servicio SOAP que contiene todos los métodos
class TaskService(ServiceBase):

#Create Task
    @rpc(Unicode(min_occurs=1), Unicode(min_occurs=0), Date(min_occurs=1), _returns=TaskModel)
    def createTask(ctx, title, description, endDate):
        # Validaciones que obtenemos de validators.py
        validate_title(title)
        validate_end_date(endDate)
        # Inicia la sesion con la base de datos
        db: Session = SessionLocal()
        #ocupamos try para asegurar que la sesion se cierre al acabar
        try:
            #crea una instancia y recibe los datos
            new_task = models.Task(
                title=title,
                description=description,
                endDate=endDate,
                #isCompleted por defecto es falso
                isCompleted=False
            )
            #carga los datos en una nueva tarea
            db.add(new_task)
            db.commit()
            db.refresh(new_task)
            # Regresa la estructura que anteriormente se definió con los datos de entrada del usuario
            return TaskModel(
                id=new_task.id,
                title=new_task.title,
                description=new_task.description,
                isCompleted=new_task.isCompleted,
                endDate=new_task.endDate
            )
        #guarda y cierra la sesión con la base de datos
        finally:
            db.close()

#get task by id
    @rpc(Integer(min_occurs=1), _returns=TaskModel)
    def getTaskById(ctx, task_id):
        # Validación que obtenemos de validators.py
        validate_task_id(task_id)
        # Inicia la sesion con la base de datos
        db: Session = SessionLocal()
        try:
             # Busca la tarea por id
            task = db.query(models.Task).filter(models.Task.id == task_id).first()
            # Validación que obtenemos de validators.py y si existe la tarea la regresa
            validate_task_exists(task)
            # Regresa la estructura que anteriormente se definió con los datos de la tarea con el id solicitado
            return TaskModel(
                id=task.id,
                title=task.title,
                description=task.description,
                isCompleted=task.isCompleted,
                endDate=task.endDate
            )
        #cierra la sesión con la base de datos
        finally:
            db.close()

#get task by title
    @rpc(Unicode(min_occurs=1), _returns=Array(TaskModel))
    def getTaskByTitle(ctx, title):
        # Inicia la sesion con la base de datos
        db: Session = SessionLocal()
        try:
            # Busca la tarea por título
            tasks = db.query(models.Task).filter(models.Task.title.ilike(f"%{title}%")).all()
            #regresa una todas de tareas que coinciden con el título solicitado
            return [TaskModel(
                id=t.id,
                title=t.title,
                description=t.description,
                isCompleted=t.isCompleted,
                endDate=t.endDate
            ) for t in tasks]
        #cierra la sesión con la base de datos
        finally:
            db.close()

#get all tasks
    @rpc(
        Integer(min_occurs=0, default=1),    # page
        Integer(min_occurs=0, default=10),   # pageSize
        Unicode(min_occurs=0, nillable=True), # filter 
        Unicode(min_occurs=0, nillable=True), # sortBy 
        Unicode(min_occurs=0, default='asc'), # sortOrder 
        _returns=PaginatedTaskResponse
    )
    def getAllTasks(ctx, page=1, pageSize=10, filter=None, sortBy=None, sortOrder='asc'):
        # Inicia la sesion con la base de datos
        db: Session = SessionLocal()
        try:
            # Asegurar valores válidos para paginación
            page = page if page and page > 0 else 1
            pageSize = pageSize if pageSize and pageSize > 0 else 10

            # Consulta base
            query = db.query(models.Task)

            # Aplicar filtro 
            if filter:
                search_term = f"%{filter}%"
                # Busca en título o descripción 
                query = query.filter(
                    or_(
                        models.Task.title.ilike(search_term),
                        models.Task.description.ilike(search_term)
                    )
                )

            # Obtener el conteo total después de filtrar
            total_tasks = query.count()

            # Aplicar ordenamiento 
            if sortBy and hasattr(models.Task, sortBy):
                column = getattr(models.Task, sortBy)
                if sortOrder.lower() == 'desc':
                    query = query.order_by(column.desc())
                else:
                    query = query.order_by(column.asc())
            else:
                # Orden por defecto 
                query = query.order_by(models.Task.id.asc())

            # Aplicar paginación 
            offset = (page - 1) * pageSize
            query = query.offset(offset).limit(pageSize)

            # Consulta final
            tasks = query.all()

            # Calcular total de páginas
            total_pages = math.ceil(total_tasks / pageSize) if pageSize > 0 else 0

            # Formatear la respuesta, regresa la tarea y los datos de paginación
            task_models = [TaskModel(
                id=t.id,
                title=t.title,
                description=t.description,
                isCompleted=t.isCompleted,
                endDate=t.endDate
            ) for t in tasks]

            return PaginatedTaskResponse(
                tasks=task_models,
                page=page,
                pageSize=pageSize,
                totalTasks=total_tasks,
                totalPages=total_pages
            )
        #cierra la sesión con la base de datos
        finally:
            db.close()

#update task
    @rpc(Integer(min_occurs=1), Unicode(min_occurs=1), Unicode(min_occurs=0), Boolean(min_occurs=1), Date(min_occurs=1), _returns=TaskModel)
    def updateTask(ctx, task_id, title, description, isCompleted, endDate):
        # Validaciones que obtenemos de validators.py
        validate_task_id(task_id)
        validate_title(title)
        validate_end_date(endDate)
        # Inicia la sesion con la base de datos
        db: Session = SessionLocal()
        try:
             # Busca la tarea por id
            task = db.query(models.Task).filter(models.Task.id == task_id).first()
            # Validación que obtenemos de validators.py
            validate_task_exists(task)
            # Actualiza todos los campos de la tarea
            task.title = title
            task.description = description
            task.isCompleted = isCompleted
            task.endDate = endDate
            # Guarda los cambios en la base de datos
            db.commit()
            db.refresh(task)
            # Regresa la estructura con los datos actualizados de la tarea
            return TaskModel(
                id=task.id,
                title=task.title,
                description=task.description,
                isCompleted=task.isCompleted,
                endDate=task.endDate
            )
        #cierra la sesión con la base de datos
        finally:
            db.close()

#patch task
    @rpc(Integer(min_occurs=1), Unicode(min_occurs=0), Unicode(min_occurs=0), Boolean(min_occurs=0), Date(min_occurs=0), _returns=TaskModel)
    def patchTask(ctx, task_id, title=None, description=None, isCompleted=None, endDate=None):
        # Validación que obtenemos de validators.py
        validate_task_id(task_id)
        # Inicia la sesion con la base de datos
        db: Session = SessionLocal()
        try:
            # Busca la tarea por id
            task = db.query(models.Task).filter(models.Task.id == task_id).first()
            # Validación que obtenemos de validators.py
            validate_task_exists(task)
            # Actualiza solo los campos que se proporcionaron
            if title is not None: 
                validate_title(title)
                task.title = title
            if description:
                task.description = description
            if isCompleted is not None:
                task.isCompleted = isCompleted
            if endDate:
                validate_end_date(endDate)
                task.endDate = endDate
            # Guarda los cambios en la base de datos    
            db.commit()
            db.refresh(task)
            # Regresa la estructura con los datos actualizados de la tarea
            return TaskModel(
                id=task.id,
                title=task.title,
                description=task.description,
                isCompleted=task.isCompleted,
                endDate=task.endDate
            )
        #cierra la sesión con la base de datos
        finally:
            db.close()

#delete task
    @rpc(Integer(min_occurs=1), _returns=Unicode)
    def deleteTask(ctx, task_id):
        # Validación que obtenemos de validators.py
        validate_task_id(task_id)
        # Inicia la sesion con la base de datos
        db: Session = SessionLocal()
        try:
            # Busca la tarea por id
            task = db.query(models.Task).filter(models.Task.id == task_id).first()
            # Validación que obtenemos de validators.py
            validate_task_exists(task)
            # Elimina la tarea de la base de datos
            db.delete(task)
            db.commit()
            return f"Tarea {task_id} eliminada correctamente"
        #cierra la sesión con la base de datos
        finally:
            db.close()

# Define la aplicacion de Soap, equivalente a SoapCore en .NET
application = Application(
    [TaskService],
    tns='task.management.soap',
    in_protocol=Soap11(validator='lxml'),
    out_protocol=Soap11()
)

# Personaliza la aplicación WSGI para modificar la ruta
class CustomWsgiApplication(WsgiApplication):
    def __call__(self, environ, start_response):
        # Manejar la ruta /task
        path_info = environ.get('PATH_INFO', '')
        if path_info.startswith('/task'):
            environ['PATH_INFO'] = path_info[len('/task'):] or '/'
            logger.debug(f"Ruta convertida: {path_info} -> {environ['PATH_INFO']}")
        return super().__call__(environ, start_response)

# Produce un objeto para que Gunicorn pueda ejecutar la aplicación
wsgi_application = WsgiApplication(application)
