# barrerd-net - BarrerOS Network Manager

**Version**: 1.0  
**Language**: C# (.NET 10)  
**Type**: System Service (Native AOT)

## What It Does

BarrerOS's network manager - configures interfaces, handles IP addressing, manages connectivity.

### Features

- **Interface Discovery**: Scans `/sys/class/net` for all network devices
- **Interface Configuration**: Brings interfaces up/down using `ip` command
- **Loopback Setup**: Configures 127.0.0.1 automatically
- **State Monitoring**: Tracks link up/down events
- **Type Detection**: Identifies ethernet, wireless, bridge, virtual interfaces
- **DHCP Ready**: Prepared for DHCP client integration

### Supported Interface Types

- **Loopback**: `lo` - configured with 127.0.0.1/8
- **Ethernet**: `eth*`, `enp*` - wired network
- **Wireless**: `wlan*`, `wlp*` - WiFi adapters
- **Bridge**: `br*` - network bridges
- **Virtual**: `docker*`, `veth*` - container interfaces

## Build

```bash
dotnet publish -c Release
```

Produces native AOT binary: `bin/Release/net10.0/linux-x64/publish/barrerd-net` (2.5MB)

## Usage

```bash
# Run as service (usually started by init)
./barrerd-net

# Stop with Ctrl+C
```

## Architecture

- Main thread: Coordination
- Discovery phase: Scans `/sys/class/net`
- Configuration phase: Uses `ip` command for interface setup
- Monitor task: Polls interface states (3s interval)
- Uses `Process.Start()` for executing system commands

## Integration

Part of BarrerOS Phase 2 - System Services.

Works with:
- `barrerd-devmgr` - device discovery
- `barrerd-log` - event logging

Designed to be started after device manager in boot sequence.

## Future Enhancements

- Built-in DHCP client (no external dependency)
- DNS configuration management
- Static IP configuration from config file
- Wireless network management (WPA supplicant)
- Routing table management

---

**Built**: December 16, 2025  
**BarrerOS**: First .NET-native Linux distribution  
🏴‍☠️💙
