from typing import Optional
import redis
import json
from functools import lru_cache
from app.core.config import settings

@lru_cache()
def get_redis_connection():
    # Crear y devolver la conexión a Redis
    return redis.Redis(host=settings.REDIS_HOST, port=6379, db=0, decode_responses=True)

def set_cache(key: str, value: dict, redis_conn: redis.Redis, ttl: int = 3600):
    # convierte datetime a 'YYYY-MM-DD'
    redis_conn.setex(key, ttl, json.dumps(value, default=str))

# Obtener caché
def get_cache(key: str, redis_conn: redis.Redis) -> Optional[dict]:
    # obtener y devolver un dict desde Redis
    cached_data = redis_conn.get(key)
    if cached_data:
        # parsea el json y devuelve un dict
        return json.loads(cached_data)
    return None

def invalidate_cache(redis_conn: redis.Redis, pattern: str):
    # Elimina claves que coincidan con un patrón.
    keys = redis_conn.keys(pattern)
    if keys:
        redis_conn.delete(*keys)

def invalidate_task_cache(redis_conn: redis.Redis, task_id: Optional[int] = None):
    # Invalida el caché para una tarea específica 
    if task_id:
        redis_conn.delete(f"{task_id}")

    # quita cachés de listas para tareas que puedan verse afectadas
    invalidate_cache(redis_conn)