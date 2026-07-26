using System.Globalization;
using Chroma.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Chroma;

public sealed partial class MainWindow
{
    private bool _updateExperienceEnabled;
    private bool _windowOpenCheckInProgress;
    private int _windowOpenCount;
    private long _updatesButtonTextCallbackToken;
    private ContentDialog? _updateProgressDialog;
    private TextBlock? _updateProgressStatusText;
    private TextBlock? _updateProgressPercentText;
    private ProgressBar? _updateProgressBar;

    public void EnableUpdateExperience()
    {
        if (_updateExperienceEnabled)
        {
            return;
        }

        _updateExperienceEnabled = true;
        _updatesButtonTextCallbackToken = UpdatesButtonText.RegisterPropertyChangedCallback(
            TextBlock.TextProperty,
            UpdatesButtonText_PropertyChanged);
        Closed += MainWindow_UpdateExperienceClosed;
    }

    public void NotifyWindowOpened()
    {
        EnableUpdateExperience();
        int openNumber = ++_windowOpenCount;
        _ = CheckForUpdatesForWindowOpenAsync(openNumber);
    }

    private async Task CheckForUpdatesForWindowOpenAsync(int openNumber)
    {
        if (_windowOpenCheckInProgress)
        {
            return;
        }

        _windowOpenCheckInProgress = true;
        try
        {
            // The first activation initializes profiles and starts the existing
            // background update check. Wait for that initialization to finish
            // before trying to display an update dialog.
            for (int attempt = 0;
                 attempt < 100 &&
                 (!_initialized || !_statusTimer.IsEnabled || Root.XamlRoot is null);
                 attempt++)
            {
                await Task.Delay(100);
            }

            if (!_initialized || Root.XamlRoot is null)
            {
                return;
            }

            if (openNumber == 1)
            {
                // Give MainWindow_Activated enough time to begin its silent
                // startup check, then wait for its result.
                await Task.Delay(150);
                while (_updateCheckInProgress)
                {
                    await Task.Delay(100);
                }
            }
            else
            {
                // Reopening from the tray or launching Chroma again while it is
                // already running must always perform a fresh check.
                await CheckForUpdatesAsync(showResult: false);
            }

            if (_availableUpdate is { IsUpdateAvailable: true } update)
            {
                await ShowUpdateAvailableDialogAsync(update);
            }
        }
        finally
        {
            _windowOpenCheckInProgress = false;
        }
    }

    private void UpdatesButtonText_PropertyChanged(
        DependencyObject sender,
        DependencyProperty dependencyProperty)
    {
        string text = UpdatesButtonText.Text ?? string.Empty;
        if (IsUpdateProgressStage(text))
        {
            ApplyUpdateProgressText(text);
            if (_updateProgressDialog is null)
            {
                _ = ShowUpdateProgressDialogAsync();
            }
            return;
        }

        if (text.StartsWith("Update ", StringComparison.Ordinal) ||
            string.Equals(text, "Updates", StringComparison.Ordinal) ||
            string.Equals(text, "Up to date", StringComparison.Ordinal))
        {
            _updateProgressDialog?.Hide();
        }
    }

