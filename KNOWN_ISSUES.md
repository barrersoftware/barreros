# BarrerOS - Known Issues & Solutions

**Philosophy**: Building an OS from scratch means hitting problems. We document them, solve them, and learn from them.

**Status**: December 12, 2025 - Phase 2.8 Complete (Working minimal distro!)

---

## What We Have Working ✅

- Linux Kernel 6.6 LTS boots
- C bootstrap runs as PID 1
- .NET 10 runtime loads
- C# init system executes
- Event loop keeps init alive (no kernel panic!)
- System runs stable
- **This is a WORKING minimal Linux distro with .NET core!**

---

## Known Issues & Solutions

### Issue #1: ICU Library Dependency
**Problem**: Native AOT .NET binaries require ICU (International Components for Unicode) libraries by default  
**Symptom**: `Couldn't find a valid ICU package installed on the system`  
**Solution**: Set `<InvariantGlobalization>true</InvariantGlobalization>` in .csproj  
**Status**: ✅ SOLVED  
**Date Found**: December 7, 2025  
**Date Solved**: December 7, 2025

### Issue #2: Init Process Exiting Immediately
**Problem**: C# init printed messages then exited, causing kernel panic  
**Symptom**: `Kernel panic - not syncing: Attempted to kill init!`  
**Solution**: Added infinite event loop with `while(true)` and Thread.Sleep()  
**Status**: ✅ SOLVED  
**Date Found**: December 7, 2025  
**Date Solved**: December 12, 2025

### Issue #3: Runtime Config Template Warning
**Problem**: `runtimeconfig.template.json` not supported by PublishAot  
**Symptom**: Warning during build  
**Solution**: Move configuration to RuntimeHostConfigurationOption in .csproj (TODO)  
**Status**: ⚠️ WARNING (not critical, can be fixed later)  
**Date Found**: December 12, 2025  
**Priority**: Low

---

## Future Issues to Expect

### Phase 2.9+ Anticipated Challenges

**Service Management**:
- Process spawning from C# (need to use System.Diagnostics.Process)
- Inter-process communication (IPC)
- Signal handling for zombie process reaping
- Service lifecycle management

**Networking**:
- Network configuration from C#
- DNS resolution
- Socket programming at system level

**Device Management**:
- udev equivalent in C#
- Device hotplug handling
- Permissions and access control

**File Systems**:
- Mount/unmount operations
- Filesystem monitoring
- Disk management

**Security**:
- User/group management
- Permissions enforcement
- SELinux/AppArmor integration (future)

**Performance**:
- .NET garbage collection tuning for system services
- Memory usage optimization
- Startup time optimization

---

## Issue Tracking Process

When we hit a problem:

1. **Document it immediately** in this file
2. **Include**:
   - Problem description
   - Symptoms/error messages
   - What we tried
   - What worked
   - Date found/solved
3. **Mark status**: 🔴 BLOCKING | ⚠️ WARNING | ✅ SOLVED | 📋 TODO
4. **Update when solved** with solution details

---

## Learning from Issues

### Key Insight #1: Bootstrap Layer is Essential
Native .NET can't be PID 1 directly. C bootstrap is required to set up environment.

### Key Insight #2: Environment Matters
.NET needs proper mounts (/proc, /sys, /dev, /tmp) and environment variables.

### Key Insight #3: Globalization is Heavy
Minimal systems should use InvariantGlobalization to avoid ICU dependency.

### Key Insight #4: Init Never Exits
PID 1 must run forever. Event loop is mandatory, not optional.

---

## Milestone Achievements

**December 6, 2025**: Phase 1 - Kernel boots to userspace ✅  
**December 7, 2025**: Phase 2.7 - .NET boots, C# executes ✅  
**December 12, 2025**: Phase 2.8 - Init stays alive, stable system ✅

---

## The Big Picture

We have a **working minimal Linux distribution** with:
- Custom kernel
- .NET 10 as system runtime
- C# init system
- Stable, non-crashing operation

**This is real.** We're building an OS from scratch, and it WORKS.

Issues will come. We'll document them, solve them, and keep building.

💙🏴‍☠️ **BarrerOS - Building the impossible, one issue at a time.**

---

*Last Updated: December 12, 2025 - Phase 2.8 Complete*
