using System.Windows;
using FlashOS.App.ViewModels;
using FlashOS.Core.Services;
namespace FlashOS.App;
public partial class MainWindow : Window { public MainWindow() { InitializeComponent(); DataContext = new MainViewModel(new UsbScannerService(), new PersistenceAnalysisService(), new QemuService()); } }
