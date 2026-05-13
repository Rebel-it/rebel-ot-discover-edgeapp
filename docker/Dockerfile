# syntax=docker/dockerfile:1

# ── Build stage ────────────────────────────────────────────────────────────────
# Run the build stage on the host's native platform (avoids QEMU segfaults).
# dotnet publish cross-compiles to linux-arm64 without emulation.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution + project files first for layer-cached restore
COPY Rebelit.OT.Discover.EdgeApp.sln .
COPY Directory.Build.props .
COPY Directory.Packages.props .
COPY Rebelit.OT.Discover.EdgeApp/Rebelit.OT.Discover.EdgeApp.csproj                                    Rebelit.OT.Discover.EdgeApp/
COPY Rebelit.OT.Discover.EdgeApp.Connections.IXON/Rebelit.OT.Discover.EdgeApp.Connections.IXON.csproj  Rebelit.OT.Discover.EdgeApp.Connections.IXON/
COPY Rebelit.OT.Discover.EdgeApp.Connections.OPCUA/Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.csproj Rebelit.OT.Discover.EdgeApp.Connections.OPCUA/

RUN dotnet restore Rebelit.OT.Discover.EdgeApp/Rebelit.OT.Discover.EdgeApp.csproj \
    -r linux-arm64

# Copy the rest of the source
COPY Rebelit.OT.Discover.EdgeApp/                    Rebelit.OT.Discover.EdgeApp/
COPY Rebelit.OT.Discover.EdgeApp.Connections.IXON/   Rebelit.OT.Discover.EdgeApp.Connections.IXON/
COPY Rebelit.OT.Discover.EdgeApp.Connections.OPCUA/  Rebelit.OT.Discover.EdgeApp.Connections.OPCUA/

RUN dotnet publish Rebelit.OT.Discover.EdgeApp/Rebelit.OT.Discover.EdgeApp.csproj \
    -c Release \
    -r linux-arm64 \
    --no-self-contained \
    -o /app/publish

# ── Runtime stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends iproute2 && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Write the entrypoint inline so the build context can remain ../src
RUN printf '#!/bin/sh\nset -e\n\nif [ -z "$SECUREEDGEPRO_BaseAddress" ]; then\n    HOST_IP=$(ip route | awk '"'"'/default/ { print $3 }'"'"')\n    echo "Detected host IP: ${HOST_IP}"\n    export SECUREEDGEPRO_BaseAddress="http://${HOST_IP}:80"\nfi\n\necho "SecureEdge Pro base address: ${SECUREEDGEPRO_BaseAddress}"\n\nexec dotnet Rebelit.OT.Discover.EdgeApp.dll\n' > /entrypoint.sh \
    && chmod +x /entrypoint.sh

# ── OPC UA connection ──────────────────────────────────────────────────────────
ARG OPCUA_ServerAddress
ARG OPCUA_Username
ARG OPCUA_Password

ENV OPCUA_ServerAddress="$OPCUA_ServerAddress"
ENV OPCUA_Username="$OPCUA_Username"
ENV OPCUA_Password="$OPCUA_Password"

# ── Logging ────────────────────────────────────────────────────────────────────
ARG  LOG_LEVEL
ENV  LOG_LEVEL="$LOG_LEVEL"

ENTRYPOINT ["/entrypoint.sh"]
