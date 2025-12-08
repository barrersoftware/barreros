# Linux Kernel 6.18.0 Bug Report
**Reported by:** Daniel Elliott / BarrerOS Project  
**Date:** December 6, 2025  
**Kernel Version:** 6.18.0 (commit 2061f18ad76e)  

## Summary
NULL pointer dereference in `sched_mm_cid_fork()` causing kernel panic when executing init process.

## Bug Details
- **Location:** `kernel/sched/core.c:10572` in `sched_mm_cid_fork()`
- **Error:** NULL pointer dereference at address `0x0000000000000120`
- **Impact:** Kernel panic, unable to execute init, system unusable

## Reproduction Steps
1. Build Linux 6.18.0 with default config + `CONFIG_SCHED_MM_CID=y`
2. Create minimal initramfs with busybox and init script
3. Boot kernel with QEMU:
   ```bash
   qemu-system-x86_64 \
     -kernel arch/x86/boot/bzImage \
     -initrd initramfs.cpio.gz \
     -append "console=ttyS0" \
     -nographic -m 2G
   ```
4. Kernel boots successfully through hardware init
5. When attempting to execute `/init`, panic occurs in scheduler

## Stack Trace
```
[   35.602467] BUG: kernel NULL pointer dereference, address: 0000000000000120
[   35.610561] #PF: supervisor read access in kernel mode
[   35.611556] #PF: error_code(0x0000) - not-present page
[   35.615343] Oops: Oops: 0000 [#1] SMP NOPTI
[   35.617073] CPU: 0 UID: 0 PID: 1 Comm: swapper/0 Tainted: G        W
[   35.621606] RIP: 0010:mutex_lock+0x17/0x30
[   35.635609] Call Trace:
[   35.636279]  <TASK>
[   35.636882]  sched_mm_cid_fork+0x47/0x410
[   35.637867]  bprm_execve+0x14e/0x540
[   35.638770]  kernel_execve+0xf3/0x140
[   35.640580]  kernel_init+0x7e/0x1c0
[   35.641439]  ret_from_fork+0x131/0x190
[   35.644155]  </TASK>
[   35.653118] ---[ end trace 0000000000000000 ]---
[   35.677196] Kernel panic - not syncing: Attempted to kill init! exitcode=0x00000009
```

## Analysis
- Bug occurs in `sched_mm_cid_fork()` at offset +0x47
- Function attempts to lock mutex at NULL+0x120 address
- Happens during `bprm_execve()` when kernel tries to execute init
- Bug is in scheduler code, not in userspace
- Introduced by CONFIG_SCHED_MM_CID feature (new CPU concurrency tracking)

## Workaround
Disable `CONFIG_SCHED_MM_CID` in kernel config:
```bash
scripts/config --disable CONFIG_SCHED_MM_CID
```
System boots successfully with this feature disabled.

## System Information
- **Hardware:** QEMU/KVM virtual machine (x86_64)
- **CPU:** QEMU Virtual CPU
- **Memory:** 2GB
- **Config:** Default defconfig with minimal modifications
- **Compiler:** GCC (Ubuntu version)

## Additional Notes
This appears to be a NULL pointer in the new per-mm concurrency ID tracking code introduced for improved scheduler performance. The feature is attempting to access an uninitialized structure during process fork/exec.

The bug prevents any userspace code from executing, making the system completely unusable with this feature enabled.

## Suggested Fix Areas
- Check initialization of mm_cid structures before first fork
- Verify locking order in `sched_mm_cid_fork()`
- Review NULL pointer handling in the concurrency ID path
- Consider making CONFIG_SCHED_MM_CID depend on additional initialization

## Contact
BarrerOS Project - Building first .NET-native operating system  
This bug discovered during Phase 1 kernel integration testing
