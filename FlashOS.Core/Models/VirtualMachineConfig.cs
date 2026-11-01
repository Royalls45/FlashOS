namespace FlashOS.Core.Models;

public sealed class VirtualMachineConfig
{
    public string Name { get; init; } = "Linux Mint";
    public string IsoPath { get; init; } = string.Empty;
    public string DiskPath { get; init; } = string.Empty;
    public string QemuPath { get; init; } = "qemu-system-x86_64.exe";
    public int MemoryMegabytes { get; init; } = 4096;
    public int CpuCount { get; init; } = 2;
    public bool UseIso { get; init; } = true;
}

public sealed class QemuAvailability
{
    public bool IsAvailable { get; init; }
    public string ExecutablePath { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
