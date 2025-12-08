#!/bin/bash
cd ~/barreros-phase2
qemu-system-x86_64 \
  -kernel ~/linux-kernel-6.6/arch/x86/boot/bzImage \
  -drive file=~/barreros-phase2/barreros-root.img,format=raw,if=virtio \
  -append "root=/dev/vda rw console=ttyS0" \
  -nographic \
  -m 2G \
  -smp 2
