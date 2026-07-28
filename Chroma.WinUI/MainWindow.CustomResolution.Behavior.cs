using System.Collections.Specialized;
using Chroma.Services;
using Chroma.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Chroma;

public sealed partial class MainWindow
{
    private void LoadResolutionOverrides()
    {
        _resolutionOverrides.Clear();
        try
        {
            foreach ((string path, ResolutionOverride resolution) in _resolutionOverrideStore.Load())
            {
                _resolutionOverrides[path] = resolution;
            }
        }
        catch (Exception exception)
        {
            _viewModel.Notification = $"Could not load custom resolutions: {exception.Message}";
        }
    }

    private void CustomResolutionToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (!_syncingResolutionEditor)
        {
            UpdateResolutionEditorState();
        }
    }

    private void ResolutionComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (_syncingResolutionEditor)
        {
            return;
        }

        if (ReferenceEquals(sender, _resolutionWidthComboBox) &&
            TryGetSelectedResolutionValue(_resolutionWidthComboBox, out int selectedWidth))
        {
            int preferredHeight = TryGetSelectedResolutionValue(
                _resolutionHeightComboBox,
                out int selectedHeight)
                ? selectedHeight
                : 0;

            _syncingResolutionEditor = true;
            try
            {
                PopulateResolutionHeights(selectedWidth, preferredHeight);
            }
            finally
            {
                _syncingResolutionEditor = false;
            }
        }

        UpdateResolutionEditorState();
    }

    private async void SaveEditorWithResolution_Click(object sender, RoutedEventArgs e)
    {
        ProfileViewModel? profile = _viewModel.SelectedProfile;
        if (profile is null)
        {
            return;
        }

        if (!TryReadResolutionEditor(out ResolutionOverride? resolution))
        {
            _viewModel.Notification = "Choose a valid width and height from the available display modes.";
            UpdateResolutionEditorState();
            return;
        }

        if (resolution is ResolutionOverride value)
        {
            _resolutionOverrides[profile.ExecutablePath] = value;
        }
        else
        {
            _resolutionOverrides.Remove(profile.ExecutablePath);
        }

        try
        {
            await _resolutionOverrideStore.SaveAsync(_resolutionOverrides);
            CommitSaturationText();
            await _viewModel.SaveEditorAsync();
            SyncSaturationEditor();
            SyncCustomResolutionEditor();
        }
        catch (Exception exception)
        {
            _viewModel.Notification = $"Could not save the custom resolution: {exception.Message}";
        }
    }

    private void CancelEditorWithResolution_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.CancelEditor();
        SyncSaturationEditor();
        SyncCustomResolutionEditor();
    }

    private void SyncCustomResolutionEditor()
    {
        if (_customResolutionToggle is null ||
            _resolutionWidthComboBox is null ||
            _resolutionHeightComboBox is null)
        {
            return;
        }

        _syncingResolutionEditor = true;
        try
        {
            ProfileViewModel? profile = _viewModel.SelectedProfile;
            ResolutionOverride resolution = default;
            bool hasOverride = profile is not null &&
                _resolutionOverrides.TryGetValue(profile.ExecutablePath, out resolution);

            DisplayModeSnapshot snapshot = DisplayModeCatalog.GetSnapshot();
            int preferredWidth = hasOverride
                ? resolution.Width
                : snapshot.Preferred.Width;
            int preferredHeight = hasOverride
                ? resolution.Height
                : snapshot.Preferred.Height;

            PopulateResolutionSelectors(
                snapshot,
                preferredWidth,
                preferredHeight);

            _customResolutionToggle.IsEnabled = profile is not null;
            _customResolutionToggle.IsOn = hasOverride;
        }
        finally
        {
            _syncingResolutionEditor = false;
        }

        UpdateResolutionEditorState();
    }

    private void PopulateResolutionSelectors(
        DisplayModeSnapshot snapshot,
        int preferredWidth,
        int preferredHeight)
    {
        var modes = snapshot.Modes.ToList();
        var preferredMode = new DisplayResolution(preferredWidth, preferredHeight);
        if (ResolutionOverrideStore.IsValid(preferredWidth, preferredHeight) &&
            !modes.Contains(preferredMode))
        {
            // Preserve an existing profile value when its monitor is currently
            // disconnected. The native agent will validate it again at launch.
            modes.Add(preferredMode);
        }

        _supportedDisplayResolutions = modes
            .Distinct()
            .OrderByDescending(mode => mode.Width)
            .ThenByDescending(mode => mode.Height)
            .ToArray();

        int[] widths = _supportedDisplayResolutions
            .Select(mode => mode.Width)
            .Distinct()
            .OrderByDescending(width => width)
            .ToArray();

        int selectedWidth = widths.Contains(preferredWidth)
            ? preferredWidth
            : widths.Contains(snapshot.Preferred.Width)
                ? snapshot.Preferred.Width
                : widths[0];

        SetResolutionComboItems(
            _resolutionWidthComboBox,
            widths,
            selectedWidth);
        PopulateResolutionHeights(selectedWidth, preferredHeight);
    }

    private void PopulateResolutionHeights(
        int selectedWidth,
        int preferredHeight)
    {
        int[] heights = _supportedDisplayResolutions
            .Where(mode => mode.Width == selectedWidth)
            .Select(mode => mode.Height)
            .Distinct()
            .OrderByDescending(height => height)
            .ToArray();

        if (heights.Length == 0)
        {
            _resolutionHeightComboBox?.Items.Clear();
            return;
        }

        int selectedHeight = heights.Contains(preferredHeight)
            ? preferredHeight
            : preferredHeight > 0
                ? heights.OrderBy(height => Math.Abs(height - preferredHeight)).First()
                : heights[0];

        SetResolutionComboItems(
            _resolutionHeightComboBox,
            heights,
            selectedHeight);
    }

    private static void SetResolutionComboItems(
        ComboBox? comboBox,
        IEnumerable<int> values,
        int selectedValue)
    {
        if (comboBox is null)
        {
            return;
        }

        comboBox.Items.Clear();
        ComboBoxItem? selectedItem = null;
        foreach (int value in values)
        {
            var item = new ComboBoxItem
            {
                Content = value.ToString(),
                Tag = value,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            comboBox.Items.Add(item);
            if (value == selectedValue)
            {
                selectedItem = item;
            }
        }

        comboBox.SelectedItem = selectedItem ?? comboBox.Items.FirstOrDefault();
    }

    private void UpdateResolutionEditorState()
    {
        if (_customResolutionToggle is null ||
            _resolutionWidthComboBox is null ||
            _resolutionHeightComboBox is null ||
            _resolutionFields is null ||
            _resolutionStatusText is null)
        {
            return;
        }

        bool enabled = _customResolutionToggle.IsEnabled && _customResolutionToggle.IsOn;
        _resolutionWidthComboBox.IsEnabled = enabled;
        _resolutionHeightComboBox.IsEnabled = enabled;
        _resolutionFields.IsHitTestVisible = enabled;
        _resolutionFields.Opacity = enabled ? 1 : 0.52;

        if (!enabled)
        {
            _resolutionStatusText.Text = "Desktop resolution stays unchanged for this profile.";
            _resolutionStatusText.Foreground =
                (Brush)Application.Current.Resources["TextMutedBrush"];
            return;
        }

        if (TryParseResolutionFields(out int width, out int height))
        {
            _resolutionStatusText.Text =
                $"Will request {width} × {height} while the game is active and restore the previous mode afterward.";
            _resolutionStatusText.Foreground =
                (Brush)Application.Current.Resources["PositiveBrush"];
        }
        else
        {
            _resolutionStatusText.Text =
                "Choose a width and height from the available display-mode lists.";
            _resolutionStatusText.Foreground =
                (Brush)Application.Current.Resources["TextSecondaryBrush"];
        }
    }

    private bool TryReadResolutionEditor(out ResolutionOverride? resolution)
    {
        resolution = null;
        if (_customResolutionToggle?.IsOn != true)
        {
            return true;
        }

        if (!TryParseResolutionFields(out int width, out int height))
        {
            return false;
        }

        resolution = new ResolutionOverride(width, height);
        return true;
    }

    private bool TryParseResolutionFields(out int width, out int height)
    {
        bool validWidth = TryGetSelectedResolutionValue(
            _resolutionWidthComboBox,
            out width);
        bool validHeight = TryGetSelectedResolutionValue(
            _resolutionHeightComboBox,
            out height);
        return validWidth && validHeight &&
            ResolutionOverrideStore.IsValid(width, height);
    }

    private static bool TryGetSelectedResolutionValue(
        ComboBox? comboBox,
        out int value)
    {
        if (comboBox?.SelectedItem is ComboBoxItem { Tag: int selectedValue })
        {
            value = selectedValue;
            return true;
        }

        value = 0;
        return false;
    }

    private void Profiles_CollectionChangedForResolution(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var existingPaths = _viewModel.Profiles
            .Select(profile => profile.ExecutablePath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        bool removed = false;

        foreach (string path in _resolutionOverrides.Keys.ToArray())
        {
            if (!existingPaths.Contains(path))
            {
                _resolutionOverrides.Remove(path);
                removed = true;
            }
        }

        if (removed)
        {
            _ = SaveResolutionOverridesAfterCleanupAsync();
        }
    }

    private async Task SaveResolutionOverridesAfterCleanupAsync()
    {
        try
        {
            await _resolutionOverrideStore.SaveAsync(_resolutionOverrides);
        }
        catch (Exception exception)
        {
            _viewModel.Notification = $"Could not clean up custom resolutions: {exception.Message}";
        }
    }
}
