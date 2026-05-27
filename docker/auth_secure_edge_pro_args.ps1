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
Write-Host "SecureEdge IP: ${env:SECURE_EDGE_IP}"
Write-Host "Username: ${env:USERNAME}"
Write-Host "Password: ${env:PASSWORD}"
Write-Host "Authenticating with SecureEdge Pro..."

# Step 1: GET to /auth/login to set initial cookies (mimic curl)
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
try {
    Invoke-WebRequest -Uri "$BASE_URL/auth/login" -Method GET -WebSession $session -UseBasicParsing -TimeoutSec 10 -ErrorAction Stop | Out-Null
} catch {
    Write-Warning "Initial GET to /auth/login failed: $($_.Exception.Message)"
}

# Step 2: POST credentials as form data
$form = @{
    username = $env:USERNAME
    password = $env:PASSWORD
}
try {
    $authResponse = Invoke-WebRequest -Uri "$BASE_URL/auth/login" `
        -Method POST `
        -WebSession $session `
        -UseBasicParsing `
        -Body $form `
        -TimeoutSec 20 `
        -ErrorAction Stop
} catch {
    Write-Error "Authentication POST failed: $($_.Exception.Message)"
    exit 1
}

# Save cookies to the temporary file
try {
    $session.Cookies.GetCookies($BASE_URL) | ForEach-Object {
        $_.ToString() | Out-File -FilePath $COOKIE_JAR -Append
    }
    Write-Host "Authentication successful. Cookies saved to $COOKIE_JAR."
} catch {
    Write-Warning "Could not save cookies: $($_.Exception.Message)"
}

# Check for authentication failure in response content
if ($authResponse.Content -match "Invalid credentials") {
    Write-Error "Error: authentication failed."
    exit 1
}