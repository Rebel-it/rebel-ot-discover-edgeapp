#!/bin/bash

# Output executed commands and stop on errors.
set -e

# Prompt for SecureEdge IP and IXON credentials
read -rp "SecureEdge IP address (e.g. 172.27.21.1): " SECURE_EDGE_IP
echo
read -rp "IXON Application ID:   " IXON_ApplicationId
read -rp "IXON Company ID:       " IXON_CompanyId
read -rp "IXON Bearer Token:    " IXON_BearerToken
read -rp "IXON Agent ID:         " IXON_AgentId
read -rp "IXON Data Source ID (leave blank to auto-create): " IXON_DataSourceId
echo
read -rp "OPC UA Server Address (e.g. opc.tcp://172.27.21.3:4840): " OPCUA_ServerAddress
read -rp "OPC UA Username:       " OPCUA_Username
read -rp "OPC UA Password:      " OPCUA_Password
echo

if [[ -z "$SECURE_EDGE_IP" || -z "$IXON_ApplicationId" || -z "$IXON_CompanyId" || -z "$IXON_BearerToken" \
   || -z "$IXON_AgentId" \
   || -z "$OPCUA_ServerAddress" || -z "$OPCUA_Username" || -z "$OPCUA_Password" ]]; then
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
    --build-arg IXON_ApplicationId="$IXON_ApplicationId" \
    --build-arg IXON_CompanyId="$IXON_CompanyId" \
    --build-arg IXON_BearerToken="$IXON_BearerToken" \
    --build-arg IXON_AgentId="$IXON_AgentId" \
    --build-arg IXON_DataSourceId="$IXON_DataSourceId" \
    --build-arg OPCUA_ServerAddress="$OPCUA_ServerAddress" \
    --build-arg OPCUA_Username="$OPCUA_Username" \
    --build-arg OPCUA_Password="$OPCUA_Password" \
    -f ./Dockerfile \
    ../src