namespace FlashOS.Core.Models;

public sealed class UsbDeviceInfo
{
    public string RootPath { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string FileSystem { get; init; } = string.Empty;
    public long TotalBytes { get; init; }
    public long FreeBytes { get; init; }
    public string TotalSize => FormatBytes(TotalBytes);
    public string FreeSpace => FormatBytes(FreeBytes);
    public string Label => $"{DisplayName} ({RootPath})";

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        string[] units = ["KB", "MB", "GB", "TB"];
        double value = bytes;
        var unit = -1;
        while (value >= 1024 && unit < units.Length - 1) { value /= 1024; unit++; }
        return $"{value:0.##} {units[unit]}";
    }
}
