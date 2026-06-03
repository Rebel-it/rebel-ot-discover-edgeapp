#!/bin/sh
set -e

# Auto-detect the host IP via the default gateway (the router hosting this container).
# Can be overridden by passing SECUREEDGEPRO_BASEADDRESS explicitly.
if [ -z "$SECUREEDGEPRO_BASEADDRESS" ]; then
    HOST_IP=$(ip route | awk '/default/ { print $3 }')
    echo "Detected host IP: ${HOST_IP}"
    export SECUREEDGEPRO_BASEADDRESS="http://${HOST_IP}:80"
fi

echo "SecureEdge Pro base address: ${SECUREEDGEPRO_BASEADDRESS}"

exec dotnet Rebelit.OT.Discover.EdgeApp.API.dll
