#!/bin/bash

set -e

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)"
cd "$PROJECT_ROOT"


log() {
  echo "[$(date +'%Y-%m-%d %H:%M:%S')] $1"
}


log "Starting local application with Docker Compose"

docker compose up --build -d

log "Deployed"
