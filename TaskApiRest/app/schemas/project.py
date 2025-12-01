from pydantic import BaseModel, ConfigDict, Field, validator
from typing import List, Optional

# Modelo base de Pydantic para Project
class ProjectBase(BaseModel):
    # restricciones de los campos
    title: str = Field(..., min_length=1, max_length=255)
    summary: Optional[str] = Field(None, max_length=1000)
    priority: int = Field(1, ge=1, le=5)
    status: Optional[str] = Field("PENDING")

# validaciones
    @validator('title')
    def title_not_empty(cls, v):
        if not v or v.strip() == "":
            raise ValueError('title es obligatorio')
        return v.strip()

    @validator('priority')
    def priority_range(cls, v):
        if not (1 <= v <= 5):
            raise ValueError('priority debe ser entre 1 y 5')
        return v
#valor por defecto para status
    @validator('status')
    def status_default(cls, v):
        if not v:
            return "PENDING"
        return v

# Modelo para crear
class ProjectCreate(ProjectBase):
    pass

# Modelo para update 
class ProjectUpdate(ProjectBase):
    pass

# Modelo para patch
class ProjectPatch(BaseModel):
    title: Optional[str] = Field(None, min_length=1, max_length=255)
    summary: Optional[str] = Field(None, max_length=1000)
    priority: Optional[int] = Field(None, ge=1, le=5)
    status: Optional[str] = Field(None)
#validaciones para patch
    @validator('title')
    def title_not_empty(cls, v):
        if v is not None and v.strip() == "":
            raise ValueError('title no puede estar vacío')
        return v.strip() if v else v

    @validator('priority')
    def priority_range(cls, v):
        if v is not None and not (1 <= v <= 5):
            raise ValueError('priority debe ser entre 1 y 5')
        return v

# Modelo de respuesta
class ProjectResponse(ProjectBase):
    model_config = ConfigDict(from_attributes=True)
    id: str
    # status puede venir vacío desde GRPC
    status: str = "PENDING"

# Modelo para bulk create
class BulkCreateRequest(BaseModel):
    projects: List[ProjectCreate] = Field(..., min_items=1)
# Modelo de respuesta para bulk create
class BulkCreateResponse(BaseModel):
    projects_created: int
    projects: List[ProjectResponse]

# Modelo para get con filtros 
class ProjectFilters(BaseModel):
    id: Optional[str] = None
    title: Optional[str] = None
    priority: Optional[int] = Field(None, ge=1, le=5)
    status: Optional[str] = None