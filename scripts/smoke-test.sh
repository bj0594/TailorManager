#!/usr/bin/env bash
# Fails unless the API answers 200 on /health.
set -euo pipefail

url="${1:-http://localhost:8080}"
status=$(curl -s -o /dev/null -w "%{http_code}" "$url/health" || true)

if [ "$status" != "200" ]; then
  echo "FAIL: $url/health answered '$status'"
  exit 1
fi
echo "OK: $url/health answered 200"
