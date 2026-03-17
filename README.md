# rebel-ot-discover-edgeapp

[![SonarQube Cloud](https://sonarcloud.io/images/project_badges/sonarcloud-light.svg)](https://sonarcloud.io/summary/new_code?id=rebel-it_rebel-ot-discover-edgeapp)

A .NET edge application that automatically discovers OPC UA data nodes and provisions them as Variables and Tags in the IXON Cloud platform. It is designed to run as a Docker container on an IXON SecureEdge Pro device.

## What it does

On a 15-minute cycle the application:

1. Connects to a configured OPC UA server and recursively browses its full address space.
2. For every data node found, creates a corresponding **Variable** (data point definition) and **Tag** (logging configuration) in the IXON Cloud via the IXON API v2.
3. Skips nodes that already exist in IXON, so repeated runs are safe and idempotent.

The result is that all OPC UA variables exposed by connected industrial equipment become immediately available for logging, alarming, and visualisation in the IXON portal — without any manual configuration.

### Architecture

| Component | Role |
|-----------|------|
| `Rebelit.OT.Discover.EdgeApp` | Entry point, 15-minute run loop |
| `Rebelit.OT.Discover.EdgeApp.Connections.OPCUA` | OPC UA client (OPC Foundation .NET SDK) |
| `Rebelit.OT.Discover.EdgeApp.Connections.IXON` | IXON API v2 client (Variables & Tags endpoints) |

## Getting started

### Prerequisites

- [Docker](https://docs.docker.com/get-docker/) with [BuildKit / `docker buildx`](https://docs.docker.com/buildx/working-with-buildx/) enabled.
- An IXON SecureEdge Pro device accessible on your local network (or with vpn connection through the portal).
- An IXON Cloud account with the **API Integrations** module enabled.

### Building and deploying to a SecureEdge Pro

The script [docker/push_ixon.sh](docker/push_ixon.sh) builds a `linux/arm64/v8` image and pushes it directly to the container registry on the SecureEdge Pro. Run it from the `docker/` directory:

```bash
cd docker
./push_ixon.sh
```

The script will prompt for four values:

| Prompt | Description | How to obtain |
|--------|-------------|---------------|
| **SecureEdge IP address** | The LAN IP address of your IXON SecureEdge Pro (e.g. `172.27.21.1`). | Check your network configuration or the IXON portal under the device's network settings. |
| **IXON Application ID** | Identifies the client application calling the IXON API. | Contact your IXON Sales representative or account manager to request an Application ID. |
| **IXON Company ID** | The `publicId` of your IXON company. | Retrieve it via the IXON API [`CompanyList`](https://developer.ixon.cloud/reference/get_companies) endpoint, or find it in the IXON portal URL when you navigate to your company settings. |
| **IXON Bearer Token** | A temporary API access token used to authenticate all API calls. | Generate one via the IXON API [`AccessTokenList`](https://developer.ixon.cloud/reference/post_access-tokens) endpoint (see below). |

The credentials are baked into the Docker image as environment variables at build time, so no secrets need to be managed on the device after deployment.

#### Generating an IXON Bearer Token

A bearer token is obtained by calling the `AccessTokenList` endpoint with your IXON email and password using HTTP Basic Auth. The token expires after the duration you specify (`expiresIn`, in seconds; maximum 60 days / 5 184 000 seconds).

```bash
# 1. Base64-encode your credentials (email::password)
CIPHER=$(echo -n 'your@email.com::YourPassword' | base64)

# 2. Request a bearer token (here valid for 24 hours)
curl --request POST \
     --url 'https://portal.ixon.cloud/api/access-tokens?fields=secretId' \
     --header 'Api-Version: 2' \
     --header "Api-Application: YOUR_APPLICATION_ID" \
     --header 'Content-Type: application/json' \
     --header "Authorization: Basic $CIPHER" \
     --data '{"expiresIn": 86400}'
```

The `secretId` value in the response is your bearer token. Enter only the token value when prompted by `push_ixon.sh` (without the `Bearer` prefix).

> **Note:** If your account uses Two-Factor Authentication, encode your credentials as `email:otp:password` instead.

## Configuration

The following values are currently hard-coded in `Scraper.cs` and may need to be updated before deployment:

| Value | Location | Default |
|-------|----------|---------|
| OPC UA endpoint URL | `Scraper.cs` | `opc.tcp://172.27.21.3:4840` |
| OPC UA username | `Scraper.cs` | `pip` |
| OPC UA password | `Scraper.cs` | `innovations` |
| Target IXON agent ID | `Scraper.cs` | `ajParmNkfASN` |
| IXON data source public ID | `Scraper.cs` | `LJrOZaLiaJjB` |
| Tag retention policy | `Scraper.cs` | `260w` (5 years) |
| Tag logging interval | `Scraper.cs` | `72s` |
| Scrape interval | `Application.cs` | 15 minutes |

