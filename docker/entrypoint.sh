#!/bin/sh
set -e

# Auto-detect the host IP via the default gateway (the router hosting this container).
# Can be overridden by passing SECUREEDGEPRO_BaseAddress explicitly.
if [ -z "$SECUREEDGEPRO_BaseAddress" ]; then
    HOST_IP=$(ip route | awk '/default/ { print $3 }')
    echo "Detected host IP: ${HOST_IP}"
    export SECUREEDGEPRO_BaseAddress="http://${HOST_IP}:80"
fi

echo "SecureEdge Pro base address: ${SECUREEDGEPRO_BaseAddress}"

exec dotnet Rebelit.OT.Discover.EdgeApp.API.dll
