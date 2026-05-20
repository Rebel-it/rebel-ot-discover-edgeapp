#!/bin/bash

# Creates and starts the backend and frontend containers on a SecureEdge Pro device.
# Expects the following variables to already be set by the caller:
#   BASE_URL            - e.g. http://172.27.21.1:80
#   COOKIE_JAR          - path to the authenticated cookie jar
#   BACKEND_CONTAINER   - container/image name for the backend
#   FRONTEND_CONTAINER  - container/image name for the frontend
#   NETWORK             - Docker network name
#   SECURE_EDGE_IP      - used only for the final success message

# ── Create containers ─────────────────────────────────────────────────────────
echo "Creating backend container..."
curl --silent --fail-with-body \
     --request POST \
     --url "${BASE_URL}/api/v1/docker/containers" \
     --cookie "${COOKIE_JAR}" \
     --header 'Content-Type: application/json' \
     --data "{
         \"container\": { \"name\": \"${BACKEND_CONTAINER}\" },
         \"image\": { \"name\": \"${BACKEND_CONTAINER}\", \"tag\": \"latest\" },
         \"ports\": [],
         \"mounts\": [],
         \"networks\": [{ \"name\": \"${NETWORK}\", \"driver\": \"bridge\" }],
         \"environment_variables\": []
     }"
echo

echo "Creating frontend container..."
curl --silent --fail-with-body \
     --request POST \
     --url "${BASE_URL}/api/v1/docker/containers" \
     --cookie "${COOKIE_JAR}" \
     --header 'Content-Type: application/json' \
     --data "{
         \"container\": { \"name\": \"${FRONTEND_CONTAINER}\" },
         \"image\": { \"name\": \"${FRONTEND_CONTAINER}\", \"tag\": \"latest\" },
         \"ports\": [{ \"source\": 80, \"destination\": 3000, \"protocol\": \"tcp\" }],
         \"mounts\": [],
         \"networks\": [{ \"name\": \"${NETWORK}\", \"driver\": \"bridge\" }],
         \"environment_variables\": []
     }"
echo

# ── Start containers ──────────────────────────────────────────────────────────
echo "Starting backend container..."
curl --silent --fail-with-body \
     --request POST \
     --url "${BASE_URL}/api/v1/docker/containers/${BACKEND_CONTAINER}/start" \
     --cookie "${COOKIE_JAR}"
echo

echo "Starting frontend container..."
curl --silent --fail-with-body \
     --request POST \
     --url "${BASE_URL}/api/v1/docker/containers/${FRONTEND_CONTAINER}/start" \
     --cookie "${COOKIE_JAR}"
echo
