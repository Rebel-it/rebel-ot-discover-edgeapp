#!/bin/bash
set -e

read -rp "SecureEdge IP address (e.g. 172.27.21.1): " SECURE_EDGE_IP
echo

if [[ -z "$SECURE_EDGE_IP" ]]; then
  echo "SecureEdge IP is required" >&2
  exit 1
fi

# Configure buildkit for the IXON registry
cat > buildkitd-secure-edge.toml <<EOF
[registry."${SECURE_EDGE_IP}:5000"]
http = true
EOF

docker buildx rm secure-edge || true

docker buildx create \
  --name secure-edge \
  --config buildkitd-secure-edge.toml

docker buildx use secure-edge

docker buildx build \
  --platform linux/arm64/v8 \
  --tag "${SECURE_EDGE_IP}:5000/my-react-ui:latest" \
  --push \
  -f Dockerfile \
  .
