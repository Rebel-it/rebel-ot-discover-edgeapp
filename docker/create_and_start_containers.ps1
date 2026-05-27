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

    ,
    [Parameter(Mandatory = $true)]
    [Microsoft.PowerShell.Commands.WebRequestSession]$Session
)

Write-Output "Creating backend container..."
$backendBody = @{
    container = @{ name = $BACKEND_CONTAINER }
    image = @{ name = $BACKEND_CONTAINER; tag = "latest" }
    ports = @()
    mounts = @()
    networks = @(@{ name = $NETWORK; driver = "bridge" })
    environment_variables = @()
} | ConvertTo-Json -Depth 10
Invoke-RestMethod -Method Post -Uri "$BASE_URL/api/v1/docker/containers" -WebSession $Session -ContentType "application/json" -Body $backendBody | Out-Null

Write-Output "Creating frontend container..."
$frontendBody = @{
    container = @{ name = $FRONTEND_CONTAINER }
    image = @{ name = $FRONTEND_CONTAINER; tag = "latest" }
    ports = @(@{ source = 80; destination = 3000; protocol = "tcp" })
    mounts = @()
    networks = @(@{ name = $NETWORK; driver = "bridge" })
    environment_variables = @()
} | ConvertTo-Json -Depth 10
Invoke-RestMethod -Method Post -Uri "$BASE_URL/api/v1/docker/containers" -WebSession $Session -ContentType "application/json" -Body $frontendBody | Out-Null

Write-Output "Starting backend container..."
Invoke-RestMethod -Method Post -Uri "$BASE_URL/api/v1/docker/containers/$BACKEND_CONTAINER/start" -WebSession $Session | Out-Null

Write-Output "Starting frontend container..."
Invoke-RestMethod -Method Post -Uri "$BASE_URL/api/v1/docker/containers/$FRONTEND_CONTAINER/start" -WebSession $Session | Out-Null
