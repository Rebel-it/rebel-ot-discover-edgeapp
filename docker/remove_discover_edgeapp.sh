#!/bin/bash

# Removes all rebel-ot-discover containers and images from the SecureEdge device.
# Run this from your laptop, the same way you run install_discover_edgeapp.sh.
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
BACKEND_CONTAINER="rebel-ot-discover-edgeapp"
FRONTEND_CONTAINER="rebel-ot-discover-edgeapp-react"

# ── Prompt for configuration ───────────────────────────────────────────────────
read -rp "SecureEdge IP address (e.g. 172.27.21.1): " SECURE_EDGE_IP
echo

if [[ -z "$SECURE_EDGE_IP" ]]; then
    echo "Error: SecureEdge IP address is required." >&2
    exit 1
fi

# shellcheck source=auth_secure_edge_pro.sh
source "${SCRIPT_DIR}/auth_secure_edge_pro.sh"

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
echo "Recommended: Remove the IXON service account you created for this tool in the IXON Cloud to avoid leaving unused accounts."