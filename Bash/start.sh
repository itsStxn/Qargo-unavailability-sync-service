#!/bin/bash

# ? Run the console app
# ? Note: It runs by locking and skipping concurrent executions

########################################
# * SETTINGS / SAFETY
########################################

set -euo pipefail


########################################
# * ERROR HANDLING
########################################

on_error() {
	echo "❌ Project failed to run"
}

trap on_error ERR


########################################
# * VARIABLES
########################################

PROJECT_DIR="$HOME/Dev/projects/Qargo Unavailability Sync Services"


########################################
# * MAIN LOGIC
########################################

cd "$PROJECT_DIR"

echo "Starting project..."

flock -n /tmp/qargo-sync.lock \
dotnet "$PROJECT_DIR/publish/QargoUnavailabilitySyncService.dll"

echo "✅ Project run successfully"
