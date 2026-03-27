#!/usr/bin/env bash
set -euo pipefail

CERT_PATH="${ASPNETCORE_Kestrel__Certificates__Default__Path:-/https/hotelbooking-selfsigned.pfx}"
CERT_PASSWORD="${ASPNETCORE_Kestrel__Certificates__Default__Password:-${CERT_PASSWORD:-}}"
CERT_DNS_NAME="${CERT_DNS_NAME:-localhost}"
CERT_VALID_DAYS="${CERT_VALID_DAYS:-365}"

if [[ -z "${CERT_PASSWORD}" ]]; then
  echo "CERT_PASSWORD or ASPNETCORE_Kestrel__Certificates__Default__Password must be set." >&2
  exit 1
fi

mkdir -p "$(dirname "${CERT_PATH}")"

if [[ ! -f "${CERT_PATH}" ]]; then
  echo "No TLS certificate found at ${CERT_PATH}. Generating a self-signed certificate for ${CERT_DNS_NAME}..."
  /usr/local/bin/generate-self-signed-cert.sh "${CERT_DNS_NAME}" "${CERT_PASSWORD}" "$(dirname "${CERT_PATH}")" "${CERT_VALID_DAYS}"
else
  echo "Using existing TLS certificate at ${CERT_PATH}."
fi

exec dotnet HotelBookingPlatform.API.dll
