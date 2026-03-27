#!/usr/bin/env bash
set -euo pipefail

DNS_NAME="${1:-localhost}"
PASSWORD="${2:-change-this-pfx-password}"
OUTPUT_DIR="${3:-certs}"
VALID_DAYS="${4:-730}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

if [[ "${OUTPUT_DIR}" = /* ]]; then
  TARGET_DIR="${OUTPUT_DIR}"
else
  TARGET_DIR="${REPO_ROOT}/${OUTPUT_DIR}"
fi

mkdir -p "${TARGET_DIR}"

CRT_PATH="${TARGET_DIR}/hotelbooking-selfsigned.crt"
KEY_PATH="${TARGET_DIR}/hotelbooking-selfsigned.key"
PFX_PATH="${TARGET_DIR}/hotelbooking-selfsigned.pfx"
OPENSSL_CONFIG_PATH="${TARGET_DIR}/hotelbooking-selfsigned-openssl.cnf"

cat > "${OPENSSL_CONFIG_PATH}" <<EOF
[req]
default_bits = 2048
prompt = no
default_md = sha256
distinguished_name = dn
x509_extensions = v3_req

[dn]
CN = ${DNS_NAME}

[v3_req]
subjectAltName = @alt_names

[alt_names]
DNS.1 = ${DNS_NAME}
EOF

openssl req -x509 -nodes -newkey rsa:2048 \
  -keyout "${KEY_PATH}" \
  -out "${CRT_PATH}" \
  -days "${VALID_DAYS}" \
  -config "${OPENSSL_CONFIG_PATH}" \
  -extensions v3_req

openssl pkcs12 -export \
  -out "${PFX_PATH}" \
  -inkey "${KEY_PATH}" \
  -in "${CRT_PATH}" \
  -password "pass:${PASSWORD}"

echo "Created certificate at: ${PFX_PATH}"
echo "Use CERT_PASSWORD=${PASSWORD} in your deployment environment."
