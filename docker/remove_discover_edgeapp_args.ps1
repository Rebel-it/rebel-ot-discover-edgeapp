#requires -Version 5.1

<#
.SYNOPSIS
    Non-interactive variant of remove_discover_edgeapp.sh.
    Removes all rebel-ot-discover containers and images from the SecureEdge device.
.DESCRIPTION
    Run this script to clean up containers and images from the SecureEdge Pro device.
.PARAMETER SECURE_EDGE_IP
    The IP address of the SecureEdge Pro device.
.PARAMETER USERNAME
    The username for authentication.
.PARAMETER PASSWORD
    The password for authentication.
.EXAMPLE
    .\remove_discover_edgeapp_args.ps1 -SECURE_EDGE_IP 192.168.1.1 -USERNAME admin -PASSWORD password
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

# Authenticate with SecureEdge Pro
. "$SCRIPT_DIR/auth_secure_edge_pro_args.ps1" -SECURE_EDGE_IP $SECURE_EDGE_IP -USERNAME $USERNAME -PASSWORD $PASSWORD

function Stop-Container {
    param (
        [string]$Name
    )
    Write-Output "Stopping container: $Name"
    try {
        Invoke-RestMethod -Method Post -Uri "$BASE_URL/api/v1/docker/containers/$Name/stop" -WebSession $session -ErrorAction Stop
    } catch {
        Write-Warning "Container $Name could not be stopped or does not exist."
    }
}

function Delete-Container {
    param (
        [string]$Name
    )
    Write-Output "Deleting container: $Name"
    try {
        Invoke-RestMethod -Method Delete -Uri "$BASE_URL/api/v1/docker/containers/$Name" -WebSession $session -ErrorAction Stop
    } catch {
        Write-Warning "Container $Name could not be deleted or does not exist."
    }
}

function Delete-Image {
    param (
        [string]$Name
    )
    Write-Output "Deleting image: $Name"
    try {
        Invoke-RestMethod -Method Delete -Uri "$BASE_URL/api/v1/docker/images/$Name" -WebSession $session -ErrorAction Stop
    } catch {
        Write-Warning "Image $Name could not be deleted or does not exist."
    }
}

Write-Output "`n=== Stopping containers ==="
Stop-Container -Name $FRONTEND_CONTAINER
Stop-Container -Name $BACKEND_CONTAINER

Write-Output "`n=== Deleting containers ==="
Delete-Container -Name $FRONTEND_CONTAINER
Delete-Container -Name $BACKEND_CONTAINER

Write-Output "`n=== Deleting images ==="
Delete-Image -Name $FRONTEND_CONTAINER
Delete-Image -Name $BACKEND_CONTAINER

# Clean up temp cookie file
Remove-Item -Path $COOKIE_JAR -ErrorAction SilentlyContinue

Write-Output "`n=== Cleanup complete ==="
Write-Output "Recommended: Remove the IXON service account you created for this tool in the IXON Cloud to avoid leaving unused accounts."