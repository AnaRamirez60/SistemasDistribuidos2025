# Crear el cliente de hydra 
CREATE_RESP=$(curl -s -w "\n%{http_code} %{redirect_url}" -L -X POST http://localhost:4445/clients \
    -H "Content-Type: application/json" \
    -d '{
        "client_id": "machine-client",
        "client_secret": "machinesecret",
        "grant_types": ["client_credentials"],
        "response_types": ["token"],
        "scope": "read write",
        "token_endpoint_auth_method": "client_secret_basic"
    }')

CREATE_HTTP=$(echo "$CREATE_RESP" | tail -n1 | awk '{print $1}')
CREATE_REDIRECT=$(echo "$CREATE_RESP" | tail -n1 | awk '{print $2}')
CREATE_BODY=$(echo "$CREATE_RESP" | sed '$d')

echo "Creación cliente - HTTP status: $CREATE_HTTP"
if [ -n "$CREATE_REDIRECT" ] && [ "$CREATE_REDIRECT" != "-" ]; then
  echo "Redirect detectado a: $CREATE_REDIRECT"
fi


# get al admin del api
CLIENT_CHECK_RAW=$(curl -s -w "\n%{http_code} %{redirect_url}" -L "http://localhost:4445/clients/machine-client")
CLIENT_CHECK_HTTP=$(echo "$CLIENT_CHECK_RAW" | tail -n1 | awk '{print $1}')
CLIENT_CHECK_REDIRECT=$(echo "$CLIENT_CHECK_RAW" | tail -n1 | awk '{print $2}')
CLIENT_CHECK_BODY=$(echo "$CLIENT_CHECK_RAW" | sed '$d')
echo "Comprobación cliente - HTTP status: $CLIENT_CHECK_HTTP"
if [ -n "$CLIENT_CHECK_REDIRECT" ] && [ "$CLIENT_CHECK_REDIRECT" != "-" ]; then
  echo "Redirect detectado a: $CLIENT_CHECK_REDIRECT"
fi

# Obtener Token de Acceso 
ACCESS_TOKEN=""
for PORT in 4444 4445; do
  RESP=$(curl -s -w "\n%{http_code} %{redirect_url}" -L -X POST "http://localhost:${PORT}/oauth2/token" \
      -u "machine-client:machinesecret" \
      -d "grant_type=client_credentials&scope=read write")

  HTTP_CODE=$(echo "$RESP" | tail -n1 | awk '{print $1}')
  REDIRECT_URL=$(echo "$RESP" | tail -n1 | awk '{print $2}')
  BODY=$(echo "$RESP" | sed '$d')  

  # intentar extraer access_token, pero si jq falla devolver cadena vacía
  ACCESS_TOKEN=$(echo "$BODY" | jq -r '.access_token' 2>/dev/null || echo "")

  echo "Puerto probado: $PORT - HTTP status: $HTTP_CODE"
  if [ -n "$REDIRECT_URL" ] && [ "$REDIRECT_URL" != "-" ]; then
    echo "Redirect detectado a: $REDIRECT_URL"
  fi
  if [ -n "$ACCESS_TOKEN" ] && [ "$ACCESS_TOKEN" != "null" ]; then
    echo "Token Obtenido (primeros 20 caracteres): ${ACCESS_TOKEN:0:20}..."
    break
  else
    echo "No se obtuvo access_token desde puerto $PORT."
    echo "Respuesta de Hydra (raw):"
    if echo "$BODY" | jq . >/dev/null 2>&1; then
      echo "$BODY" | jq .
    else
      echo "$BODY"
    fi

    # Si recibimos 401 intentar client_secret_post como diagnóstico
    if [ "$HTTP_CODE" -eq 401 ]; then
      echo "HTTP 401 recibido. Intentando método alternativo client_secret_post (client_id+client_secret en cuerpo)..."
      RESP2=$(curl -s -w "\n%{http_code} %{redirect_url}" -L -X POST "http://localhost:${PORT}/oauth2/token" \
        -d "grant_type=client_credentials&scope=read write&client_id=machine-client&client_secret=machinesecret")

      HTTP_CODE2=$(echo "$RESP2" | tail -n1 | awk '{print $1}')
      REDIRECT2=$(echo "$RESP2" | tail -n1 | awk '{print $2}')
      BODY2=$(echo "$RESP2" | sed '$d')
      ACCESS_TOKEN=$(echo "$BODY2" | jq -r '.access_token' 2>/dev/null || echo "")

      echo "Respuesta alternativa - HTTP status: $HTTP_CODE2"
      if [ -n "$REDIRECT2" ] && [ "$REDIRECT2" != "-" ]; then
        echo "Redirect detectado a: $REDIRECT2"
      fi
      if [ -n "$ACCESS_TOKEN" ] && [ "$ACCESS_TOKEN" != "null" ]; then
        echo "Token Obtenido via client_secret_post (primeros 20 caracteres): ${ACCESS_TOKEN:0:20}..."
        break
      else
        echo "No se obtuvo token con client_secret_post. Respuesta:"
        if echo "$BODY2" | jq . >/dev/null 2>&1; then
          echo "$BODY2" | jq .
        else
          echo "$BODY2"
        fi
      fi
    fi
  fi
done

if [ -z "$ACCESS_TOKEN" ]; then
  echo "Error: No se pudo obtener el Access Token desde ninguno de los puertos (4444, 4445)."
  echo "Comprueba que Hydra esté en ejecución, que el cliente exista y que las credenciales sean correctas."
 echo " - Confirma que el admin API (puerto 4445) permita crear/listar clientes y que la creación devolvió 201 o 409."
  echo " - Si usas otra configuración, ajusta los puertos/URLs en este script."
  exit 1
fi

# create task
echo " POST /tasks/"
curl -s -X POST "http://localhost:8000/tasks/" \
    -H "Authorization: Bearer $ACCESS_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"title":"T1","description":"Descripción","endDate":"2025-12-31"}' | jq '.'
echo

# Realizar Petición al api rest get all tasks
echo " GET /tasks/?page=1&pageSize=5 "
curl -s -H "Authorization: Bearer $ACCESS_TOKEN" \
    "http://localhost:8000/tasks/?page=1&pageSize=5" | jq '.'
echo

# get By Title
echo " GET /tasks/getByTitle?title=T "
curl -s -H "Authorization: Bearer $ACCESS_TOKEN" \
    "http://localhost:8000/tasks/getByTitle?title=T" | jq '.'
echo

# get By ID
echo " GET /tasks/1"
curl -s -H "Authorization: Bearer $ACCESS_TOKEN" \
    "http://localhost:8000/tasks/1" | jq '.'
echo

# put task
echo " PUT /tasks/1"
curl -s -X PUT "http://localhost:8000/tasks/1" \
    -H "Authorization: Bearer $ACCESS_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"title":"Título Actualizado","description":"Descripción nueva","isCompleted":true,"endDate":"2026-01-20"}' | jq '.'
echo

# patch task
echo " PATCH /tasks/1"
curl -s -X PATCH "http://localhost:8000/tasks/1" \
    -H "Authorization: Bearer $ACCESS_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"isCompleted":false}' | jq '.'
echo

# delete task
echo " DELETE /tasks/7"
curl -s -X DELETE "http://localhost:8000/tasks/1" \
    -H "Authorization: Bearer $ACCESS_TOKEN" | jq '.'
echo

# get By ID
echo " GET /tasks/1"
curl -s -H "Authorization: Bearer $ACCESS_TOKEN" \
    "http://localhost:8000/tasks/1" | jq '.'
echo
