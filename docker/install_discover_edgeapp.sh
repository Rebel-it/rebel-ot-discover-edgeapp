#!/bin/bash

# Output executed commands and stop on errors.
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
BACKEND_CONTAINER="rebel-ot-discover-edgeapp"
FRONTEND_CONTAINER="rebel-ot-discover-edgeapp-react"
NETWORK="machine-builder"
LOG_LEVEL="Information"

# ── Prompt for configuration ───────────────────────────────────────────────────
read -rp "SecureEdge IP address (e.g. 172.27.21.1): " SECURE_EDGE_IP

if [[ -z "$SECURE_EDGE_IP" ]]; then
    echo "Error: SecureEdge IP address is required." >&2
    exit 1
fi


# ── Authenticate with SecureEdge Pro ──────────────────────────────────────────
# shellcheck source=auth_secure_edge_pro.sh
source "${SCRIPT_DIR}/auth_secure_edge_pro.sh"

# ── Configure buildkit for the IXON registry ──────────────────────────────────
cat > "${SCRIPT_DIR}/buildkitd-secure-edge-pro.toml" <<EOF
[registry."${SECURE_EDGE_IP}:5000"]
http = true
EOF

set -x

# ── Set up a single shared buildx builder ─────────────────────────────────────
docker buildx rm secure-edge-pro || true
docker buildx create --name secure-edge-pro \
                     --config "${SCRIPT_DIR}/buildkitd-secure-edge-pro.toml"
docker buildx use secure-edge-pro

# ── Build and push the backend image ──────────────────────────────────────────
docker buildx build \
    --platform linux/arm64/v8 \
    --tag "${SECURE_EDGE_IP}:5000/${BACKEND_CONTAINER}:latest" \
    --no-cache \
    --push \
    --build-arg LOG_LEVEL="$LOG_LEVEL" \
    -f "${SCRIPT_DIR}/Dockerfile" \
    "${SCRIPT_DIR}/.."

# ── Build and push the frontend image ─────────────────────────────────────────
docker buildx build \
    --platform linux/arm64/v8 \
    --tag "${SECURE_EDGE_IP}:5000/${FRONTEND_CONTAINER}:latest" \
    --no-cache \
    --push \
    -f "${SCRIPT_DIR}/../src/Rebelit.OT.Discover.EdgeApp.WebApp/docker/DockerFile" \
    "${SCRIPT_DIR}/../src/Rebelit.OT.Discover.EdgeApp.WebApp"

set +x

# ── Create and start containers ───────────────────────────────────────────────
# shellcheck source=create_and_start_containers.sh
source "${SCRIPT_DIR}/create_and_start_containers.sh"

rm -f "${COOKIE_JAR}"

echo
echo "=== Installation complete ==="
echo "Please visit the application on your Secure Edge Pro: http://${SECURE_EDGE_IP}:3000"
