using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Chroma;

public sealed partial class MainWindow
{
    private bool _profileEditorComfortEnabled;

    public void EnableProfileEditorComfort()
    {
        if (_profileEditorComfortEnabled)
        {
            return;
        }

        _profileEditorComfortEnabled = true;
        ApplyProfileEditorComfort();

        ProfilesList.SelectionChanged += (_, _) =>
            DispatcherQueue.TryEnqueue(RefreshCompactResolutionStatus);

        if (_customResolutionToggle is not null)
        {
            _customResolutionToggle.Toggled += (_, _) =>
                DispatcherQueue.TryEnqueue(RefreshCompactResolutionStatus);
        }

        if (_resolutionWidthComboBox is not null)
        {
            _resolutionWidthComboBox.SelectionChanged += (_, _) =>
                DispatcherQueue.TryEnqueue(RefreshCompactResolutionStatus);
        }

        if (_resolutionHeightComboBox is not null)
        {
            _resolutionHeightComboBox.SelectionChanged += (_, _) =>
                DispatcherQueue.TryEnqueue(RefreshCompactResolutionStatus);
        }

        RefreshCompactResolutionStatus();
    }

    private void ApplyProfileEditorComfort()
    {
        if (ProfilesPage.ColumnDefinitions.Count >= 2)
        {
            ProfilesPage.ColumnDefinitions[1].Width = new GridLength(480);
        }

        Grid? profilePane = ProfilesPage.Children
            .OfType<Grid>()
            .FirstOrDefault(grid => Grid.GetColumn(grid) == 0 && Grid.GetRow(grid) == 0);
        if (profilePane is not null)
        {
            profilePane.Margin = new Thickness(0, 0, 22, 0);
        }

        Border? editorCard = ProfilesPage.Children
            .OfType<Border>()
            .FirstOrDefault(border => Grid.GetColumn(border) == 1 && Grid.GetRow(border) == 0);
        if (editorCard is null)
        {
            return;
        }

        editorCard.Padding = new Thickness(26, 22, 26, 18);
        editorCard.CornerRadius = new CornerRadius(17);

        StackPanel? editorBody = EnumerateLayoutDescendants<StackPanel>(editorCard)
            .FirstOrDefault(panel =>
                Grid.GetRow(panel) == 1 &&
                panel.Children.Count >= 5 &&
                ContainsLayoutText(panel, "Saturation"));
        if (editorBody is not null)
        {
            editorBody.Spacing = 18;
        }

        TextBlock? editorTitle = EnumerateLayoutDescendants<TextBlock>(editorCard)
            .FirstOrDefault(block => block.Text == "Edit Profile");
        if (editorTitle is not null)
        {
            editorTitle.FontSize = 22;
            editorTitle.Margin = new Thickness(0, 0, 0, 2);
        }

        TextBlock? saturationHeading = EnumerateLayoutDescendants<TextBlock>(editorCard)
            .FirstOrDefault(block => block.Text == "Saturation");
        if (saturationHeading is not null)
        {
            saturationHeading.FontSize = 17;
            saturationHeading.Margin = new Thickness(0, 2, 0, 0);
        }

        Grid? saturationEditor = EnumerateLayoutDescendants<Grid>(editorCard)
            .FirstOrDefault(grid =>
                Math.Abs(grid.Width - 232) < 0.1 &&
                Math.Abs(grid.Height - 56) < 0.1);
        if (saturationEditor is not null)
        {
            saturationEditor.Width = 252;
            saturationEditor.Height = 58;
            saturationEditor.Margin = new Thickness(0, 8, 0, 0);
        }

        Border? resolutionCard = FindDeepestLayoutDescendant<Border>(editorCard, border =>
            border.Child is StackPanel && ContainsLayoutText(border.Child, "Custom resolution"));
        if (resolutionCard is not null)
        {
            resolutionCard.Padding = new Thickness(18, 16, 18, 16);
            resolutionCard.Margin = new Thickness(0, 8, 0, 0);
            resolutionCard.CornerRadius = new CornerRadius(15);

            if (resolutionCard.Child is StackPanel resolutionContent)
            {
                resolutionContent.Spacing = 14;
            }

            TextBlock? resolutionHeading = EnumerateLayoutDescendants<TextBlock>(resolutionCard)
                .FirstOrDefault(block => block.Text == "Custom resolution");
            if (resolutionHeading is not null)
            {
                resolutionHeading.FontSize = 16;
            }

            TextBlock? description = EnumerateLayoutDescendants<TextBlock>(resolutionCard)
                .FirstOrDefault(block =>
                    block.Text.StartsWith("Switch the game display", StringComparison.Ordinal));
            if (description is not null)
            {
                description.Text = "Use a supported display mode while this game is active.";
                description.FontSize = 11.5;
            }

            TextBlock? modeNote = EnumerateLayoutDescendants<TextBlock>(resolutionCard)
                .FirstOrDefault(block =>
                    block.Text.StartsWith("Dropdowns use modes", StringComparison.Ordinal));
            if (modeNote is not null)
            {
                modeNote.Text = "Available modes at the current refresh rate.";
                modeNote.FontSize = 11.5;
            }

            Grid? fieldsGrid = EnumerateLayoutDescendants<Grid>(resolutionCard)
                .FirstOrDefault(grid =>
                    EnumerateLayoutDescendants<ComboBox>(grid).Count() == 2);
            if (fieldsGrid is not null)
            {
                fieldsGrid.ColumnSpacing = 14;
            }

            foreach (ComboBox comboBox in EnumerateLayoutDescendants<ComboBox>(resolutionCard))
            {
                comboBox.Height = 46;
                comboBox.MinHeight = 46;
                comboBox.MaxDropDownHeight = 340;
            }

            if (_resolutionStatusText is not null)
            {
                _resolutionStatusText.Margin = new Thickness(0, 2, 0, 0);
                _resolutionStatusText.FontSize = 11.5;
            }
        }

        Grid? actionRow = EnumerateLayoutDescendants<Grid>(editorCard)
            .FirstOrDefault(grid =>
                Grid.GetRow(grid) == 2 &&
                EnumerateLayoutDescendants<Button>(grid)
                    .Any(button => button.Content is string text && text == "Save Changes"));
        if (actionRow is not null)
        {
            actionRow.ColumnSpacing = 16;
            actionRow.Margin = new Thickness(0, 16, 0, 0);
        }
    }

    private void RefreshCompactResolutionStatus()
    {
        if (_resolutionStatusText is null || _customResolutionToggle is null)
        {
            return;
        }

        if (!_customResolutionToggle.IsEnabled || !_customResolutionToggle.IsOn)
        {
            _resolutionStatusText.Text = "Desktop mode unchanged.";
            _resolutionStatusText.Foreground =
                (Brush)Application.Current.Resources["TextMutedBrush"];
            return;
        }

        if (TryParseResolutionFields(out int width, out int height))
        {
            _resolutionStatusText.Text =
                $"{width} × {height} while active  •  desktop restored on exit";
            _resolutionStatusText.Foreground =
                (Brush)Application.Current.Resources["PositiveBrush"];
        }
        else
        {
            _resolutionStatusText.Text = "Select an available display mode.";
            _resolutionStatusText.Foreground =
                (Brush)Application.Current.Resources["TextSecondaryBrush"];
        }
    }
}
