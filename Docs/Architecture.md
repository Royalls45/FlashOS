# Архитектура FlashOS

`FlashOS.App` содержит WPF UI и ViewModel. `FlashOS.Core` содержит модели, интерфейсы и сервисы без зависимости от UI.

Поток первого этапа:

`MainWindow` → `MainViewModel` → `IUsbScannerService` → `DriveInfo` / безопасный обход файловой системы.

Сканер использует `EnumerationOptions.IgnoreInaccessible`, не запускает найденные BAT-файлы, не пишет на накопитель и обрабатывает отключение USB через исключения доступа и ввода-вывода.
