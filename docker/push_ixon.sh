#!/bin/bash

# Output executed commands and stop on errors.
set -e

# Prompt for SecureEdge IP and IXON credentials
read -rp "SecureEdge IP address (e.g. 172.27.21.1): " SECURE_EDGE_IP
echo
echo
echo
read -rp "Log level (Verbose/Debug/Information/Warning/Error/Fatal) [Information]: " LOG_LEVEL
LOG_LEVEL=${LOG_LEVEL:-Information}
echo

if [[ -z "$SECURE_EDGE_IP" ]]; then
    echo "Error: all values are required." >&2
    exit 1
fi

# Write the buildkitd config with the provided IP
cat > "$(dirname "$0")/buildkitd-secure-edge-pro.toml" <<EOF
[registry."${SECURE_EDGE_IP}:5000"]
http = true
EOF

set -x

# Uncomment the following line should the edge gateway have been
# given a different IP address.
# docker buildx rm secure-edge-pro;

# Remove the existing instance if necessary
docker buildx rm secure-edge-pro || true

# Create and initialize the build environment.
docker buildx create --name secure-edge-pro \
                     --config buildkitd-secure-edge-pro.toml
docker buildx use secure-edge-pro

# Build and push the image with credentials baked in via build args
docker buildx build \
    --platform linux/arm64/v8 \
    --tag "${SECURE_EDGE_IP}:5000/rebel-ot-discover-edgeapp:latest" \
    --no-cache \
    --push \
    --build-arg OPCUA_ServerAddress="$OPCUA_ServerAddress" \
    --build-arg LOG_LEVEL="$LOG_LEVEL" \
    -f ./Dockerfile \
    ../src