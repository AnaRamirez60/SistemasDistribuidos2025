set -euo pipefail

# Hosts candidatos para hydra 
CANDIDATE_HOSTS=(hydra-auth hydra host.docker.internal localhost 127.0.0.1)
HYDRA_ADMIN=""
HYDRA_PUBLIC=""

echo "${CANDIDATE_HOSTS[*]}"
for h in "${CANDIDATE_HOSTS[@]}"; do
  admin_url="http://${h}:4445"
  public_url="http://${h}:4444"

  # comprobar admin ready
  if curl -s --connect-timeout 2 -o /dev/null -w '%{http_code}' "${admin_url}/health/ready" | grep -q "^200$"; then
    HYDRA_ADMIN=${admin_url}
    HYDRA_PUBLIC=${public_url}
    echo "Encontrado hydra admin en ${HYDRA_ADMIN}"
    break
  fi
  # comprobar public ready
  if curl -s --connect-timeout 2 -o /dev/null -w '%{http_code}' "${public_url}/health/ready" | grep -q "^200$"; then
    HYDRA_ADMIN=${admin_url}
    HYDRA_PUBLIC=${public_url}
    echo "Encontrado hydra public en ${HYDRA_PUBLIC}"
    break
  fi
done

# fallback a localhost si no se encuentra nada
if [ -z "${HYDRA_ADMIN}" ]; then
  HYDRA_ADMIN="http://localhost:4445"
  HYDRA_PUBLIC="http://localhost:4444"
fi

DEFAULT_CLIENT_ID="machine-client"
DEFAULT_CLIENT_SECRET="machinesecret"
DEFAULT_SCOPE="read write"

CLIENT_ID=${CLIENT_ID:-$DEFAULT_CLIENT_ID}
CLIENT_SECRET=${CLIENT_SECRET:-$DEFAULT_CLIENT_SECRET}
SCOPE=${SCOPE:-$DEFAULT_SCOPE}

# Si existe client.json, usarlo
if [ -f /client.json ]; then
  CLIENT_PAYLOAD="$(cat /client.json)"
else
  CLIENT_PAYLOAD=$(cat <<EOF
{
  "client_id": "${CLIENT_ID}",
  "client_secret": "${CLIENT_SECRET}",
  "grant_types": ["client_credentials"],
  "response_types": ["token"],
  "scope": "read write",
  "token_endpoint_auth_method": "client_secret_basic"
}
EOF
)
fi

echo "Hydra Admin: ${HYDRA_ADMIN}"
TMP_CREATE="/tmp/hydra_create_resp.$$"
HTTP_CODE=$(curl -sS -w "%{http_code}" -o "${TMP_CREATE}" -X POST -L \
  -H "Content-Type: application/json" \
  --data-binary "${CLIENT_PAYLOAD}" \
  "${HYDRA_ADMIN}/clients" || true)

if [ "${HTTP_CODE}" = "201" ] || [ "${HTTP_CODE}" = "200" ]; then
  echo "Cliente creado correctamente ${HTTP_CODE})."
elif [ "${HTTP_CODE}" = "409" ]; then
  echo "Cliente ya existe (409)."
else
  echo "El cleinte puede repetirse ${HTTP_CODE}"
  sed -n '1,200p' "${TMP_CREATE}" || true
fi

