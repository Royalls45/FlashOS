using FlashOS.Core.Models;
namespace FlashOS.Core.Interfaces;

public interface IPersistenceAnalysisService
{
    VentoyAnalysis AnalyzeVentoy(string path, string rootPath);
    ScriptAnalysis AnalyzeScript(string path);
}

public interface IQemuService
{
    QemuAvailability CheckAvailability(string? configuredPath = null);
    string BuildPreviewCommand(VirtualMachineConfig config);
    Task<ProcessResult> StartAsync(VirtualMachineConfig config, CancellationToken cancellationToken = default);
}

public sealed class ProcessResult
{
    public bool Started { get; init; }
    public int? ExitCode { get; init; }
    public string Message { get; init; } = string.Empty;
}

public interface IBackupService
{
    string CreateBackup(string sourcePath, string backupDirectory);
}
