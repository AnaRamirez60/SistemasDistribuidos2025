from redis import Redis
import json
from typing import Any, Optional
import os
from functools import lru_cache

# Configuración de Redis
REDIS_HOST = os.getenv("REDIS_HOST", "redis")
REDIS_PORT = int(os.getenv("REDIS_PORT", 6379))
REDIS_DB = int(os.getenv("REDIS_DB", 0))

# Pool de conexiones Redis (singleton)
_redis_connection = None

# Obtener conexión a Redis con caching
@lru_cache()
def get_redis_connection() -> Redis:
    # setup de la conexión a Redis
    global _redis_connection
    if _redis_connection is None:
        try:
            # Crear la conexión
            _redis_connection = Redis(
                host=REDIS_HOST,
                port=REDIS_PORT,
                db=REDIS_DB,
                decode_responses=True,
                socket_connect_timeout=5,
                retry_on_timeout=True
            )
            # Test the connection
            _redis_connection.ping()
        except Exception as e:
            print(f"Error: {e}")
            raise
    return _redis_connection

# Funciones de caché genéricas
def set_cache(key: str, value: dict, redis_conn: Redis, ttl: int = 3600):
    try:
        redis_conn.setex(key, ttl, json.dumps(value, default=str))
    except Exception as e:
        print(f"Error setting cache for key {key}: {e}")

def get_cache(key: str, redis_conn: Redis) -> Optional[dict]:
    # Obtener datos de caché
    try:
        cached_data = redis_conn.get(key)
        if cached_data:
            return json.loads(cached_data)
        return None
    except Exception as e:
        print(f"Error getting cache for key {key}: {e}")
        return None

# Funciones para invalidar caché 
def invalidate_cache(redis_conn: Redis, pattern: Optional[str] = None):
    try:
        if pattern:
            # Invalidar por patrón específico
            keys = redis_conn.keys(pattern)
            if keys:
                redis_conn.delete(*keys)
        else:
            # Invalidar todas las claves tasks y projects
            task_keys = redis_conn.keys("task:*")
            project_keys = redis_conn.keys("project:*")
            task_list_keys = redis_conn.keys("tasks_list:*")
            project_list_keys = redis_conn.keys("projects_list:*")
            
            # Combinar todas las keys
            all_keys = task_keys + project_keys + task_list_keys + project_list_keys
            
            # si hay keys, eliminarlas
            if all_keys:
                redis_conn.delete(*all_keys)
    except Exception as e:
        print(f"Error: {e}")

# Funciones específicas para invalidar caché de tasks y projects
def invalidate_task_cache(redis_conn: Redis, task_id: Optional[int] = None):
    try:
        if task_id:
            # Invalidar caché específica de una task
            task_key = f"task:{task_id}"
            redis_conn.delete(task_key)
        
        # Siempre invalidar las listas de tasks
        list_keys = redis_conn.keys("tasks_list:*")
        if list_keys:
            redis_conn.delete(*list_keys)
            
    except Exception as e:
        print(f"Error: {e}")

# Nueva función para invalidar caché de projects para bulk create 
def invalidate_project_cache(redis_conn: Redis, project_id: Optional[str] = None):
    try:
        if project_id:
            # Invalidar caché específica de un project
            project_key = f"project:{project_id}"
            redis_conn.delete(project_key)
        
        # Siempre invalidar las listas de projects
        list_keys = redis_conn.keys("projects_list:*")
        if list_keys:
            redis_conn.delete(*list_keys)
            
    except Exception as e:
        print(f"Error: {e}")