import httpx
from fastapi import Depends, HTTPException, status
from fastapi.security import OAuth2PasswordBearer
from app.core.config import settings

#punto público
INTROSPECT_URL = f"{settings.HYDRA_ADMIN_URL}/admin/oauth2/introspect" 

# OAuth2 scheme para extraer el token
oauth2_scheme = OAuth2PasswordBearer(tokenUrl="token")

# Verifica el token con Hydra
async def check_scope(token: str, required_scope: str) -> bool:
  # Llama al endpoint de introspección de Hydra
    async with httpx.AsyncClient() as client:
        try:
            response = await client.post(
                INTROSPECT_URL,
                # Autenticarse como machine-client
                auth=(settings.HYDRA_CLIENT_ID, settings.HYDRA_CLIENT_SECRET),
                data={"token": token},
                headers={"Content-Type": "application/x-www-form-urlencoded"}
            )
            
            response.raise_for_status() 
            
            # Datos de introspección
            introspection_data = response.json()
            
            # Si el token no está activo, la validación falla
            if not introspection_data.get("active", False):
                return False
                
            # Verifica si el scope requerido está en la lista de scopes del token
            token_scopes = introspection_data.get("scope", "").split(" ")
            return required_scope in token_scopes

        except httpx.RequestError as e:
            # Error de red al contactar Hydra
            raise HTTPException(
                status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
                detail=f"Authentication service {e}"
            )
        except httpx.HTTPStatusError as e:
            # si client_id o secret son incorrectos
            raise HTTPException(
                status_code=status.HTTP_502_BAD_GATEWAY,
                detail=f"client_id o secret incorrecto {e.response.text}"
            )

def get_auth_dependency(required_scope: str):
    # Autenticación que verifica el scope requerido
    async def auth_check(token: str = Depends(oauth2_scheme)):
        is_valid = False 
        try:
            # Verifica el scope del token
            is_valid = await check_scope(token, required_scope)
        except HTTPException as e:
            raise e
        except Exception as e:
            # Captura cualquier otro error inesperado
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail=f"Authentication failed {e}"
            )
            
        if not is_valid:
        # Si el token no es válido o no tiene el scope requerido
            raise HTTPException(
                status_code=status.HTTP_403_FORBIDDEN,
                detail=f"No permissions '{required_scope}' is required."
            )
        return True # El token es válido y tiene el scope

# regresa la función de autenticación
    return auth_check