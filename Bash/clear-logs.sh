#!/bin/bash

#? Clear project's logs

set -euo pipefail

PROJECT_DIR="$HOME/Dev/projects/Qargo Unavailability Sync Service"

cd "$PROJECT_DIR" || {
	echo "Project directory not found: $PROJECT_DIR"
	exit 1
}

trash "$PROJECT_DIR/Logs/"*.log

echo "✅ Project logs have been cleared"
