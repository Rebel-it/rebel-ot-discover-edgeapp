#!/bin/bash

# Sourced by install/remove scripts. Expects SECURE_EDGE_IP to already be set.
# Sets: BASE_URL, COOKIE_JAR
# Caller is responsible for: rm -f "${COOKIE_JAR}" on exit.

if [[ -z "$SECURE_EDGE_IP" ]]; then
    echo "Error: SECURE_EDGE_IP must be set before sourcing this script." >&2
    exit 1
fi

read -rp "Username SecureEdge Pro [admin]: " USERNAME
USERNAME=${USERNAME:-admin}

read -rsp "Password SecureEdge Pro: " PASSWORD
echo
echo

if [[ -z "$PASSWORD" ]]; then
    echo "Error: password is required." >&2
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
