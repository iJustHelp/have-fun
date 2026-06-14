#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"

# ZIP tools do not always preserve Unix executable bits. Repair the launchers
# when the user starts this file with "bash app.sh".
/bin/chmod +x "$script_dir/app.sh" "$script_dir/scripts/run.sh" 2>/dev/null || true

exec /bin/bash "$script_dir/scripts/run.sh"
