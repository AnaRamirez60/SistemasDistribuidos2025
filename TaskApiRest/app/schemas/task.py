import datetime
from pydantic import BaseModel, ConfigDict, Field
from typing import List, Optional

# Modelo base de Pydantic 
class TaskBase(BaseModel):
    # Campos base
    title: str = Field(...)
    description: Optional[str] = None
    endDate: datetime.date

# Modelo para la crear
class TaskCreate(TaskBase):
    pass

# Modelo para put
class TaskUpdate(TaskBase):
    isCompleted: bool

# Modelo para patch
class TaskPatch(BaseModel):
    title: Optional[str] = Field(None, min_length=1)
    description: Optional[str] = None
    isCompleted: Optional[bool] = None
    endDate: Optional[datetime.date] = None

# Modelo de respuesta 
class TaskResponse(TaskBase):
    model_config = ConfigDict(from_attributes=True) 
    id: int
    isCompleted: bool

# Modelo de respuesta para paginación
class PaginatedTaskResponse(BaseModel):
    tasks: List[TaskResponse]
    page: int
    pageSize: int
    totalTasks: int
    totalPages: int