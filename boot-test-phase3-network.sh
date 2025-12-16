#!/bin/bash
# BarrerOS Phase 3 - Network Test

qemu-system-x86_64 \
  -kernel ~/linux-kernel-6.6/arch/x86/boot/bzImage \
  -initrd bootstrap-init-v3 \
  -drive file=barreros-root.img,format=raw,if=virtio \
  -append "console=ttyS0 root=/dev/vda rw init=/sbin/barrer-init" \
  -m 512M \
  -nographic \
  -enable-kvm \
  -netdev user,id=net0 \
  -device e1000,netdev=net0

