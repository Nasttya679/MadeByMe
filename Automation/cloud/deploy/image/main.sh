#!/bin/bash

set -e

# Move to project root (2 levels up from script location)
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../../.." && pwd)"

cd $PROJECT_ROOT

# Default values
IMAGE_NAME="made-by-me-app"
VERSION="1.0"
AWS_ACCOUNT_ID="971778147356"
REGION="us-east-1"

# Parse args
while [[ $# -gt 0 ]]; do
  case $1 in
    --image)
      IMAGE_NAME="$2"
      shift 2
      ;;
    --version)
      VERSION="$2"
      shift 2
      ;;
    --account)
      AWS_ACCOUNT_ID="$2"
      shift 2
      ;;
    --region)
      REGION="$2"
      shift 2
      ;;
    *)
      echo "Unknown parameter: $1"
      exit 1
      ;;
  esac
done


log() {
  echo "[$(date +'%Y-%m-%d %H:%M:%S')] $1"
}


REPOSITORY="$AWS_ACCOUNT_ID.dkr.ecr.$REGION.amazonaws.com/$IMAGE_NAME"

log "Building Docker image"
docker build -t $IMAGE_NAME:$VERSION .


log  "Logging in to AWS ECR"
aws ecr get-login-password --region $REGION \
| docker login --username AWS --password-stdin $AWS_ACCOUNT_ID.dkr.ecr.$REGION.amazonaws.com


log "Tagging image"
docker tag $IMAGE_NAME:$VERSION $REPOSITORY:$VERSION


log "Pushing image"
docker push $REPOSITORY:$VERSION


log "Cleaning up local Docker images"

docker rmi $IMAGE_NAME:$VERSION || true
docker rmi $REPOSITORY:$VERSION || true


log "Deployed"
