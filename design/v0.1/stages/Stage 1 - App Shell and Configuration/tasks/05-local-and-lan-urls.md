# Task 05: Local and LAN URL Service

## Goal

Provide URLs that the first page can show to the host.

## Work

- Add a server-side service that always provides a localhost URL.
- Detect a non-loopback IPv4 address when available.
- Build a LAN join URL from the detected address and current app port.
- Keep the app usable when LAN detection fails.
- Avoid any internet dependency.

## Done Criteria

- Localhost URL is always available.
- LAN URL is available when a non-loopback IPv4 address can be detected.
- LAN detection failure does not prevent startup.

## Verification

- Run locally and confirm a localhost URL appears.
- Confirm a LAN URL appears on a machine with a non-loopback IPv4 address.
- Confirm no cloud or external network access is required.
