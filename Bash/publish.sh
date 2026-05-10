#!/bin/bash

# ? Publish the project => dll files

########################################
# * SETTINGS / SAFETY
########################################

set -euo pipefail


########################################
# * ERROR HANDLING
########################################

on_error() {
	echo "❌ Project has not been published"
}

trap on_error ERR


########################################
# * VARIABLES
########################################

PROJECT_DIR="$HOME/Dev/projects/Qargo Unavailability Sync Service"


########################################
# * MAIN LOGIC
########################################

cd "$PROJECT_DIR"

trash -f obj bin

dotnet publish \
	./*.csproj \
	-c Release \
	-r linux-x64 \
	--self-contained false \
	-o "./publish"

echo "✅ Project has been published"
