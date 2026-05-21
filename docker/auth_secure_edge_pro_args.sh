#!/bin/bash

# Sourced by non-interactive install/remove scripts.
# Expects SECURE_EDGE_IP, USERNAME, and PASSWORD to already be set by the caller.
# Sets: BASE_URL, COOKIE_JAR
# Caller is responsible for: rm -f "${COOKIE_JAR}" on exit.

if [[ -z "$SECURE_EDGE_IP" ]]; then
    echo "Error: SECURE_EDGE_IP must be set before sourcing this script." >&2
    exit 1
fi

if [[ -z "$USERNAME" ]]; then
    echo "Error: USERNAME must be set before sourcing this script." >&2
    exit 1
fi

if [[ -z "$PASSWORD" ]]; then
    echo "Error: PASSWORD must be set before sourcing this script." >&2
    exit 1
fi

BASE_URL="http://${SECURE_EDGE_IP}:80"
COOKIE_JAR="$(mktemp)"

echo "Authenticating with SecureEdge Pro..."
curl --silent --fail-with-body \
     --request POST \
     --url "${BASE_URL}/auth/login" \
     --cookie-jar "${COOKIE_JAR}" \
     --data "username=${USERNAME}" \
     --data "password=${PASSWORD}"
echo