echo "access_token ${HYDRA_PUBLIC}"
ACCESS_TOKEN=""
for attempt in 1 2 3 4 5; do
  RESP=$(curl -sS --connect-timeout 5 -w "\n%{http_code}" -u "${CLIENT_ID}:${CLIENT_SECRET}" \
    -X POST "${HYDRA_PUBLIC}/oauth2/token" \
    -d "grant_type=client_credentials&scope=${SCOPE}" || true)
  BODY=$(echo "$RESP" | sed '$d')
  CODE=$(echo "$RESP" | tail -n1)

  if [ -n "$BODY" ] && echo "$BODY" | jq -e . >/dev/null 2>&1; then
    ACCESS_TOKEN=$(echo "$BODY" | jq -r '.access_token // empty' 2>/dev/null || echo "")
  fi

  echo " ${attempt}: HTTP ${CODE}"
  if [ -n "${ACCESS_TOKEN}" ]; then
    echo "Access token obtenido."
    break
  fi

  # Si HTTP es 401 o no hay token, intentar client_secret_post como fallback una vez
  if [ "${attempt}" -eq 1 ]; then
    RESP2=$(curl -sS --connect-timeout 5 -w "\n%{http_code}" -X POST "${HYDRA_PUBLIC}/oauth2/token" \
      -d "grant_type=client_credentials&scope=${SCOPE}&client_id=${CLIENT_ID}&client_secret=${CLIENT_SECRET}" || true)
    BODY2=$(echo "$RESP2" | sed '$d')
    CODE2=$(echo "$RESP2" | tail -n1)
    ACCESS_TOKEN=$(echo "$BODY2" | jq -r '.access_token // empty' 2>/dev/null || echo "")
    echo "Fallback ${CODE2}"
    if [ -n "${ACCESS_TOKEN}" ]; then
      echo "Access token obtenido"
      break
    fi
  fi

  sleep 2
done

if [ -z "${ACCESS_TOKEN}" ]; then
  echo "No se pudo obtener el access_token ${HYDRA_PUBLIC}." >&2
  echo "Comprueba que Hydra esté ejecutándose y que el cliente exista. URLs probadas: ${CANDIDATE_HOSTS[*]}"
  exit 1
fi

# Detectar host del API REST (intento usar hostname del servicio en red de Compose)
API_CANDIDATES=(taskapi-rest localhost 127.0.0.1 host.docker.internal)
API_HOST=""
for h in "${API_CANDIDATES[@]}"; do
  if curl -s --connect-timeout 2 -o /dev/null -w '%{http_code}' "http://${h}:8000/docs" | grep -q "^200$"; then
    API_HOST="${h}"
    break
  fi
done
API_HOST=${API_HOST:-localhost}
echo "Usando api rest en http://${API_HOST}:8000"

# Realizar llamadas con token
echo " POST /tasks/"
curl -s -X POST "http://${API_HOST}:8000/tasks/" \
    -H "Authorization: Bearer ${ACCESS_TOKEN}" \
    -H "Content-Type: application/json" \
    -d '{"title":"T1","description":"Descripción","endDate":"2025-12-31"}' | jq '.'
echo

echo " GET /tasks/?page=1&pageSize=5 "
curl -s -H "Authorization: Bearer ${ACCESS_TOKEN}" \
    "http://${API_HOST}:8000/tasks/?page=1&pageSize=5" | jq '.'
echo

echo " GET /tasks/getByTitle?title=T "
curl -s -H "Authorization: Bearer ${ACCESS_TOKEN}" \
    "http://${API_HOST}:8000/tasks/getByTitle?title=T" | jq '.'
echo

echo " GET /tasks/1"
curl -s -H "Authorization: Bearer ${ACCESS_TOKEN}" \
    "http://${API_HOST}:8000/tasks/1" | jq '.'
echo

echo " PUT /tasks/1"
curl -s -X PUT "http://${API_HOST}:8000/tasks/1" \
    -H "Authorization: Bearer ${ACCESS_TOKEN}" \
    -H "Content-Type: application/json" \
    -d '{"title":"Título Actualizado","description":"Descripción nueva","isCompleted":true,"endDate":"2026-01-20"}' | jq '.'
echo

echo " PATCH /tasks/1"
curl -s -X PATCH "http://${API_HOST}:8000/tasks/1" \
    -H "Authorization: Bearer ${ACCESS_TOKEN}" \
    -H "Content-Type: application/json" \
    -d '{"isCompleted":false}' | jq '.'
echo

echo " DELETE /tasks/1"
curl -s -X DELETE "http://${API_HOST}:8000/tasks/1" \
    -H "Authorization: Bearer ${ACCESS_TOKEN}" | jq '.'
echo

echo " GET /tasks/1"
curl -s -H "Authorization: Bearer ${ACCESS_TOKEN}" \
    "http://${API_HOST}:8000/tasks/1" | jq '.'
echo

