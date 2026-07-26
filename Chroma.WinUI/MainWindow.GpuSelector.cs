using Chroma.Models;
using Chroma.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Chroma;

public sealed partial class MainWindow
{
    private readonly GpuSelectionService _gpuSelectionService = new();
    private ComboBox? _gpuSelectorCombo;
    private TextBlock? _gpuSelectorStatus;
    private bool _gpuSelectorSyncing;

    public void EnableGpuSelector()
    {
        if (_gpuSelectorCombo is not null || SettingsPage.Content is not StackPanel settingsStack)
        {
            return;
        }

        var title = new TextBlock
        {
            Text = "Graphics processor",
            FontSize = 17,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
        };
        var description = new TextBlock
        {
            Text = "Choose which GPU backend Chroma controls. Automatic follows the primary Windows display.",
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextSecondaryBrush"],
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 520
        };
        _gpuSelectorStatus = new TextBlock
        {
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["CyanBrush"],
            FontSize = 12.5,
            Margin = new Thickness(0, 3, 0, 0),
            TextWrapping = TextWrapping.Wrap
        };

        var copy = new StackPanel { Spacing = 5 };
        copy.Children.Add(title);
        copy.Children.Add(description);
        copy.Children.Add(_gpuSelectorStatus);

        _gpuSelectorCombo = new ComboBox
        {
            Width = 340,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };

        IReadOnlyList<GpuSelectionOption> options = _gpuSelectionService.GetOptions();
        GpuPreference savedPreference = _gpuSelectionService.GetPreference();
        ComboBoxItem? selectedItem = null;
        foreach (GpuSelectionOption option in options)
        {
            var item = new ComboBoxItem
            {
                Content = option.DisplayName,
                Tag = option,
                ToolTip = option.Detail
            };
            _gpuSelectorCombo.Items.Add(item);
            if (option.Preference == savedPreference)
            {
                selectedItem = item;
            }
        }

        _gpuSelectorSyncing = true;
        _gpuSelectorCombo.SelectedItem = selectedItem ?? _gpuSelectorCombo.Items.FirstOrDefault();
        _gpuSelectorSyncing = false;
        _gpuSelectorCombo.SelectionChanged += GpuSelector_SelectionChanged;
        UpdateGpuSelectorStatus(savedPreference, initialized: null);

        var grid = new Grid { ColumnSpacing = 22 };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(340) });
        grid.Children.Add(copy);
        Grid.SetColumn(_gpuSelectorCombo, 1);
        grid.Children.Add(_gpuSelectorCombo);

        var card = new Border
        {
            Style = (Style)Application.Current.Resources["CardBorderStyle"],
            Padding = new Thickness(22),
            Child = grid
        };

        // Keep the background-service card last so the selector appears alongside
        // the other persistent application settings.
        int insertionIndex = Math.Max(1, settingsStack.Children.Count - 1);
        settingsStack.Children.Insert(insertionIndex, card);
    }

    private async void GpuSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_gpuSelectorSyncing ||
            _gpuSelectorCombo?.SelectedItem is not ComboBoxItem { Tag: GpuSelectionOption option })
        {
            return;
        }

        GpuPreference previousPreference = _gpuSelectionService.GetPreference();
        if (option.Preference == previousPreference)
        {
            return;
        }

        _gpuSelectorCombo.IsEnabled = false;
        if (_gpuSelectorStatus is not null)
        {
            _gpuSelectorStatus.Text = $"Switching to {option.DisplayName}…";
        }

        try
        {
            // Persist before restarting because the native factory reads this value
            // when the new agent process creates its GPU backend.
            _gpuSelectionService.SetPreference(option.Preference);
            await AgentRestartService.ShutdownAsync();
            bool agentRunning = await _viewModel.EnsureAgentRunningAsync();
            await _viewModel.RefreshAgentStatusAsync();

            bool initialized = agentRunning && _viewModel.AgentStatus.RuntimeInitialized;
            UpdateGpuSelectorStatus(option.Preference, initialized);
            _viewModel.Notification = initialized
                ? $"GPU selection changed to {option.DisplayName}."
                : $"{option.DisplayName} is selected. Chroma is waiting for that GPU backend to become available.";
        }
        catch (Exception exception)
        {
            _gpuSelectionService.SetPreference(previousPreference);
            _gpuSelectorSyncing = true;
            SelectGpuPreference(previousPreference);
            _gpuSelectorSyncing = false;
            UpdateGpuSelectorStatus(previousPreference, initialized: false);
            _viewModel.Notification = $"Could not change the GPU selection: {exception.Message}";
        }
        finally
        {
            _gpuSelectorCombo.IsEnabled = true;
        }
    }

    private void SelectGpuPreference(GpuPreference preference)
    {
        if (_gpuSelectorCombo is null)
        {
            return;
        }

        foreach (object item in _gpuSelectorCombo.Items)
        {
            if (item is ComboBoxItem { Tag: GpuSelectionOption option } comboItem &&
                option.Preference == preference)
            {
                _gpuSelectorCombo.SelectedItem = comboItem;
                return;
            }
        }
    }

    private void UpdateGpuSelectorStatus(GpuPreference preference, bool? initialized)
    {
        if (_gpuSelectorStatus is null)
        {
            return;
        }

        string label = preference switch
        {
            GpuPreference.Intel => "Intel",
            GpuPreference.Nvidia => "NVIDIA",
            GpuPreference.Amd => "AMD",
            _ => "Automatic"
        };

        _gpuSelectorStatus.Text = initialized switch
        {
            true => $"{label} GPU backend active.",
            false => $"{label} selected; backend initialization is pending.",
            null => $"Current selection: {label}."
        };
    }
}
