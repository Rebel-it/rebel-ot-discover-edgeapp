#!/bin/bash

# Non-interactive variant of remove_discover_edgeapp.sh.
# Removes all rebel-ot-discover containers and images from the SecureEdge device.
#
# Usage: ./remove_discover_edgeapp_args.sh <SECURE_EDGE_IP> <USERNAME> <PASSWORD>

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
BACKEND_CONTAINER="rebel-ot-discover-edgeapp"
FRONTEND_CONTAINER="rebel-ot-discover-edgeapp-react"

# ── Parse arguments ───────────────────────────────────────────────────────────
SECURE_EDGE_IP="${1:-}"
USERNAME="${2:-}"
PASSWORD="${3:-}"

if [[ -z "$SECURE_EDGE_IP" ]]; then
    echo "Error: SECURE_EDGE_IP is required as the first argument." >&2
    echo "Usage: $0 <SECURE_EDGE_IP> <USERNAME> <PASSWORD>" >&2
    exit 1
fi

if [[ -z "$USERNAME" ]]; then
    echo "Error: USERNAME is required as the second argument." >&2
    echo "Usage: $0 <SECURE_EDGE_IP> <USERNAME> <PASSWORD>" >&2
    exit 1
fi

if [[ -z "$PASSWORD" ]]; then
    echo "Error: PASSWORD is required as the third argument." >&2
    echo "Usage: $0 <SECURE_EDGE_IP> <USERNAME> <PASSWORD>" >&2
    exit 1
fi

# ── Authenticate with SecureEdge Pro ──────────────────────────────────────────
# shellcheck source=auth_secure_edge_pro_args.sh
source "${SCRIPT_DIR}/auth_secure_edge_pro_args.sh"

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
