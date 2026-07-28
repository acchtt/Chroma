using Chroma.Services;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;

namespace Chroma;

public partial class App : Application
{
    public static MainWindow? MainWindowInstance { get; private set; }

    public App()
    {
        InitializeComponent();
        AppInstance.GetCurrent().Activated += Current_Activated;
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _ = UpdateService.CleanupStaleUpdateFilesAsync();
        MainWindowInstance = new MainWindow();
        MainWindowInstance.EnableUpdateExperience();
        MainWindowInstance.EnableGpuSelector();
        MainWindowInstance.EnableCustomResolutionEditor();
        MainWindowInstance.EnableProfileLayoutRefresh();
        MainWindowInstance.EnableUpdateButtonCopy();
        MainWindowInstance.EnableFooterUtilityBar();
        MainWindowInstance.EnableAntiCheatSafety();
        BrandPalette.Apply(Resources);
        MainWindowInstance.Activate();
        MainWindowInstance.NotifyWindowOpened();
    }

    private void Current_Activated(object? sender, AppActivationArguments args)
    {
        MainWindow? window = MainWindowInstance;
        window?.DispatcherQueue.TryEnqueue(() =>
        {
            window.ShowAndActivate();
            window.NotifyWindowOpened();
        });
    }
}
