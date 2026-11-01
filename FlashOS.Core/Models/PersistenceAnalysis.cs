namespace FlashOS.Core.Models;

public sealed class VentoyPersistenceEntry
{
    public string IsoPath { get; init; } = string.Empty;
    public string PersistenceFile { get; init; } = string.Empty;
    public string Status { get; init; } = "Не проверено";
}

public sealed class VentoyAnalysis
{
    public string FilePath { get; init; } = string.Empty;
    public bool IsValidJson { get; init; }
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<VentoyPersistenceEntry> Entries { get; init; } = [];
}

public sealed class ScriptAnalysis
{
    public string FilePath { get; init; } = string.Empty;
    public bool ReadSuccessfully { get; init; }
    public bool ContainsPotentiallyDestructiveCommands { get; init; }
    public IReadOnlyList<string> Warnings { get; init; } = [];
    public string Status => !ReadSuccessfully ? "Не удалось прочитать" : ContainsPotentiallyDestructiveCommands ? "Требует ручной проверки" : "Прочитан как текст; не запускался";
}
