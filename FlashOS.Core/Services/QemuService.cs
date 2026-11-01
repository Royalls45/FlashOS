using System.Diagnostics;
using FlashOS.Core.Interfaces;
using FlashOS.Core.Models;
namespace FlashOS.Core.Services;

public sealed class QemuService : IQemuService
{
    public QemuAvailability CheckAvailability(string? configuredPath = null)
    {
        var candidate = configuredPath;
        if (string.IsNullOrWhiteSpace(candidate))
            candidate = Environment.GetEnvironmentVariable("QEMU_PATH") ?? "qemu-system-x86_64.exe";
        try
        {
            var psi = new ProcessStartInfo(candidate, "--version") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
            using var process = Process.Start(psi);
            if (process is null) return new QemuAvailability { Message = "Не удалось запустить проверку QEMU" };
            var output = process.StandardOutput.ReadLine() ?? process.StandardError.ReadLine() ?? string.Empty;
            process.WaitForExit(3000);
            return new QemuAvailability { IsAvailable = process.ExitCode == 0, ExecutablePath = candidate, Version = output, Message = process.ExitCode == 0 ? "QEMU доступен" : "QEMU вернул ошибку" };
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
        { return new QemuAvailability { ExecutablePath = candidate, Message = $"QEMU не найден: {ex.Message}" }; }
    }

    public string BuildPreviewCommand(VirtualMachineConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.IsoPath) || !File.Exists(config.IsoPath)) throw new FileNotFoundException("ISO не найден", config.IsoPath);
        if (config.MemoryMegabytes < 512 || config.CpuCount < 1) throw new ArgumentOutOfRangeException(nameof(config), "Некорректные ресурсы VM");
        var args = $"-m {config.MemoryMegabytes} -smp {config.CpuCount} -display gtk";
        if (config.UseIso) args += $" -cdrom \"{config.IsoPath}\" -boot d";
        if (!string.IsNullOrWhiteSpace(config.DiskPath)) args += $" -drive file=\"{config.DiskPath}\",format=qcow2";
        return $"\"{config.QemuPath}\" {args}";
    }

    public async Task<ProcessResult> StartAsync(VirtualMachineConfig config, CancellationToken cancellationToken = default)
    {
        var availability = CheckAvailability(config.QemuPath);
        if (!availability.IsAvailable) return new ProcessResult { Message = availability.Message };
        var preview = BuildPreviewCommand(config);
        var start = preview.IndexOf(' ');
        var fileName = start > 0 ? preview[1..(start - 1)] : config.QemuPath;
        var arguments = start > 0 ? preview[(start + 1)..] : string.Empty;
        try
        {
            using var process = Process.Start(new ProcessStartInfo { FileName = fileName, Arguments = arguments, UseShellExecute = false, CreateNoWindow = false });
            if (process is null) return new ProcessResult { Message = "QEMU не запустился" };
            await process.WaitForExitAsync(cancellationToken);
            return new ProcessResult { Started = true, ExitCode = process.ExitCode, Message = $"QEMU завершился с кодом {process.ExitCode}" };
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
        { return new ProcessResult { Message = $"Ошибка запуска QEMU: {ex.Message}" }; }
    }
}
