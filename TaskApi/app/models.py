from sqlalchemy import Column, Integer, String, Boolean, Date
from .database import Base

# Modelo de la tabla 'tasks' para la base de datos
class Task(Base):
    __tablename__ = "tasks"

    id = Column(Integer, primary_key=True, index=True, autoincrement=True)
    title = Column(String(100), nullable=False, index=True)
    description = Column(String(255), nullable=True)
    isCompleted = Column(Boolean, default=False, nullable=False)
    endDate = Column(Date, nullable=False)