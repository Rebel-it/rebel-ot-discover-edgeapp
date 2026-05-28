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
curl --silent --show-error \
    --cookie-jar "${COOKIE_JAR}" \
    "${BASE_URL}/auth/login" >/dev/null

AUTH_RESPONSE="$(curl --silent --show-error --location \
    --request POST \
    --url "${BASE_URL}/auth/login" \
    --cookie "${COOKIE_JAR}" \
    --cookie-jar "${COOKIE_JAR}" \
    --data-urlencode "username=${USERNAME}" \
    --data-urlencode "password=${PASSWORD}")"

if echo "${AUTH_RESPONSE}" | grep -qi "Invalid credentials"; then
    echo "Error: authentication failed." >&2
    exit 1
fi
echo
