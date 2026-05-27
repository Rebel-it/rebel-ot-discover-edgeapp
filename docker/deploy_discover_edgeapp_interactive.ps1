#requires -Version 5.1

<#
.SYNOPSIS
    Interactive variant of deploy_discover_edgeapp_args.ps1.
    Prompts for SecureEdge IP, username, and password if not provided.
.DESCRIPTION
    Run this script from the directory where you extracted the release zip.
#>

param (
    [Parameter(Mandatory = $false)]
    [string]$SECURE_EDGE_IP,

    [Parameter(Mandatory = $false)]
    [string]$USERNAME,

    [Parameter(Mandatory = $false)]
    [string]$PASSWORD
)

# Prompt for parameters if not set
if (-not $SECURE_EDGE_IP) {
    $SECURE_EDGE_IP = Read-Host "SecureEdge IP address (e.g. 172.27.21.1)"
}
if (-not $SECURE_EDGE_IP) {
    Write-Error "Error: SecureEdge IP address is required."
    exit 1
}

if (-not $USERNAME) {
    $USERNAME = Read-Host "Username"
}
if (-not $USERNAME) {
    Write-Error "Error: Username is required."
    exit 1
}

if (-not $PASSWORD) {
    $securePassword = Read-Host -AsSecureString "Password"
    if (-not $securePassword) {
        Write-Error "Error: Password is required."
        exit 1
    }

    $BSTR = [IntPtr]::Zero
    try {
        $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
        $PASSWORD = [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($BSTR)
    } finally {
        if ($BSTR -ne [IntPtr]::Zero) {
            [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
        }
    }
}
if (-not $PASSWORD) {
    Write-Error "Error: Password is required."
    exit 1
}

$SCRIPT_DIR = Split-Path -Parent $MyInvocation.MyCommand.Definition
$BACKEND_CONTAINER = "rebel-ot-discover-edgeapp"
$FRONTEND_CONTAINER = "rebel-ot-discover-edgeapp-react"
$NETWORK = "machine-builder"

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

# Set environment variables for downstream scripts
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

# Authenticate with SecureEdge Pro
. "$SCRIPT_DIR/auth_secure_edge_pro_args.ps1" -SECURE_EDGE_IP $SECURE_EDGE_IP -USERNAME $USERNAME -PASSWORD $PASSWORD

# Load and push images to SecureEdge Pro registry
Write-Output "Loading backend image..."
$BACKEND_IMAGE = Get-LoadedImageReference -TarPath "$SCRIPT_DIR/rebel-ot-discover-edgeapp.tar"
if (-not $BACKEND_IMAGE) {
    Write-Error "Could not determine backend image reference from docker load output."
    
}
docker tag $BACKEND_IMAGE "$($SECURE_EDGE_IP):5000/${BACKEND_CONTAINER}:latest"
if ($LASTEXITCODE -ne 0) { exit 1 }
Write-Output "Pushing backend image..."
docker push "$($SECURE_EDGE_IP):5000/${BACKEND_CONTAINER}:latest"
if ($LASTEXITCODE -ne 0) { exit 1 }
docker rmi $BACKEND_IMAGE "$($SECURE_EDGE_IP):5000/${BACKEND_CONTAINER}:latest" 2>$null | Out-Null

Write-Output "Loading frontend image..."
$FRONTEND_IMAGE = Get-LoadedImageReference -TarPath "$SCRIPT_DIR/rebel-ot-discover-edgeapp-react.tar"
if (-not $FRONTEND_IMAGE) {
    Write-Error "Could not determine frontend image reference from docker load output."
   
}
docker tag $FRONTEND_IMAGE "$($SECURE_EDGE_IP):5000/${FRONTEND_CONTAINER}:latest"
if ($LASTEXITCODE -ne 0) { exit 1 }
Write-Output "Pushing frontend image..."
docker push "$($SECURE_EDGE_IP):5000/${FRONTEND_CONTAINER}:latest"
if ($LASTEXITCODE -ne 0) { exit 1 }
docker rmi $FRONTEND_IMAGE "$($SECURE_EDGE_IP):5000/${FRONTEND_CONTAINER}:latest" 2>$null | Out-Null

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
