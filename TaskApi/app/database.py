import os
from sqlalchemy import create_engine
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import sessionmaker

# url de conexión a la base de datos
DATABASE_URL = os.getenv("DATABASE_URL", "mysql+mysqlconnector://root:1234@TaskDb:3306/task")

engine = create_engine(DATABASE_URL)

# Creación de la sesión para interactuar con la BD
SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)

# Base para alembic
Base = declarative_base()

# Función para obtener una sesión de base de datos
def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()