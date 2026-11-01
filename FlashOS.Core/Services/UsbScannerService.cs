using System.IO;
using FlashOS.Core.Interfaces;
using FlashOS.Core.Models;

namespace FlashOS.Core.Services;

public sealed class UsbScannerService : IUsbScannerService
{
    public IReadOnlyList<UsbDeviceInfo> ScanDevices(CancellationToken cancellationToken = default)
    {
        var devices = new List<UsbDeviceInfo>();
        foreach (var drive in DriveInfo.GetDrives())
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (drive.DriveType != DriveType.Removable || !drive.IsReady) continue;
                devices.Add(new UsbDeviceInfo
                {
                    RootPath = drive.RootDirectory.FullName,
                    DisplayName = string.IsNullOrWhiteSpace(drive.VolumeLabel) ? "Съёмный накопитель" : drive.VolumeLabel,
                    FileSystem = drive.DriveFormat,
                    TotalBytes = drive.TotalSize,
                    FreeBytes = drive.AvailableFreeSpace
                });
            }
            catch (IOException) { /* накопитель мог быть отключён во время сканирования */ }
            catch (UnauthorizedAccessException) { }
        }
        return devices;
    }

    public IReadOnlyList<DiscoveredFile> ScanFiles(string rootPath, CancellationToken cancellationToken = default)
    {
        var result = new List<DiscoveredFile>();
        if (!Directory.Exists(rootPath)) return result;
        var options = new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = true, ReturnSpecialDirectories = false };
        try
        {
            foreach (var file in Directory.EnumerateFiles(rootPath, "*", options))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var extension = Path.GetExtension(file).ToLowerInvariant();
                var name = Path.GetFileName(file).ToLowerInvariant();
                var type = extension == ".iso" ? DiscoveredFileType.Iso
                    : name == "persistence.bat" ? DiscoveredFileType.Script
                    : name == "persistence.dat" || extension == ".dat" ? DiscoveredFileType.Persistence
                    : name == "ventoy.json" ? DiscoveredFileType.VentoyConfig : DiscoveredFileType.Other;
                if (type == DiscoveredFileType.Other) continue;
                long size = 0;
                try { size = new FileInfo(file).Length; } catch (IOException) { }
                result.Add(new DiscoveredFile { Path = file, SizeBytes = size, Type = type });
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        return result.OrderBy(x => x.Type).ThenBy(x => x.Name).ToList();
    }
}
