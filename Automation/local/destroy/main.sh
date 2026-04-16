#!/bin/bash

set -e

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)"
cd "$PROJECT_ROOT"


log() {
  echo "[$(date +'%Y-%m-%d %H:%M:%S')] $1"
}


log "Stopping and removing local application"

docker compose down -v

log "Destroyed"
