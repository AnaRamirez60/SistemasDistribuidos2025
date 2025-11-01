import logging
from spyne import Application, rpc, ServiceBase, Unicode, Integer, Boolean, Date, ComplexModel, Array
from spyne.protocol.soap import Soap11
from spyne.server.wsgi import WsgiApplication
from sqlalchemy.orm import Session
from .database import SessionLocal
from . import models
from .validators import validate_title, validate_end_date, validate_task_id, validate_task_exists

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

# Define la aplicacion de Soap, equivalente a SoapCore en .NET
application = Application([TaskService],
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