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

    if [ "$HTTP_CODE" -eq 401 ]; then
      echo "HTTP 401 recibido. Intentando método alternativo client_secret_post..."
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
  exit 1
fi

# Get All Projects
echo " GET /projects/"
curl -s -H "Authorization: Bearer $ACCESS_TOKEN" \
    "http://localhost:8000/projects/" | jq '.'
echo

# Create Project
echo " POST /projects/"
CREATE_RESPONSE=$(curl -s -X POST "http://localhost:8000/projects/" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $ACCESS_TOKEN" \
    -d '{"title":"Test12","summary":"desc","priority":3}' | jq '.')
echo "$CREATE_RESPONSE"
echo

# Extraer ID del proyecto creado
PROJECT_ID=$(echo "$CREATE_RESPONSE" | jq -r '._id // empty')
if [ -z "$PROJECT_ID" ] || [ "$PROJECT_ID" = "null" ]; then
    PROJECT_ID="692ca9cbb001b4b9ced4b95b"
fi

# Get Project By Id
echo " GET /projects/$PROJECT_ID"
curl -s -H "Authorization: Bearer $ACCESS_TOKEN" \
    "http://localhost:8000/projects/$PROJECT_ID" | jq '.'
echo

# Get Projects por Filtro
echo " GET /projects/?title=y"
curl -s -H "Authorization: Bearer $ACCESS_TOKEN" \
    "http://localhost:8000/projects/?title=y" | jq '.'
echo

# Put Project
echo " PUT /projects/$PROJECT_ID"
curl -s -X PUT "http://localhost:8000/projects/$PROJECT_ID" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $ACCESS_TOKEN" \
    -d '{"title": "Nuevo Título Actualizado", "summary": "Resumen revisado para el proyecto X.", "priority": 2, "status": "IN_PROGRESS"}' | jq '.'
echo

# Create Group of Projects
echo " POST /projects/bulk"
curl -s -X POST "http://localhost:8000/projects/bulk" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $ACCESS_TOKEN" \
    -d '[
        {
            "title": "Proyecto Masivo 12",
            "summary": "Primer proyecto del lote.",
            "priority": 5
        },
        {
            "title": "Proyecto Masivo 13",
            "summary": "Segundo proyecto del lote.",
            "priority": 3,
            "status": "PENDING"
        }
    ]' | jq '.'
echo

# Get All Projects después de operaciones
echo " GET /projects/"
curl -s -H "Authorization: Bearer $ACCESS_TOKEN" \
    "http://localhost:8000/projects/" | jq '.'
echo

# Delete Project
echo " DELETE /projects/$PROJECT_ID"
curl -s -X DELETE "http://localhost:8000/projects/$PROJECT_ID" \
    -H "Authorization: Bearer $ACCESS_TOKEN" | jq '.'
echo