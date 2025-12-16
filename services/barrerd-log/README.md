# barrerd-log - BarrerOS Logging Service

**Version**: 1.0  
**Language**: C# (.NET 10)  
**Type**: System Service (Native AOT)

## What It Does

BarrerOS's logging service - replaces traditional syslog with a modern C# implementation.

### Features

- **Kernel Log Reader**: Reads from `/proc/kmsg` and captures kernel messages
- **Async Queue**: Non-blocking concurrent queue for log entries
- **Dual Output**: Writes to both file and console
- **Separate Logs**: System log and kernel log files
- **Graceful Shutdown**: Handles Ctrl+C properly

### Log Files

- `/var/log/barreros/system.log` - Application and system logs
- `/var/log/barreros/kernel.log` - Kernel messages

## Build

```bash
dotnet publish -c Release
```

Produces native AOT binary: `bin/Release/net10.0/linux-x64/publish/barrerd-log` (2.1MB)

## Usage

```bash
# Run as service (usually started by init)
./barrerd-log

# Stop with Ctrl+C
```

## Architecture

- Main thread: Coordination
- Background task 1: Kernel log reader (`/proc/kmsg`)
- Background task 2: Log queue writer
- `ConcurrentQueue` for thread-safe log buffering
- `StreamWriter` with AutoFlush for immediate writes

## Integration

Part of BarrerOS Phase 2 - System Services.

Designed to be started early in boot process by C# init system.

---

**Built**: December 16, 2025  
**BarrerOS**: First .NET-native Linux distribution  
🏴‍☠️💙
