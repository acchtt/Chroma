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
            DispatcherQueue.TryEnqueue(RefreshProfileEditorDisclosure);

        if (_customResolutionToggle is not null)
        {
            _customResolutionToggle.Toggled += (_, _) =>
                DispatcherQueue.TryEnqueue(RefreshProfileEditorDisclosure);
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

        RefreshProfileEditorDisclosure();
    }

    private void ApplyProfileEditorComfort()
    {
        if (ProfilesPage.ColumnDefinitions.Count >= 2)
        {
            ProfilesPage.ColumnDefinitions[1].Width = new GridLength(450);
        }

        Grid? profilePane = ProfilesPage.Children
            .OfType<Grid>()
            .FirstOrDefault(grid => Grid.GetColumn(grid) == 0 && Grid.GetRow(grid) == 0);
        if (profilePane is not null)
        {
            profilePane.Margin = new Thickness(0, 0, 18, 0);
        }

        Border? editorCard = ProfilesPage.Children
            .OfType<Border>()
            .FirstOrDefault(border => Grid.GetColumn(border) == 1 && Grid.GetRow(border) == 0);
        if (editorCard is null)
        {
            return;
        }

        editorCard.Padding = new Thickness(22, 18, 22, 16);
        editorCard.CornerRadius = new CornerRadius(16);

        StackPanel? editorBody = EnumerateLayoutDescendants<StackPanel>(editorCard)
            .FirstOrDefault(panel =>
                Grid.GetRow(panel) == 1 &&
                panel.Children.Count >= 5 &&
                ContainsLayoutText(panel, "Saturation"));
        if (editorBody is not null)
        {
            editorBody.Spacing = 12;
            CompactSelectedGameHeader(editorBody);
            BuildCompactSaturationCard(editorBody);
        }

        TextBlock? editorTitle = EnumerateLayoutDescendants<TextBlock>(editorCard)
            .FirstOrDefault(block => block.Text == "Edit Profile");
        if (editorTitle is not null)
        {
            editorTitle.FontSize = 21;
            editorTitle.Margin = new Thickness(0);
        }

        Border? resolutionCard = FindDeepestLayoutDescendant<Border>(editorCard, border =>
            border.Child is StackPanel && ContainsLayoutText(border.Child, "Custom resolution"));
        if (resolutionCard is not null)
        {
            resolutionCard.Padding = new Thickness(16, 14, 16, 14);
            resolutionCard.Margin = new Thickness(0, 2, 0, 0);
            resolutionCard.CornerRadius = new CornerRadius(14);

            if (resolutionCard.Child is StackPanel resolutionContent)
            {
                resolutionContent.Spacing = 10;
            }

            TextBlock? resolutionHeading = EnumerateLayoutDescendants<TextBlock>(resolutionCard)
                .FirstOrDefault(block => block.Text == "Custom resolution");
            if (resolutionHeading is not null)
            {
                resolutionHeading.FontSize = 15.5;
            }

            foreach (ComboBox comboBox in EnumerateLayoutDescendants<ComboBox>(resolutionCard))
            {
                comboBox.Height = 44;
                comboBox.MinHeight = 44;
                comboBox.MaxDropDownHeight = 320;
            }

            if (_resolutionStatusText is not null)
            {
                _resolutionStatusText.Margin = new Thickness(0, 1, 0, 0);
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
            actionRow.ColumnSpacing = 14;
            actionRow.Margin = new Thickness(0, 10, 0, 0);

            foreach (Button button in EnumerateLayoutDescendants<Button>(actionRow))
            {
                button.Height = 46;
            }
        }
    }

    private void CompactSelectedGameHeader(StackPanel editorBody)
    {
        Grid? gameHeader = editorBody.Children
            .OfType<Grid>()
            .FirstOrDefault(grid =>
                grid.ColumnDefinitions.Count == 2 &&
                grid.Children.OfType<Border>().Any(border => Grid.GetColumn(border) == 0) &&
                grid.Children.OfType<StackPanel>().Any(panel => Grid.GetColumn(panel) == 1));
        if (gameHeader is null)
        {
            return;
        }

        gameHeader.ColumnSpacing = 14;
        gameHeader.Margin = new Thickness(0, 0, 0, 2);
        gameHeader.ColumnDefinitions[0].Width = new GridLength(62);

        Border? iconBorder = gameHeader.Children
            .OfType<Border>()
            .FirstOrDefault(border => Grid.GetColumn(border) == 0);
        if (iconBorder is not null)
        {
            iconBorder.Width = 62;
            iconBorder.Height = 62;
            iconBorder.CornerRadius = new CornerRadius(11);

            Image? image = FindLayoutDescendant<Image>(iconBorder, _ => true);
            if (image is not null)
            {
                image.Width = 54;
                image.Height = 54;
            }
        }

        StackPanel? details = gameHeader.Children
            .OfType<StackPanel>()
            .FirstOrDefault(panel => Grid.GetColumn(panel) == 1);
        if (details is null)
        {
            return;
        }

        details.Spacing = 3;
        TextBlock[] lines = details.Children.OfType<TextBlock>().ToArray();
        if (lines.Length > 0)
        {
            lines[0].FontSize = 18;
        }
        if (lines.Length > 1)
        {
            lines[1].TextWrapping = TextWrapping.NoWrap;
            lines[1].TextTrimming = TextTrimming.CharacterEllipsis;
            lines[1].MaxLines = 1;
            lines[1].FontSize = 11;
        }
        if (lines.Length > 2)
        {
            lines[2].FontSize = 10.5;
        }
    }

    private void BuildCompactSaturationCard(StackPanel editorBody)
    {
        TextBlock? heading = editorBody.Children
            .OfType<TextBlock>()
            .FirstOrDefault(block => block.Text == "Saturation");
        Grid? range = editorBody.Children
            .OfType<Grid>()
            .FirstOrDefault(grid =>
                grid.Children.OfType<TextBlock>().Any(block => block.Text == "0%") &&
                grid.Children.OfType<TextBlock>().Any(block => block.Text == "300%"));
        Grid? numericEditor = editorBody.Children
            .OfType<Grid>()
            .FirstOrDefault(grid =>
                FindLayoutDescendant<TextBox>(grid, box => ReferenceEquals(box, SaturationTextBox)) is not null);

        if (heading is null || range is null || numericEditor is null ||
            !editorBody.Children.Contains(SaturationSlider))
        {
            return;
        }

        int insertionIndex = editorBody.Children.IndexOf(heading);
        editorBody.Children.Remove(heading);
        editorBody.Children.Remove(range);
        editorBody.Children.Remove(SaturationSlider);
        editorBody.Children.Remove(numericEditor);

        heading.FontSize = 16;
        heading.Margin = new Thickness(0);
        heading.VerticalAlignment = VerticalAlignment.Center;

        numericEditor.Width = 178;
        numericEditor.Height = 48;
        numericEditor.Margin = new Thickness(0);
        numericEditor.HorizontalAlignment = HorizontalAlignment.Right;

        var cardHeader = new Grid { ColumnSpacing = 14 };
        cardHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        cardHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        Grid.SetColumn(numericEditor, 1);
        cardHeader.Children.Add(heading);
        cardHeader.Children.Add(numericEditor);

        range.Margin = new Thickness(0, 2, 0, -2);
        SaturationSlider.Margin = new Thickness(0, -2, 0, 0);

        var content = new StackPanel { Spacing = 7 };
        content.Children.Add(cardHeader);
        content.Children.Add(range);
        content.Children.Add(SaturationSlider);

        var card = new Border
        {
            Background = (Brush)Application.Current.Resources["PanelRaisedBrush"],
            BorderBrush = (Brush)Application.Current.Resources["StrokeBrush"],
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(16, 12, 16, 12),
            Child = content
        };

        editorBody.Children.Insert(Math.Min(insertionIndex, editorBody.Children.Count), card);
    }

    private void RefreshProfileEditorDisclosure()
    {
        bool expanded = _customResolutionToggle?.IsEnabled == true &&
            _customResolutionToggle.IsOn;

        if (_resolutionDetails is not null)
        {
            _resolutionDetails.Visibility = expanded
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        RefreshCompactResolutionStatus();
    }

    private void RefreshCompactResolutionStatus()
    {
        if (_resolutionStatusText is null || _customResolutionToggle is null)
        {
            return;
        }

        if (!_customResolutionToggle.IsEnabled || !_customResolutionToggle.IsOn)
        {
            _resolutionStatusText.Text = string.Empty;
            return;
        }

        if (TryParseResolutionFields(out int width, out int height))
        {
            _resolutionStatusText.Text =
                $"{width} × {height} while active  •  restores on exit";
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
