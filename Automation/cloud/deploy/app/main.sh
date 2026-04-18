#!/bin/bash

set -e

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../../.." && pwd)"
cd "$PROJECT_ROOT/IAC-Terraform"


log() {
  echo "[$(date +'%Y-%m-%d %H:%M:%S')] $1"
}


log "Terraform init"
terraform init

log "Terraform apply"
terraform apply -auto-approve

log "Deployed"