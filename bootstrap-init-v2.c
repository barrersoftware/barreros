#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <sys/mount.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <sys/sysmacros.h>

int main(int argc, char *argv[]) {
    printf("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
    printf("🏴‍☠️ BarrerOS Phase 2.7 - Enhanced Bootstrap\n");
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
    mount("tmpfs", "/tmp", "tmpfs", 0, "mode=1777");
    
    // Create essential device nodes if devtmpfs didn't
    printf("Creating device nodes...\n");
    mknod("/dev/null", S_IFCHR | 0666, makedev(1, 3));
    mknod("/dev/zero", S_IFCHR | 0666, makedev(1, 5));
    mknod("/dev/random", S_IFCHR | 0666, makedev(1, 8));
    mknod("/dev/urandom", S_IFCHR | 0666, makedev(1, 9));
    
    // Set up environment variables
    printf("Setting environment...\n");
    setenv("PATH", "/bin:/sbin:/usr/bin:/usr/sbin:/lib/dotnet", 1);
    setenv("HOME", "/root", 1);
    setenv("USER", "root", 1);
    setenv("LOGNAME", "root", 1);
    setenv("TERM", "linux", 1);
    setenv("DOTNET_ROOT", "/lib/dotnet", 1);
    setenv("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "1", 1);
    
    printf("\n✅ System initialization complete\n\n");

    // Launch .NET init
    printf("Launching .NET runtime...\n\n");
    
    char *dotnet_args[] = {
        "/lib/dotnet/dotnet",
        "/sbin/barrer-init",
        NULL
    };
    
    execv("/lib/dotnet/dotnet", dotnet_args);
    
    // If we get here, exec failed
    perror("execv failed");
    printf("❌ Failed to launch .NET init\n");
    return 1;
}
