using Chroma.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Chroma;

public sealed partial class MainWindow
{
    private bool _footerUtilityBarEnabled;
    private bool _footerUtilityBarApplied;
    private bool _updatesButtonVisualHooked;
    private bool _syncingUpdatesButtonVisualState;

    public void EnableFooterUtilityBar()
    {
        if (_footerUtilityBarEnabled)
        {
            return;
        }

        _footerUtilityBarEnabled = true;
        ProfilesPage.Loaded += ProfilesPage_LoadedForFooterUtilityBar;
        Root.ActualThemeChanged += Root_ActualThemeChangedForFooterUtilityBar;
        DispatcherQueue.TryEnqueue(ApplyFooterUtilityBar);
    }

    private void ProfilesPage_LoadedForFooterUtilityBar(object sender, RoutedEventArgs e)
    {
        ApplyFooterUtilityBar();
    }

    private void Root_ActualThemeChangedForFooterUtilityBar(FrameworkElement sender, object args)
    {
        if (_footerUtilityBarApplied)
        {
            DispatcherQueue.TryEnqueue(RefreshUpdatesButtonVisualState);
        }
    }

    private void ApplyFooterUtilityBar()
    {
        if (_footerUtilityBarApplied)
        {
            return;
        }

        Border? footer = ProfilesPage.Children
            .OfType<Border>()
            .FirstOrDefault(border => Grid.GetRow(border) == 1 && Grid.GetColumnSpan(border) == 2);
        if (footer is null)
        {
            return;
        }

        Button? websiteButton = EnumerateLayoutDescendants<Button>(footer)
            .FirstOrDefault(button => ContainsFooterText(button, "Website"));
        Button? githubButton = EnumerateLayoutDescendants<Button>(footer)
            .FirstOrDefault(button => ContainsFooterText(button, "GitHub"));

        if (websiteButton is null || githubButton is null)
        {
            return;
        }

        DetachFooterElement(websiteButton);
        DetachFooterElement(githubButton);
        DetachFooterElement(UpdatesButton);
        DetachFooterElement(FooterVersionText);
        DetachFooterElement(FooterBrandLogo);

        ConfigureFooterActionButton(websiteButton, 108);
        ConfigureFooterActionButton(githubButton, 108);
        ConfigureFooterActionButton(UpdatesButton, 116);
        UpdatesButtonText.Text = "Updates";

        FooterBrandLogo.Width = 38;
        FooterBrandLogo.Height = 38;
        FooterBrandLogo.Margin = new Thickness(0);
        FooterBrandLogo.HorizontalAlignment = HorizontalAlignment.Center;
        FooterBrandLogo.VerticalAlignment = VerticalAlignment.Center;

        var logoFrame = new Border
        {
            Width = 52,
            Height = 52,
            Padding = new Thickness(6),
            Background = (Brush)Application.Current.Resources["PanelRaisedBrush"],
            BorderBrush = (Brush)Application.Current.Resources["StrokeBrush"],
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(14),
            Child = FooterBrandLogo,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var brandTitle = new TextBlock
        {
            Text = "Chroma",
            FontSize = 21,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = (Brush)Application.Current.Resources["TextPrimaryBrush"],
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };

        FooterVersionText.Text = $"{UpdateService.CurrentVersionTag}  •  by acchtt";
        FooterVersionText.FontSize = 11;
        FooterVersionText.FontWeight = Microsoft.UI.Text.FontWeights.Normal;
        FooterVersionText.Foreground = (Brush)Application.Current.Resources["TextMutedBrush"];
        FooterVersionText.Margin = new Thickness(0);
        FooterVersionText.HorizontalAlignment = HorizontalAlignment.Left;
        FooterVersionText.VerticalAlignment = VerticalAlignment.Center;
        FooterVersionText.TextWrapping = TextWrapping.NoWrap;

        var brandText = new StackPanel
        {
            Spacing = 2,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        brandText.Children.Add(brandTitle);
        brandText.Children.Add(FooterVersionText);

        var brandBlock = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 13,
            MinWidth = 232,
            Height = 54,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        brandBlock.Children.Add(logoFrame);
        brandBlock.Children.Add(brandText);

        var safeIcon = new Border
        {
            Width = 26,
            Height = 26,
            Background = (Brush)Application.Current.Resources["PanelBrush"],
            BorderBrush = (Brush)Application.Current.Resources["StrokeBrush"],
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(13),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Child = new FontIcon
            {
                Glyph = "\uE83D",
                FontSize = 14,
                Foreground = (Brush)Application.Current.Resources["PositiveBrush"],
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };

        var safeContent = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 9,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        safeContent.Children.Add(safeIcon);
        safeContent.Children.Add(new TextBlock
        {
            Text = "Anti-cheat safe",
            FontSize = 13,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = (Brush)Application.Current.Resources["TextPrimaryBrush"],
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        });

        var safeBadge = new Border
        {
            Width = 158,
            Height = 42,
            Padding = new Thickness(12, 0, 12, 0),
            Background = (Brush)Application.Current.Resources["PanelRaisedBrush"],
            BorderBrush = (Brush)Application.Current.Resources["StrokeBrush"],
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Child = safeContent,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        ToolTipService.SetToolTip(
            safeBadge,
            "External GPU-control architecture. No game injection or game-memory access. Not an official anti-cheat certification.");

        var actionGroup = new Grid
        {
            ColumnSpacing = 8,
            Height = 42,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        actionGroup.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        actionGroup.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        actionGroup.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        Grid.SetColumn(websiteButton, 0);
        Grid.SetColumn(githubButton, 1);
        Grid.SetColumn(UpdatesButton, 2);
        actionGroup.Children.Add(websiteButton);
        actionGroup.Children.Add(githubButton);
        actionGroup.Children.Add(UpdatesButton);

        var commandBar = new Grid
        {
            ColumnSpacing = 18,
            Height = 54,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        commandBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        commandBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        commandBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        Grid.SetColumn(brandBlock, 0);
        Grid.SetColumn(safeBadge, 1);
        Grid.SetColumn(actionGroup, 2);
        commandBar.Children.Add(brandBlock);
        commandBar.Children.Add(safeBadge);
        commandBar.Children.Add(actionGroup);

        if (ProfilesPage.RowDefinitions.Count >= 2)
        {
            ProfilesPage.RowDefinitions[1].Height = new GridLength(96);
        }

        footer.Margin = new Thickness(0, 12, 0, 0);
        footer.Padding = new Thickness(16, 11, 16, 11);
        footer.MinHeight = 78;
        footer.CornerRadius = new CornerRadius(16);
        footer.BorderThickness = new Thickness(1);
        footer.BorderBrush = (Brush)Application.Current.Resources["FooterStrokeBrush"];
        footer.Background = (Brush)Application.Current.Resources["FooterBrush"];
        footer.VerticalAlignment = VerticalAlignment.Center;
        footer.Child = commandBar;

        if (!_updatesButtonVisualHooked)
        {
            UpdatesButtonText.RegisterPropertyChangedCallback(
                TextBlock.TextProperty,
                UpdatesButtonText_TextPropertyChanged);
            _updatesButtonVisualHooked = true;
        }

        _footerUtilityBarApplied = true;
        RefreshUpdatesButtonVisualState();
    }

    private void UpdatesButtonText_TextPropertyChanged(
        DependencyObject sender,
        DependencyProperty dependencyProperty)
    {
        RefreshUpdatesButtonVisualState();
    }

    private void RefreshUpdatesButtonVisualState()
    {
        if (_syncingUpdatesButtonVisualState)
        {
            return;
        }

        _syncingUpdatesButtonVisualState = true;
        try
        {
            bool updateAvailable = _availableUpdate is not null;
            string normalizedText = NormalizeUpdatesButtonText(UpdatesButtonText.Text);
            if (!string.Equals(UpdatesButtonText.Text, normalizedText, StringComparison.Ordinal))
            {
                UpdatesButtonText.Text = normalizedText;
            }

            UpdatesButton.Background = updateAvailable
                ? (Brush)Application.Current.Resources["PrimaryActionGradientBrush"]
                : (Brush)Application.Current.Resources["PanelRaisedBrush"];
            UpdatesButton.BorderBrush = updateAvailable
                ? (Brush)Application.Current.Resources["PrimaryActionGradientBrush"]
                : (Brush)Application.Current.Resources["StrokeBrush"];
            UpdatesButton.BorderThickness = new Thickness(1);

            Brush foreground = updateAvailable
                ? (Brush)Application.Current.Resources["PrimaryActionForegroundBrush"]
                : (Brush)Application.Current.Resources["TextPrimaryBrush"];
            Brush iconForeground = updateAvailable
                ? (Brush)Application.Current.Resources["PrimaryActionForegroundBrush"]
                : (Brush)Application.Current.Resources["CyanBrush"];

            foreach (TextBlock text in EnumerateLayoutDescendants<TextBlock>(UpdatesButton))
            {
                text.Foreground = foreground;
            }

            foreach (FontIcon icon in EnumerateLayoutDescendants<FontIcon>(UpdatesButton))
            {
                icon.Foreground = iconForeground;
            }

            ToolTipService.SetToolTip(
                UpdatesButton,
                updateAvailable
                    ? $"{_availableUpdate!.LatestVersionTag} is available. Click to view and install the update."
                    : "Check for Chroma updates.");
        }
        finally
        {
            _syncingUpdatesButtonVisualState = false;
        }
    }

    private static string NormalizeUpdatesButtonText(string? text)
    {
        string value = string.IsNullOrWhiteSpace(text) ? "Updates" : text.Trim();

        if (value.Equals("Up to date", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("Check updates", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("Update ", StringComparison.OrdinalIgnoreCase))
        {
            return "Updates";
        }

        int percentIndex = value.LastIndexOf('%');
        if (percentIndex > 0)
        {
            int digitStart = percentIndex - 1;
            while (digitStart >= 0 && char.IsDigit(value[digitStart]))
            {
                digitStart--;
            }

            if (digitStart < percentIndex - 1)
            {
                string percentage = value[(digitStart + 1)..(percentIndex + 1)];
                return $"Updating {percentage}";
            }
        }

        if (value.StartsWith("Checking", StringComparison.OrdinalIgnoreCase))
        {
            return "Checking…";
        }

        if (value.StartsWith("Preparing", StringComparison.OrdinalIgnoreCase))
        {
            return "Preparing…";
        }

        if (value.StartsWith("Downloading", StringComparison.OrdinalIgnoreCase))
        {
            return "Downloading…";
        }

        if (value.StartsWith("Verifying", StringComparison.OrdinalIgnoreCase))
        {
            return "Verifying…";
        }

        if (value.StartsWith("Extracting", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("Installing", StringComparison.OrdinalIgnoreCase))
        {
            return "Installing…";
        }

        return value;
    }

    private void ConfigureFooterActionButton(Button button, double width)
    {
        button.Width = width;
        button.Height = 42;
        button.MinHeight = 42;
        button.Margin = new Thickness(0);
        button.Padding = new Thickness(12, 0, 12, 0);
        button.Background = (Brush)Application.Current.Resources["PanelRaisedBrush"];
        button.BorderBrush = (Brush)Application.Current.Resources["StrokeBrush"];
        button.BorderThickness = new Thickness(1);
        button.HorizontalAlignment = HorizontalAlignment.Center;
        button.VerticalAlignment = VerticalAlignment.Center;
        button.HorizontalContentAlignment = HorizontalAlignment.Center;
        button.VerticalContentAlignment = VerticalAlignment.Center;
        button.CornerRadius = new CornerRadius(11);

        foreach (FontIcon icon in EnumerateLayoutDescendants<FontIcon>(button))
        {
            icon.FontSize = 16;
            icon.VerticalAlignment = VerticalAlignment.Center;
        }

        foreach (TextBlock text in EnumerateLayoutDescendants<TextBlock>(button))
        {
            text.FontSize = 13;
            text.VerticalAlignment = VerticalAlignment.Center;
        }
    }

    private static void DetachFooterElement(FrameworkElement element)
    {
        DependencyObject? parent = VisualTreeHelper.GetParent(element);
        switch (parent)
        {
            case Panel panel:
                panel.Children.Remove(element);
                break;
            case Border border when ReferenceEquals(border.Child, element):
                border.Child = null;
                break;
            case ContentControl contentControl when ReferenceEquals(contentControl.Content, element):
                contentControl.Content = null;
                break;
        }
    }

    private static bool ContainsFooterText(DependencyObject root, string text) =>
        FindLayoutDescendant<TextBlock>(root,
            block => string.Equals(block.Text, text, StringComparison.Ordinal)) is not null;
}
