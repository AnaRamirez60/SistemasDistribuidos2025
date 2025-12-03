from fastapi import FastAPI
from contextlib import asynccontextmanager
from redis import Redis
from app.dependencies.cache import get_redis_connection
from app.routers import tasks
from app.routers import projects 

# Variable global para la conexión con Redis
redis_conn = None

@asynccontextmanager
async def lifespan(app: FastAPI):
    # Inicializa la conexión a Redis
    global redis_conn
    redis_conn = get_redis_connection()
    # Verifica la conexión a Redis
    try:
        redis_conn.ping()
    except Exception as e:
        print(f"Error al conectar con Redis {e}")
    
    yield # La aplicación se ejecuta aquí
    
    # Shutdown de Redis
    if redis_conn:
        redis_conn.close()

# Creación de la aplicación FastAPI
app = FastAPI(
    title="API REST",
    version="1.0.0",
    lifespan=lifespan 
)

# router de las tareas
app.include_router(tasks.router)
app.include_router(projects.router)

