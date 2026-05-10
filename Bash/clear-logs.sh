#!/bin/bash

# ? Clear project's logs

########################################
# * SETTINGS / SAFETY
########################################

set -euo pipefail


########################################
# * ERROR HANDLING
########################################

on_error() {
	echo "❌ Project logs have not been cleared"
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

trash -f "$PROJECT_DIR/Logs/"*.log

echo "✅ Project logs have been cleared"
