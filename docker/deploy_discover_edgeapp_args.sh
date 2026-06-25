#!/bin/bash

# Deploys pre-built images from the release zip to a SecureEdge Pro device.
# Run this from the directory where you extracted the release zip.
#
# Usage: ./deploy_discover_edgeapp_args.sh <SECURE_EDGE_IP> <USERNAME> <PASSWORD>

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
BACKEND_CONTAINER="rebel-ot-discover-edgeapp"
FRONTEND_CONTAINER="rebel-ot-discover-edgeapp-react"
NETWORK="machine-builder"

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

# ── Check dependencies ────────────────────────────────────────────────────────
if ! command -v docker &>/dev/null; then
    echo "Error: 'docker' is required but not installed." >&2
    echo "  https://docs.docker.com/get-docker/" >&2
    exit 1
fi

# ── Check insecure registry configuration ────────────────────────────────────
if ! docker info 2>/dev/null | grep -q "${SECURE_EDGE_IP}:5000"; then
    echo "Error: '${SECURE_EDGE_IP}:5000' is not configured as an insecure registry in Docker." >&2
    echo "  Add it via Docker Desktop → Settings → Docker Engine:" >&2
    echo "    { \"insecure-registries\": [\"${SECURE_EDGE_IP}:5000\"] }" >&2
    echo "  Then restart Docker and re-run this script." >&2
    exit 1
fi

# ── Authenticate with SecureEdge Pro ─────────────────────────────────────────
# shellcheck source=auth_secure_edge_pro_args.sh
source "${SCRIPT_DIR}/auth_secure_edge_pro_args.sh"

set -x

# ── Load and push images to SecureEdge Pro registry ──────────────────────────
echo "Loading backend image..."
BACKEND_IMAGE=$(docker load -i "${SCRIPT_DIR}/rebel-ot-discover-edgeapp.tar" | awk '/Loaded image/ {print $NF}')
docker tag "${BACKEND_IMAGE}" "${SECURE_EDGE_IP}:5000/${BACKEND_CONTAINER}:latest"
echo "Pushing backend image..."
docker push "${SECURE_EDGE_IP}:5000/${BACKEND_CONTAINER}:latest"
docker rmi "${BACKEND_IMAGE}" "${SECURE_EDGE_IP}:5000/${BACKEND_CONTAINER}:latest" 2>/dev/null || true

echo "Loading frontend image..."
FRONTEND_IMAGE=$(docker load -i "${SCRIPT_DIR}/rebel-ot-discover-edgeapp-react.tar" | awk '/Loaded image/ {print $NF}')
docker tag "${FRONTEND_IMAGE}" "${SECURE_EDGE_IP}:5000/${FRONTEND_CONTAINER}:latest"
echo "Pushing frontend image..."
docker push "${SECURE_EDGE_IP}:5000/${FRONTEND_CONTAINER}:latest"
docker rmi "${FRONTEND_IMAGE}" "${SECURE_EDGE_IP}:5000/${FRONTEND_CONTAINER}:latest" 2>/dev/null || true

set +x

# ── Create and start containers ───────────────────────────────────────────────
# shellcheck source=create_and_start_containers.sh
source "${SCRIPT_DIR}/create_and_start_containers.sh"

rm -f "${COOKIE_JAR}"

echo
echo "=== Deployment complete ==="
URL="http://${SECURE_EDGE_IP}:3000"
if command -v xdg-open &>/dev/null; then
    xdg-open "$URL"
elif command -v open &>/dev/null; then
    open "$URL"
else
    echo "Please visit the application on your Secure Edge Pro: $URL"
fi
