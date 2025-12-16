# barrerd-devmgr - BarrerOS Device Manager

**Version**: 1.0  
**Language**: C# (.NET 10)  
**Type**: System Service (Native AOT)

## What It Does

BarrerOS's device manager - replaces udev with a modern C# implementation.

### Features

- **Initial Device Scan**: Discovers all devices at startup
- **Hot-Plug Monitoring**: Detects device additions/removals
- **Multiple Device Types**: Block, PCI, network, input devices
- **Concurrent Tracking**: Thread-safe device registry
- **Event Logging**: Reports all device changes

### Supported Devices

- **Block Devices**: Disks, partitions from `/sys/block`
- **PCI Devices**: Storage, network, display, USB controllers from `/sys/bus/pci`
- **Network Devices**: Ethernet, WiFi interfaces from `/sys/class/net`
- **Input Devices**: Keyboards, mice from `/sys/class/input`

## Build

```bash
dotnet publish -c Release
```

Produces native AOT binary: `bin/Release/net10.0/linux-x64/publish/barrerd-devmgr` (2.1MB)

## Usage

```bash
# Run as service (usually started by init)
./barrerd-devmgr

# Stop with Ctrl+C
```

## Architecture

- Main thread: Coordination
- Background task 1: Block device monitor (2s polling)
- Background task 2: PCI device monitor (5s polling)
- `ConcurrentDictionary` for thread-safe device tracking
- Scans `/sys` hierarchy for device discovery

## Integration

Part of BarrerOS Phase 2 - System Services.

Works with `barrerd-log` for event logging.

Designed to be started early in boot after `/sys` is mounted.

---

**Built**: December 16, 2025  
**BarrerOS**: First .NET-native Linux distribution  
🏴‍☠️💙
