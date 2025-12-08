#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <sys/mount.h>
#include <sys/stat.h>

int main(int argc, char *argv[]) {
    printf("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
    printf("🏴‍☠️ BarrerOS Phase 2 - C Bootstrap Init\n");
    printf("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n");

    // Mount essential filesystems
    printf("Mounting /proc...\n");
    mkdir("/proc", 0755);
    mount("proc", "/proc", "proc", 0, NULL);
    
    printf("Mounting /sys...\n");
    mkdir("/sys", 0755);
    mount("sysfs", "/sys", "sysfs", 0, NULL);
    
    printf("Mounting /dev...\n");
    mkdir("/dev", 0755);
    mount("devtmpfs", "/dev", "devtmpfs", 0, NULL);
    
    printf("Mounting /tmp...\n");
    mkdir("/tmp", 0755);
    mount("tmpfs", "/tmp", "tmpfs", 0, NULL);

    printf("\n✅ System mounts complete\n\n");

    // Launch .NET init
    printf("Launching .NET init via /lib/dotnet/dotnet...\n\n");
    
    char *dotnet_args[] = {
        "/lib/dotnet/dotnet",
        "/sbin/barrer-init",
        NULL
    };
    
    execv("/lib/dotnet/dotnet", dotnet_args);
    
    // If we get here, exec failed
    printf("❌ Failed to launch .NET init\n");
    return 1;
}
