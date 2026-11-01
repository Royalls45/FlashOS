using FlashOS.Core.Models;

namespace FlashOS.Core.Interfaces;

public interface IUsbScannerService
{
    IReadOnlyList<UsbDeviceInfo> ScanDevices(CancellationToken cancellationToken = default);
    IReadOnlyList<DiscoveredFile> ScanFiles(string rootPath, CancellationToken cancellationToken = default);
}
