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

    private void ResolutionTextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        args.Cancel = !string.IsNullOrEmpty(args.NewText) &&
            (args.NewText.Length > 5 || !args.NewText.All(char.IsDigit));
    }

    private void ResolutionTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_syncingResolutionEditor)
        {
            UpdateResolutionEditorState();
        }
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
            _viewModel.Notification =
                $"Resolution must be {ResolutionOverrideStore.MinimumWidth}–{ResolutionOverrideStore.MaximumWidth} pixels wide and " +
                $"{ResolutionOverrideStore.MinimumHeight}–{ResolutionOverrideStore.MaximumHeight} pixels high.";
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
            _resolutionWidthTextBox is null ||
            _resolutionHeightTextBox is null)
        {
            return;
        }

        _syncingResolutionEditor = true;
        try
        {
            ProfileViewModel? profile = _viewModel.SelectedProfile;
            bool hasOverride = profile is not null &&
                _resolutionOverrides.TryGetValue(profile.ExecutablePath, out ResolutionOverride resolution);

            _customResolutionToggle.IsEnabled = profile is not null;
            _customResolutionToggle.IsOn = hasOverride;
            _resolutionWidthTextBox.Text = hasOverride ? resolution.Width.ToString() : "1920";
            _resolutionHeightTextBox.Text = hasOverride ? resolution.Height.ToString() : "1080";
        }
        finally
        {
            _syncingResolutionEditor = false;
        }

        UpdateResolutionEditorState();
    }

    private void UpdateResolutionEditorState()
    {
        if (_customResolutionToggle is null ||
            _resolutionFields is null ||
            _resolutionStatusText is null)
        {
            return;
        }

        bool enabled = _customResolutionToggle.IsEnabled && _customResolutionToggle.IsOn;
        _resolutionFields.IsEnabled = enabled;
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
                $"Enter {ResolutionOverrideStore.MinimumWidth}–{ResolutionOverrideStore.MaximumWidth} for width and " +
                $"{ResolutionOverrideStore.MinimumHeight}–{ResolutionOverrideStore.MaximumHeight} for height.";
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
        bool validWidth = int.TryParse(_resolutionWidthTextBox?.Text, out width);
        bool validHeight = int.TryParse(_resolutionHeightTextBox?.Text, out height);
        return validWidth && validHeight && ResolutionOverrideStore.IsValid(width, height);
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
