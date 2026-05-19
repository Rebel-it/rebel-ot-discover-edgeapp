#!/bin/bash

# Removes all rebel-ot-discover containers and images from the SecureEdge device.
# Run this from your laptop, the same way you run push_ixon.sh.
set -e

BACKEND_CONTAINER="rebel-ot-discover-edgeapp"
FRONTEND_CONTAINER="rebel-ot-discover-edgeapp-react"
COOKIE_JAR="$(mktemp)"

# ── Prompt for configuration ───────────────────────────────────────────────────
read -rp "SecureEdge IP address (e.g. 172.27.21.1): " SECURE_EDGE_IP
echo

if [[ -z "$SECURE_EDGE_IP" ]]; then
    echo "Error: SecureEdge IP address is required." >&2
    exit 1
fi

read -rp "Username [admin]: " USERNAME
USERNAME=${USERNAME:-admin}

read -rsp "Password: " PASSWORD
echo
echo

if [[ -z "$PASSWORD" ]]; then
    echo "Error: password is required." >&2
    exit 1
fi

BASE_URL="http://${SECURE_EDGE_IP}:80"

# ── Authenticate ───────────────────────────────────────────────────────────────
echo "Authenticating..."
curl --silent --fail-with-body \
     --request POST \
     --url "${BASE_URL}/auth/login" \
     --cookie-jar "${COOKIE_JAR}" \
     --data "username=${USERNAME}" \
     --data "password=${PASSWORD}"
echo

# Helper: stop a container (ignores 404 if already stopped/missing)
stop_container() {
    local name=$1
    echo "Stopping container: ${name}"
    curl --silent --fail-with-body \
         --request POST \
         --url "${BASE_URL}/api/v1/docker/containers/${name}/stop" \
         --cookie "${COOKIE_JAR}" || true
    echo
}

# Helper: delete a container (ignores 404 if already missing)
delete_container() {
    local name=$1
    echo "Deleting container: ${name}"
    curl --silent --fail-with-body \
         --request DELETE \
         --url "${BASE_URL}/api/v1/docker/containers/${name}" \
         --cookie "${COOKIE_JAR}" || true
    echo
}

# Helper: delete an image (ignores 404 if already missing)
delete_image() {
    local name=$1
    echo "Deleting image: ${name}"
    curl --silent --fail-with-body \
         --request DELETE \
         --url "${BASE_URL}/api/v1/docker/images/${name}" \
         --cookie "${COOKIE_JAR}" || true
    echo
}

echo
echo "=== Stopping containers ==="
stop_container "${FRONTEND_CONTAINER}"
stop_container "${BACKEND_CONTAINER}"

echo
echo "=== Deleting containers ==="
delete_container "${FRONTEND_CONTAINER}"
delete_container "${BACKEND_CONTAINER}"

echo
echo "=== Deleting images ==="
delete_image "${FRONTEND_CONTAINER}"
delete_image "${BACKEND_CONTAINER}"

# ── Clean up temp cookie file ──────────────────────────────────────────────────
rm -f "${COOKIE_JAR}"

echo
echo "=== Cleanup complete ==="

