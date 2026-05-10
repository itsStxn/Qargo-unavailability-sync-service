#!/bin/bash

#? Run the console app
#? Note: It runs by locking and skipping concurrent executions

set -euo pipefail

PROJECT_DIR="$HOME/Dev/projects/Qargo Unavailability Sync Service"

cd "$PROJECT_DIR" || {
	echo "Project directory not found: $PROJECT_DIR"
	exit 1
}

echo "Starting project..."

flock -n /tmp/qargo-sync.lock \
dotnet "$PROJECT_DIR/publish/QargoUnavailabilitySyncService.dll"

echo "✅ Project has run successfully"
