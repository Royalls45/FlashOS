using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using FlashOS.Core.Interfaces;
using FlashOS.Core.Models;

namespace FlashOS.App.ViewModels;
public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly IUsbScannerService _scanner;
    private readonly IPersistenceAnalysisService _analysis;
    private readonly IQemuService _qemu;
    private UsbDeviceInfo? _selectedDevice;
    private string _analysisStatus = "Выберите накопитель";
    public ObservableCollection<UsbDeviceInfo> Devices { get; } = [];
    public ObservableCollection<DiscoveredFile> Files { get; } = [];
    public ObservableCollection<string> Log { get; } = [];
    public UsbDeviceInfo? SelectedDevice { get => _selectedDevice; set { _selectedDevice = value; OnPropertyChanged(); if (value is not null) ScanSelected(); } }
    public string PersistenceStatus => Files.Any(f => f.Type == DiscoveredFileType.Persistence) ? "Найдено хранилище — совместимость не подтверждена" : "Хранилище не найдено";
    public string AnalysisStatus { get => _analysisStatus; private set { _analysisStatus = value; OnPropertyChanged(); } }
    public ICommand RefreshCommand { get; }
    public ICommand ScanCommand { get; }
    public ICommand LaunchCommand { get; }
    public MainViewModel(IUsbScannerService scanner, IPersistenceAnalysisService analysis, IQemuService qemu)
    { _scanner = scanner; _analysis = analysis; _qemu = qemu; RefreshCommand = new RelayCommand(Refresh); ScanCommand = new RelayCommand(ScanSelected); LaunchCommand = new RelayCommand(LaunchIso); Refresh(); }
    private void Refresh()
    {
        Devices.Clear(); Files.Clear();
        try { foreach (var d in _scanner.ScanDevices()) Devices.Add(d); AddLog($"Найдено съёмных накопителей: {Devices.Count}"); if (Devices.Count == 0) AddLog("Съёмные накопители не обнаружены."); }
        catch (Exception ex) { AddLog($"Ошибка сканирования: {ex.Message}"); }
    }
    private void ScanSelected()
    {
        if (SelectedDevice is null) return;
        Files.Clear();
        try
        {
            foreach (var f in _scanner.ScanFiles(SelectedDevice.RootPath)) Files.Add(f);
            foreach (var file in Files.Where(f => f.Type == DiscoveredFileType.VentoyConfig))
            { var report = _analysis.AnalyzeVentoy(file.Path, SelectedDevice.RootPath); AddLog($"Ventoy: {report.Status}"); foreach (var e in report.Entries) AddLog($"Persistence: {e.IsoPath} → {e.PersistenceFile}: {e.Status}"); }
            foreach (var file in Files.Where(f => f.Type == DiscoveredFileType.Script))
            { var report = _analysis.AnalyzeScript(file.Path); AddLog($"persistence.bat: {report.Status}"); foreach (var warning in report.Warnings) AddLog($"Предупреждение: {warning}"); }
            OnPropertyChanged(nameof(PersistenceStatus)); AnalysisStatus = $"Проверено файлов FlashOS: {Files.Count}"; AddLog($"Проверен накопитель {SelectedDevice.RootPath}");
        }
        catch (Exception ex) { AnalysisStatus = "Ошибка доступа"; AddLog($"Ошибка доступа к накопителю: {ex.Message}"); }
    }
    private async void LaunchIso()
    {
        var iso = Files.FirstOrDefault(f => f.Type == DiscoveredFileType.Iso);
        if (iso is null) { AddLog("Запуск отменён: ISO не найден."); return; }
        var config = new VirtualMachineConfig { IsoPath = iso.Path };
        try
        {
            var preview = _qemu.BuildPreviewCommand(config);
            var answer = MessageBox.Show($"Будет запущен QEMU с ISO:\n{iso.Path}\n\nКоманда:\n{preview}\n\nPersistence не подтверждён. Продолжить?", "Подтверждение запуска", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (answer != MessageBoxResult.Yes) { AddLog("Запуск QEMU отменён пользователем."); return; }
            AddLog("Запуск QEMU..."); var result = await _qemu.StartAsync(config); AddLog(result.Message);
        }
        catch (Exception ex) { AddLog($"Запуск невозможен: {ex.Message}"); }
    }
    private void AddLog(string message) => Log.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
    public event PropertyChangedEventHandler? PropertyChanged; private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}
public sealed class RelayCommand(Action execute) : ICommand { public event EventHandler? CanExecuteChanged; public bool CanExecute(object? p) => true; public void Execute(object? p) => execute(); }
