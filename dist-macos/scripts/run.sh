#!/usr/bin/env bash
set -euo pipefail

port=3333

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
dist_root="$(cd -- "$script_dir/.." && pwd)"
exe_path="$dist_root/app/HaveFun.Web"
pid_path="$dist_root/havefun.pid"
log_path="$dist_root/havefun.log"
error_log_path="$dist_root/havefun.err.log"

is_private_ipv4() {
    local second_octet

    case "$1" in
        10.*|192.168.*) return 0 ;;
        172.*)
            second_octet="${1#172.}"
            second_octet="${second_octet%%.*}"
            [[ "$second_octet" -ge 16 && "$second_octet" -le 31 ]]
            return
            ;;
    esac

    return 1
}

get_interface_ipv4() {
    local interface_name="$1"
    /usr/sbin/ipconfig getifaddr "$interface_name" 2>/dev/null || true
}

get_preferred_local_ipv4() {
    local default_interface=""
    local interface_name
    local address
    local fallback_address=""

    default_interface="$(
        /sbin/route -n get default 2>/dev/null |
            /usr/bin/awk '/interface:/{print $2; exit}'
    )"

    for interface_name in "$default_interface" en0 en1; do
        [[ -n "$interface_name" ]] || continue

        address="$(get_interface_ipv4 "$interface_name")"
        [[ -n "$address" ]] || continue

        if is_private_ipv4 "$address"; then
            printf '%s\n' "$address"
            return 0
        fi

        [[ -n "$fallback_address" ]] || fallback_address="$address"
    done

    if [[ -z "$fallback_address" ]]; then
        fallback_address="$(
            /sbin/ifconfig |
                /usr/bin/awk '
                    $1 == "inet" && $2 != "127.0.0.1" && $2 !~ /^169\.254\./ {
                        print $2
                        exit
                    }
                '
        )"
    fi

    [[ -n "$fallback_address" ]] || return 1
    printf '%s\n' "$fallback_address"
}

stop_process() {
    local process_id="$1"
    local process_name

    kill -0 "$process_id" 2>/dev/null || return 0

    process_name="$(
        /bin/ps -p "$process_id" -o comm= 2>/dev/null |
            /usr/bin/awk -F/ '{print $NF}'
    )"

    if [[ "$process_name" != "HaveFun.Web" ]]; then
        echo "PID $process_id belongs to '$process_name', not HaveFun.Web. Leaving it running." >&2
        return 1
    fi

    echo "Stopping existing Have Fun process $process_id."
    kill "$process_id"

    for _ in 1 2 3 4 5 6 7 8 9 10; do
        kill -0 "$process_id" 2>/dev/null || return 0
        sleep 1
    done

    kill -9 "$process_id"
}

stop_previous_launch() {
    local previous_process_id

    [[ -f "$pid_path" ]] || return 0

    previous_process_id="$(/usr/bin/head -n 1 "$pid_path" | /usr/bin/tr -d '[:space:]')"

    if [[ ! "$previous_process_id" =~ ^[0-9]+$ ]]; then
        rm -f "$pid_path"
        return 0
    fi

    if ! kill -0 "$previous_process_id" 2>/dev/null; then
        rm -f "$pid_path"
        return 0
    fi

    stop_process "$previous_process_id"
    rm -f "$pid_path"
}

stop_havefun_listeners() {
    local listener_pids
    local process_id
    local process_name

    listener_pids="$(/usr/sbin/lsof -nP -iTCP:"$port" -sTCP:LISTEN -t 2>/dev/null || true)"
    [[ -n "$listener_pids" ]] || return 0

    for process_id in $listener_pids; do
        process_name="$(
            /bin/ps -p "$process_id" -o comm= 2>/dev/null |
                /usr/bin/awk -F/ '{print $NF}'
        )"

        if [[ "$process_name" != "HaveFun.Web" ]]; then
            echo "Port $port is already in use by PID $process_id ($process_name)." >&2
            exit 1
        fi

        stop_process "$process_id"
    done
}

if [[ "$(/usr/bin/uname -s)" != "Darwin" ]]; then
    echo "This launcher is for macOS." >&2
    exit 1
fi

if [[ ! -f "$exe_path" ]]; then
    echo "Published app not found at $exe_path." >&2
    exit 1
fi

/bin/chmod +x "$exe_path"

local_ip="$(get_preferred_local_ipv4)" || {
    echo "Could not determine a Wi-Fi/local IPv4 address." >&2
    exit 1
}

binding_url="http://$local_ip:$port"

stop_previous_launch
stop_havefun_listeners

cd -- "$(dirname -- "$exe_path")"
ASPNETCORE_URLS="$binding_url" /usr/bin/nohup "$exe_path" \
    >"$log_path" 2>"$error_log_path" </dev/null &
process_id=$!
printf '%s\n' "$process_id" >"$pid_path"

sleep 3

if ! kill -0 "$process_id" 2>/dev/null; then
    rm -f "$pid_path"
    echo "Have Fun exited immediately. See $error_log_path for details." >&2
    /usr/bin/tail -n 20 "$error_log_path" >&2 || true
    exit 1
fi

/usr/bin/open "$binding_url"

echo "Started Have Fun (PID $process_id) at $binding_url"
echo "Opened $binding_url"
echo "Logs: $log_path"