    private async Task ShowUpdateProgressDialogAsync()
    {
        if (_updateProgressDialog is not null || Root.XamlRoot is null)
        {
            return;
        }

        var statusText = new TextBlock
        {
            Text = "Preparing update…",
            FontSize = 15,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap
        };
        var percentText = new TextBlock
        {
            Foreground = (Brush)Application.Current.Resources["TextMutedBrush"],
            HorizontalAlignment = HorizontalAlignment.Right
        };
        var progressHeader = new Grid();
        progressHeader.ColumnDefinitions.Add(new ColumnDefinition());
        progressHeader.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = GridLength.Auto
        });
        progressHeader.Children.Add(statusText);
        Grid.SetColumn(percentText, 1);
        progressHeader.Children.Add(percentText);

        var progressBar = new ProgressBar
        {
            Minimum = 0,
            Maximum = 100,
            IsIndeterminate = true,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        string currentVersion = UpdateService.CurrentVersionTag;
        string latestVersion = _availableUpdate?.LatestVersionTag ?? "the latest version";
        var detailText = new TextBlock
        {
            Text = $"{currentVersion}  →  {latestVersion}\n" +
                   "Chroma will verify the download before replacing any files.",
            Foreground = (Brush)Application.Current.Resources["TextMutedBrush"],
            TextWrapping = TextWrapping.Wrap
        };
        var content = new StackPanel
        {
            Spacing = 12,
            MinWidth = 430
        };
        content.Children.Add(progressHeader);
        content.Children.Add(progressBar);
        content.Children.Add(detailText);

        var dialog = new ContentDialog
        {
            XamlRoot = Root.XamlRoot,
            Title = $"Updating Chroma to {latestVersion}",
            Content = content
        };

        _updateProgressDialog = dialog;
        _updateProgressStatusText = statusText;
        _updateProgressPercentText = percentText;
        _updateProgressBar = progressBar;
        ApplyUpdateProgressText(UpdatesButtonText.Text ?? "Preparing update…");

        try
        {
            await dialog.ShowAsync();
        }
        catch
        {
            // The main window may close while the updater launches. In that
            // case the dialog is no longer needed and can be discarded.
        }
        finally
        {
            if (ReferenceEquals(_updateProgressDialog, dialog))
            {
                _updateProgressDialog = null;
                _updateProgressStatusText = null;
                _updateProgressPercentText = null;
                _updateProgressBar = null;
            }
        }
    }

    private void ApplyUpdateProgressText(string text)
    {
        if (_updateProgressStatusText is null ||
            _updateProgressPercentText is null ||
            _updateProgressBar is null)
        {
            return;
        }

        double? percentage = ParseTrailingPercentage(text);
        _updateProgressStatusText.Text = RemoveTrailingPercentage(text);

        if (percentage is double reportedPercentage)
        {
            double normalized = Math.Clamp(reportedPercentage, 0d, 100d);
            _updateProgressBar.IsIndeterminate = false;
            _updateProgressBar.Value = normalized;
            _updateProgressPercentText.Text = $"{normalized:0}%";
        }
        else
        {
            _updateProgressBar.IsIndeterminate = true;
            _updateProgressPercentText.Text = string.Empty;
        }
    }

    private static bool IsUpdateProgressStage(string text) =>
        text.StartsWith("Preparing update", StringComparison.Ordinal) ||
        text.StartsWith("Downloading update", StringComparison.Ordinal) ||
        text.StartsWith("Verifying SHA-256 checksum", StringComparison.Ordinal) ||
        text.StartsWith("Preparing update files", StringComparison.Ordinal) ||
        text.StartsWith("Ready to restart", StringComparison.Ordinal) ||
        text.StartsWith("Restarting", StringComparison.Ordinal);

    private static double? ParseTrailingPercentage(string text)
    {
        int percentIndex = text.LastIndexOf('%');
        if (percentIndex <= 0)
        {
            return null;
        }

        int numberStart = percentIndex - 1;
        while (numberStart >= 0 &&
               (char.IsDigit(text[numberStart]) || text[numberStart] == '.'))
        {
            numberStart--;
        }
        numberStart++;

        return double.TryParse(
            text[numberStart..percentIndex],
            NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out double percentage)
            ? percentage
            : null;
    }

    private static string RemoveTrailingPercentage(string text)
    {
        int percentIndex = text.LastIndexOf('%');
        if (percentIndex != text.Length - 1)
        {
            return text;
        }

        int numberStart = percentIndex - 1;
        while (numberStart >= 0 &&
               (char.IsDigit(text[numberStart]) || text[numberStart] == '.'))
        {
            numberStart--;
        }

        return text[..Math.Max(0, numberStart)].TrimEnd();
    }

    private void MainWindow_UpdateExperienceClosed(object sender, WindowEventArgs args)
    {
        if (_updatesButtonTextCallbackToken != 0)
        {
            UpdatesButtonText.UnregisterPropertyChangedCallback(
                TextBlock.TextProperty,
                _updatesButtonTextCallbackToken);
            _updatesButtonTextCallbackToken = 0;
        }

        _updateProgressDialog?.Hide();
    }
}
