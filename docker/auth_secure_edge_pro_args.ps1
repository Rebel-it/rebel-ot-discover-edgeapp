# PowerShell script equivalent of auth_secure_edge_pro_args.sh

# Ensure required variables are set
if (-not $env:SECURE_EDGE_IP) {
    Write-Error "Error: SECURE_EDGE_IP must be set before running this script."
    exit 1
}

if (-not $env:USERNAME) {
    Write-Error "Error: USERNAME must be set before running this script."
    exit 1
}

if (-not $env:PASSWORD) {
    Write-Error "Error: PASSWORD must be set before running this script."
    exit 1
}

# Define variables
$BASE_URL = "http://${env:SECURE_EDGE_IP}:80"
$COOKIE_JAR = [System.IO.Path]::GetTempFileName()

# Authenticate with SecureEdge Pro
Write-Host "Authenticating with SecureEdge Pro..."
$response = Invoke-WebRequest -Uri "$BASE_URL/auth/login" `
    -Method Post `
    -SessionVariable session `
    -Body @{ username = $env:USERNAME; password = $env:PASSWORD } `
    -ErrorAction Stop

# Save cookies to the temporary file
$response.RawContentStream | Set-Content -Path $COOKIE_JAR

Write-Host "Authentication successful. Cookies saved to $COOKIE_JAR."