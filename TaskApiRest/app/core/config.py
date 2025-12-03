from pydantic_settings import BaseSettings

# Configuración de la aplicación
class Settings(BaseSettings):
    SOAP_WSDL_URL: str
    GRPC_SERVICE_URL: str
    REDIS_HOST: str
    HYDRA_ADMIN_URL: str
    HYDRA_PUBLIC_URL: str
    HYDRA_CLIENT_ID: str
    HYDRA_CLIENT_SECRET: str

# Lee variables de entorno desde un archivo .env
    class Config:
        env_file = ".env"

# Instancia de configuración
settings = Settings()