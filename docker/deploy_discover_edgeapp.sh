#!/bin/bash

# Deploys pre-built images from the release zip to a SecureEdge Pro device.
# Run this from the directory where you extracted the release zip.
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
BACKEND_CONTAINER="rebel-ot-discover-edgeapp"
FRONTEND_CONTAINER="rebel-ot-discover-edgeapp-react"
NETWORK="machine-builder"

# ── Check dependencies ────────────────────────────────────────────────────────
if ! command -v crane &>/dev/null; then
    echo "Error: 'crane' is required but not installed." >&2
    echo "  macOS:  brew install crane" >&2
    echo "  Linux:  https://github.com/google/go-containerregistry/releases" >&2
    exit 1
fi

# ── Prompt for configuration ──────────────────────────────────────────────────
read -rp "SecureEdge IP address (e.g. 172.27.21.1): " SECURE_EDGE_IP

if [[ -z "$SECURE_EDGE_IP" ]]; then
    echo "Error: SecureEdge IP address is required." >&2
    exit 1
fi

# ── Authenticate with SecureEdge Pro ─────────────────────────────────────────
# shellcheck source=auth_secure_edge_pro.sh
source "${SCRIPT_DIR}/auth_secure_edge_pro.sh"

set -x

# ── Push images to SecureEdge Pro registry ────────────────────────────────────
echo "Pushing backend image..."
crane push --insecure \
    "${SCRIPT_DIR}/rebel-ot-discover-edgeapp.tar" \
    "${SECURE_EDGE_IP}:5000/${BACKEND_CONTAINER}:latest"

echo "Pushing frontend image..."
crane push --insecure \
    "${SCRIPT_DIR}/rebel-ot-discover-edgeapp-react.tar" \
    "${SECURE_EDGE_IP}:5000/${FRONTEND_CONTAINER}:latest"

set +x

# ── Create and start containers ───────────────────────────────────────────────
# shellcheck source=create_and_start_containers.sh
source "${SCRIPT_DIR}/create_and_start_containers.sh"

rm -f "${COOKIE_JAR}"

echo
echo "=== Deployment complete ==="
echo "Please visit the application on your Secure Edge Pro: http://${SECURE_EDGE_IP}:3000"
