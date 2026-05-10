#!/bin/bash

#? Publish the project => dll files

set -euo pipefail

PROJECT_DIR="$HOME/Dev/projects/Qargo Unavailability Sync Service"

cd "$PROJECT_DIR" || {
	echo "Project directory not found: $PROJECT_DIR"
	exit 1
}

trash obj bin

dotnet publish \
	./*.csproj \
	-c Release \
	-r linux-x64 \
	--self-contained false \
	-o "./publish"

echo "✅ Project has been published"
