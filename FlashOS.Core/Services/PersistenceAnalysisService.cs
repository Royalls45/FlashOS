using System.Text.Json;
using FlashOS.Core.Interfaces;
using FlashOS.Core.Models;
namespace FlashOS.Core.Services;

public sealed class PersistenceAnalysisService : IPersistenceAnalysisService
{
    public VentoyAnalysis AnalyzeVentoy(string path, string rootPath)
    {
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var entries = new List<VentoyPersistenceEntry>();
            if (document.RootElement.TryGetProperty("persistence", out var persistence) && persistence.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in persistence.EnumerateArray())
                {
                    var iso = item.TryGetProperty("image", out var image) ? image.GetString() ?? string.Empty : string.Empty;
                    var backend = item.TryGetProperty("backend", out var backendValue) ? backendValue.GetString() ?? string.Empty : string.Empty;
                    entries.Add(new VentoyPersistenceEntry { IsoPath = iso, PersistenceFile = backend, Status = ResolvePath(rootPath, backend) ? "Файл найден; совместимость не подтверждена" : "Файл не найден" });
                }
            }
            return new VentoyAnalysis { FilePath = path, IsValidJson = true, Status = entries.Count == 0 ? "JSON прочитан, записей Persistence не найдено" : $"JSON прочитан, записей: {entries.Count}", Entries = entries };
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        { return new VentoyAnalysis { FilePath = path, Status = $"Ошибка анализа: {ex.Message}" }; }
    }

    public ScriptAnalysis AnalyzeScript(string path)
    {
        try
        {
            var text = File.ReadAllText(path);
            var warnings = new List<string>();
            var lower = text.ToLowerInvariant();
            foreach (var token in new[] { "format ", "del ", "rd ", "rmdir ", "diskpart", "shutdown", "powershell", "curl ", "bitsadmin" })
                if (lower.Contains(token)) warnings.Add($"Обнаружена команда или токен: {token.Trim()}");
            return new ScriptAnalysis { FilePath = path, ReadSuccessfully = true, ContainsPotentiallyDestructiveCommands = warnings.Count > 0, Warnings = warnings };
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        { return new ScriptAnalysis { FilePath = path, Warnings = [$"Ошибка чтения: {ex.Message}"] }; }
    }

    private static bool ResolvePath(string root, string path) => !string.IsNullOrWhiteSpace(path) && File.Exists(Path.Combine(root, path.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar)));
}
