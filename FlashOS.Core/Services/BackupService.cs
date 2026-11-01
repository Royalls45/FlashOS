using FlashOS.Core.Interfaces;
namespace FlashOS.Core.Services;
public sealed class BackupService : IBackupService
{
    public string CreateBackup(string sourcePath, string backupDirectory)
    {
        if (!File.Exists(sourcePath)) throw new FileNotFoundException("Исходный файл не найден", sourcePath);
        Directory.CreateDirectory(backupDirectory);
        var destination = Path.Combine(backupDirectory, $"{Path.GetFileName(sourcePath)}.{DateTime.UtcNow:yyyyMMdd-HHmmss}.bak");
        File.Copy(sourcePath, destination, overwrite: false);
        return destination;
    }
}
