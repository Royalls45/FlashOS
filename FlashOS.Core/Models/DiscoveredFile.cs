namespace FlashOS.Core.Models;

public enum DiscoveredFileType { Iso, Persistence, VentoyConfig, Script, Other }

public sealed class DiscoveredFile
{
    public string Path { get; init; } = string.Empty;
    public string Name => System.IO.Path.GetFileName(Path);
    public long SizeBytes { get; init; }
    public DiscoveredFileType Type { get; init; }
    public string TypeLabel => Type switch
    {
        DiscoveredFileType.Iso => "ISO-образ",
        DiscoveredFileType.Persistence => "Persistence",
        DiscoveredFileType.VentoyConfig => "Ventoy config",
        DiscoveredFileType.Script => "BAT-скрипт (не запущен)",
        _ => "Файл"
    };
}
