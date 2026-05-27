#requires -Version 5.1

<#
.SYNOPSIS
    Creates and starts the backend and frontend containers on a SecureEdge Pro device.
.DESCRIPTION
    Expects the following variables to already be set by the caller:
        BASE_URL            - e.g. http://172.27.21.1:80
        COOKIE_JAR          - path to the authenticated cookie jar
        BACKEND_CONTAINER   - container/image name for the backend
        FRONTEND_CONTAINER  - container/image name for the frontend
        NETWORK             - Docker network name
        SECURE_EDGE_IP      - used only for the final success message
#>

param (
    [Parameter(Mandatory = $true)]
    [string]$BASE_URL,

    [Parameter(Mandatory = $true)]
    [string]$COOKIE_JAR,

    [Parameter(Mandatory = $true)]
    [string]$BACKEND_CONTAINER,

    [Parameter(Mandatory = $true)]
    [string]$FRONTEND_CONTAINER,

    [Parameter(Mandatory = $true)]
    [string]$NETWORK,

    [Parameter(Mandatory = $true)]
    [string]$SECURE_EDGE_IP
)

Write-Output "Creating backend container..."
Invoke-RestMethod -Method Post -Uri "$BASE_URL/api/v1/docker/containers" -WebSession $session -Body @{
    container = @{ name = $BACKEND_CONTAINER };
    image = @{ name = $BACKEND_CONTAINER; tag = "latest" };
    ports = @();
    mounts = @();
    networks = @(@{ name = $NETWORK; driver = "bridge" });
    environment_variables = @()
} | ConvertTo-Json -Depth 10

Write-Output "Creating frontend container..."
Invoke-RestMethod -Method Post -Uri "$BASE_URL/api/v1/docker/containers" -WebSession $session -Body @{
    container = @{ name = $FRONTEND_CONTAINER };
    image = @{ name = $FRONTEND_CONTAINER; tag = "latest" };
    ports = @(@{ source = 80; destination = 3000; protocol = "tcp" });
    mounts = @();
    networks = @(@{ name = $NETWORK; driver = "bridge" });
    environment_variables = @()
} | ConvertTo-Json -Depth 10

Write-Output "Starting backend container..."
Invoke-RestMethod -Method Post -Uri "$BASE_URL/api/v1/docker/containers/$BACKEND_CONTAINER/start" -WebSession $session
