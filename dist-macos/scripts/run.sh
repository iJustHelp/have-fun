#!/usr/bin/env bash
set -euo pipefail

port=3333

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
dist_root="$(cd -- "$script_dir/.." && pwd)"
app_path="$dist_root/app/HaveFun.Web"
pid_path="$dist_root/havefun.pid"
log_path="$dist_root/havefun.log"

die() {
    printf 'Error: %s\n' "$*" >&2
    exit 1
}

is_private_ipv4() {
    local ip="$1"
    [[ "$ip" == 10.* || "$ip" == 192.168.* || "$ip" =~ ^172\.(1[6-9]|2[0-9]|3[0-1])\. ]]
}

get_interface_ip() {
    local interface="$1"
    ipconfig getifaddr "$interface" 2>/dev/null || true
}

get_default_interface() {
    route -n get default 2>/dev/null | awk '/interface:/{print $2; exit}' || true
}

get_preferred_local_ipv4() {
    local interfaces=()
    local default_interface

    default_interface="$(get_default_interface)"
    if [[ -n "$default_interface" ]]; then
        interfaces+=("$default_interface")
    fi

    interfaces+=("en0" "en1")

    local interface ip
    for interface in "${interfaces[@]}"; do
        [[ -n "$interface" ]] || continue
        ip="$(get_interface_ip "$interface")"
        if [[ -n "$ip" && "$ip" != 127.* && "$ip" != 169.254.* ]] && is_private_ipv4 "$ip"; then
            printf '%s\n' "$ip"
            return 0
        fi
    done

    for interface in "${interfaces[@]}"; do
        [[ -n "$interface" ]] || continue
        ip="$(get_interface_ip "$interface")"
        if [[ -n "$ip" && "$ip" != 127.* && "$ip" != 169.254.* ]]; then
            printf '%s\n' "$ip"
            return 0
        fi
    done

    ifconfig 2>/dev/null |
        awk '/inet / && $2 !~ /^127\./ && $2 !~ /^169\.254\./ { print $2; exit }' || true
}

process_command() {
    local process_id="$1"
    ps -p "$process_id" -o command= 2>/dev/null || true
}

stop_pid_file_process() {
    [[ -f "$pid_path" ]] || return 0

    local previous_pid
    previous_pid="$(head -n 1 "$pid_path" 2>/dev/null || true)"

    if [[ ! "$previous_pid" =~ ^[0-9]+$ ]]; then
        rm -f "$pid_path"
        return 0
    fi

    local command_line
    command_line="$(process_command "$previous_pid")"

    if [[ -z "$command_line" ]]; then
        rm -f "$pid_path"
        return 0
    fi

    if [[ "$command_line" == *"$app_path"* || "$command_line" == *"HaveFun.Web"* ]]; then
        printf 'Stopping existing Have Fun process %s.\n' "$previous_pid"
        kill "$previous_pid" 2>/dev/null || true
        sleep 2
        kill -9 "$previous_pid" 2>/dev/null || true
        rm -f "$pid_path"
    else
        printf 'Warning: PID file points to process %s, but it is not Have Fun. Leaving it running.\n' "$previous_pid" >&2
    fi
}

stop_listeners_on_port() {
    command -v lsof >/dev/null 2>&1 || return 0

    local pids process_id command_line
    pids="$(lsof -tiTCP:"$port" -sTCP:LISTEN 2>/dev/null || true)"

    [[ -n "$pids" ]] || return 0

    while IFS= read -r process_id; do
        [[ -n "$process_id" ]] || continue
        command_line="$(process_command "$process_id")"

        if [[ "$command_line" == *"$app_path"* || "$command_line" == *"HaveFun.Web"* ]]; then
            printf 'Stopping existing Have Fun process %s on port %s.\n' "$process_id" "$port"
            kill "$process_id" 2>/dev/null || true
            sleep 2
            kill -9 "$process_id" 2>/dev/null || true
        fi
    done <<< "$pids"
}

assert_port_available() {
    command -v lsof >/dev/null 2>&1 || return 0

    local pids
    pids="$(lsof -tiTCP:"$port" -sTCP:LISTEN 2>/dev/null || true)"

    [[ -z "$pids" ]] || die "Port $port is already in use by process id(s): $(printf '%s' "$pids" | tr '\n' ' ')"
}

if [[ ! "$port" =~ ^[0-9]+$ || "$port" -lt 1 || "$port" -gt 65535 ]]; then
    die "Port must be a number from 1 to 65535."
fi

[[ "$(uname -s)" == "Darwin" ]] || die "This launcher is for macOS."
[[ -f "$app_path" ]] || die "Published app not found at $app_path."

xattr -dr com.apple.quarantine "$dist_root" 2>/dev/null || true

if [[ ! -x "$app_path" ]]; then
    chmod +x "$app_path"
fi

local_ip="$(get_preferred_local_ipv4)"
[[ -n "$local_ip" ]] || die "Could not determine a Wi-Fi/local IPv4 address."

binding_url="http://$local_ip:$port"
browser_url="$binding_url"

stop_pid_file_process
stop_listeners_on_port
assert_port_available

(
    cd "$dist_root/app"
    ASPNETCORE_URLS="$binding_url" nohup "$app_path" > "$log_path" 2>&1 </dev/null &
    printf '%s\n' "$!" > "$pid_path"
)

process_id="$(head -n 1 "$pid_path")"
sleep 3

if ! kill -0 "$process_id" 2>/dev/null; then
    rm -f "$pid_path"
    printf 'Have Fun exited immediately. Recent log output:\n' >&2
    tail -n 40 "$log_path" >&2 || true
    exit 1
fi

if command -v open >/dev/null 2>&1; then
    open "$browser_url"
fi

printf 'Started Have Fun (PID %s) at %s\n' "$process_id" "$binding_url"
printf 'Opened %s\n' "$browser_url"
printf 'Players must be on the same Wi-Fi/LAN and use the displayed URL.\n'
