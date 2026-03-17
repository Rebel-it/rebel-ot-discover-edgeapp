# syntax=docker/dockerfile:1

# ── Build stage ────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution + project files first for layer-cached restore
COPY Rebelit.OT.Discover.EdgeApp.sln .
COPY Directory.Build.props .
COPY Directory.Packages.props .
COPY Rebelit.OT.Discover.EdgeApp/Rebelit.OT.Discover.EdgeApp.csproj                                    Rebelit.OT.Discover.EdgeApp/
COPY Rebelit.OT.Discover.EdgeApp.Connections.IXON/Rebelit.OT.Discover.EdgeApp.Connections.IXON.csproj  Rebelit.OT.Discover.EdgeApp.Connections.IXON/
COPY Rebelit.OT.Discover.EdgeApp.Connections.OPCUA/Rebelit.OT.Discover.EdgeApp.Connections.OPCUA.csproj Rebelit.OT.Discover.EdgeApp.Connections.OPCUA/

RUN dotnet restore Rebelit.OT.Discover.EdgeApp/Rebelit.OT.Discover.EdgeApp.csproj

# Copy the rest of the source
COPY Rebelit.OT.Discover.EdgeApp/                    Rebelit.OT.Discover.EdgeApp/
COPY Rebelit.OT.Discover.EdgeApp.Connections.IXON/   Rebelit.OT.Discover.EdgeApp.Connections.IXON/
COPY Rebelit.OT.Discover.EdgeApp.Connections.OPCUA/  Rebelit.OT.Discover.EdgeApp.Connections.OPCUA/

RUN dotnet publish Rebelit.OT.Discover.EdgeApp/Rebelit.OT.Discover.EdgeApp.csproj \
    -c Release \
    -o /app/publish

# ── Runtime stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# ── IXON credentials ───────────────────────────────────────────────────────────
ARG IXON_ApplicationId
ARG IXON_CompanyId
ARG IXON_BearerToken

ENV IXON_ApplicationId="$IXON_ApplicationId"
ENV IXON_CompanyId="$IXON_CompanyId"
ENV IXON_BearerToken="$IXON_BearerToken"

ENTRYPOINT ["dotnet", "Rebelit.OT.Discover.EdgeApp.dll"]
