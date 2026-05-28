#requires -Version 5.1

<#
.SYNOPSIS
    Non-interactive variant of deploy_discover_edgeapp.sh.
    Deploys pre-built images from the release zip to a SecureEdge Pro device.
.DESCRIPTION
    Run this script from the directory where you extracted the release zip.
.PARAMETER SECURE_EDGE_IP
    The IP address of the SecureEdge Pro device.
.PARAMETER USERNAME
    The username for authentication.
.PARAMETER PASSWORD
    The password for authentication.
.EXAMPLE
    .\deploy_discover_edgeapp_args.ps1 -SECURE_EDGE_IP 192.168.1.1 -USERNAME admin -PASSWORD password
#>

param (
    [Parameter(Mandatory = $true)]
    [string]$SECURE_EDGE_IP,

    [Parameter(Mandatory = $true)]
    [string]$USERNAME,

    [Parameter(Mandatory = $true)]
    [string]$PASSWORD
)

$SCRIPT_DIR = Split-Path -Parent $MyInvocation.MyCommand.Definition
$BACKEND_CONTAINER = "rebel-ot-discover-edgeapp"
$FRONTEND_CONTAINER = "rebel-ot-discover-edgeapp-react"
$NETWORK = "machine-builder"

$env:SECURE_EDGE_IP = $SECURE_EDGE_IP
$env:USERNAME = $USERNAME
$env:PASSWORD = $PASSWORD

function Get-LoadedImageReference {
    param(
        [Parameter(Mandatory = $true)]
        [string]$TarPath
    )

    $loadOutput = docker load -i $TarPath 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "docker load failed for '$TarPath'."
        exit 1
    }
    $loadText = ($loadOutput | Out-String)

    if ($loadText -match "Loaded image:\s*(.+)") {
        return $matches[1].Trim()
    }

    if ($loadText -match "Loaded image ID:\s*(.+)") {
        return $matches[1].Trim()
    }

    return $null
}

# Check dependencies
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Error "Error: 'docker' is required but not installed."
    Write-Error "  https://docs.docker.com/get-docker/"
    exit 1
}

# Check insecure registry configuration
if (-not (docker info 2>$null | Select-String -Quiet ([regex]::Escape("$($SECURE_EDGE_IP):5000")))) {
    Write-Error "Error: '$($SECURE_EDGE_IP):5000' is not configured as an insecure registry in Docker."
    Write-Error "  Add it via Docker Desktop → Settings → Docker Engine:"
    Write-Error "    { \"insecure-registries\": [\"$($SECURE_EDGE_IP):5000\"] }"
    Write-Error "  Then restart Docker and re-run this script."
    exit 1
}

# Authenticate with SecureEdge Pro
. "$SCRIPT_DIR/auth_secure_edge_pro_args.ps1" -SECURE_EDGE_IP $SECURE_EDGE_IP -USERNAME $USERNAME -PASSWORD $PASSWORD

# Load and push images to SecureEdge Pro registry
Write-Output "Loading backend image..."
$BACKEND_IMAGE = Get-LoadedImageReference -TarPath "$SCRIPT_DIR/rebel-ot-discover-edgeapp.tar"
if (-not $BACKEND_IMAGE) {
    Write-Error "Could not determine backend image reference from docker load output."
    exit 1
}
docker tag $BACKEND_IMAGE "$($SECURE_EDGE_IP):5000/${BACKEND_CONTAINER}:latest"
if ($LASTEXITCODE -ne 0) { exit 1 }
Write-Output "Pushing backend image..."
docker push -q "$($SECURE_EDGE_IP):5000/${BACKEND_CONTAINER}:latest" *> $null
if ($LASTEXITCODE -ne 0) { exit 1 }
docker rmi $BACKEND_IMAGE "$($SECURE_EDGE_IP):5000/${BACKEND_CONTAINER}:latest" *> $null

Write-Output "Loading frontend image..."
$FRONTEND_IMAGE = Get-LoadedImageReference -TarPath "$SCRIPT_DIR/rebel-ot-discover-edgeapp-react.tar"
if (-not $FRONTEND_IMAGE) {
    Write-Error "Could not determine frontend image reference from docker load output."
    exit 1
}
docker tag $FRONTEND_IMAGE "$($SECURE_EDGE_IP):5000/${FRONTEND_CONTAINER}:latest"
if ($LASTEXITCODE -ne 0) { exit 1 }
Write-Output "Pushing frontend image..."
docker push -q "$($SECURE_EDGE_IP):5000/${FRONTEND_CONTAINER}:latest" *> $null
if ($LASTEXITCODE -ne 0) { exit 1 }
docker rmi $FRONTEND_IMAGE "$($SECURE_EDGE_IP):5000/${FRONTEND_CONTAINER}:latest" *> $null

# Create and start containers
. "$SCRIPT_DIR/create_and_start_containers.ps1" `
    -BASE_URL $BASE_URL `
    -COOKIE_JAR $COOKIE_JAR `
    -BACKEND_CONTAINER $BACKEND_CONTAINER `
    -FRONTEND_CONTAINER $FRONTEND_CONTAINER `
    -NETWORK $NETWORK `
    -SECURE_EDGE_IP $SECURE_EDGE_IP `
    -Session $session

Remove-Item -Path "$COOKIE_JAR" -ErrorAction SilentlyContinue

Write-Output "`n=== Deployment complete ==="
Write-Output "Please visit the application on your Secure Edge Pro: http://$SECURE_EDGE_IP:3000"