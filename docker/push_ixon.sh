#!/bin/bash

# Output executed commands and stop on errors.
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

# ── Prompt for configuration ───────────────────────────────────────────────────
read -rp "SecureEdge IP address (e.g. 172.27.21.1): " SECURE_EDGE_IP
echo

if [[ -z "$SECURE_EDGE_IP" ]]; then
    echo "Error: SecureEdge IP address is required." >&2
    exit 1
fi

read -rp "Log level (Verbose/Debug/Information/Warning/Error/Fatal) [Information]: " LOG_LEVEL
LOG_LEVEL=${LOG_LEVEL:-Information}
echo

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
    --tag "${SECURE_EDGE_IP}:5000/rebel-ot-discover-edgeapp:latest" \
    --no-cache \
    --push \
    --build-arg OPCUA_ServerAddress="$OPCUA_ServerAddress" \
    --build-arg LOG_LEVEL="$LOG_LEVEL" \
    -f "${SCRIPT_DIR}/Dockerfile" \
    "${SCRIPT_DIR}/../src"

# ── Build and push the frontend image ─────────────────────────────────────────
docker buildx build \
    --platform linux/arm64/v8 \
    --tag "${SECURE_EDGE_IP}:5000/rebel-ot-discover-edgeapp-react:latest" \
    --no-cache \
    --push \
    -f "${SCRIPT_DIR}/../src/Rebelit.OT.Discover.EdgeApp.WebApp/docker/DockerFile" \
    "${SCRIPT_DIR}/../src/Rebelit.OT.Discover.EdgeApp.WebApp"
