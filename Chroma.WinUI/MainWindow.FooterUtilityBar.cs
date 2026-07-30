using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Chroma;

public sealed partial class MainWindow
{
    private bool _footerUtilityBarEnabled;
    private bool _footerUtilityBarApplied;

    public void EnableFooterUtilityBar()
    {
        if (_footerUtilityBarEnabled)
        {
            return;
        }

        _footerUtilityBarEnabled = true;
        ProfilesPage.Loaded += ProfilesPage_LoadedForFooterUtilityBar;
        DispatcherQueue.TryEnqueue(ApplyFooterUtilityBar);
    }

    private void ProfilesPage_LoadedForFooterUtilityBar(object sender, RoutedEventArgs e)
    {
        ApplyFooterUtilityBar();
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

        ConfigureFooterActionButton(websiteButton, 112);
        ConfigureFooterActionButton(githubButton, 112);
        ConfigureFooterActionButton(UpdatesButton, 112);
        UpdatesButtonText.Text = "Updates";

        FooterBrandLogo.Width = 44;
        FooterBrandLogo.Height = 44;
        FooterBrandLogo.Margin = new Thickness(0);
        FooterBrandLogo.HorizontalAlignment = HorizontalAlignment.Center;
        FooterBrandLogo.VerticalAlignment = VerticalAlignment.Center;

        var logoGlow = new Border
        {
            Width = 62,
            Height = 62,
            Background = (Brush)Application.Current.Resources["NeonGradientBrush"],
            CornerRadius = new CornerRadius(16),
            Opacity = 0.07,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var logoFrame = new Border
        {
            Width = 60,
            Height = 60,
            Padding = new Thickness(6),
            Background = (Brush)Application.Current.Resources["PanelRaisedBrush"],
            BorderBrush = (Brush)Application.Current.Resources["StrokeBrush"],
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(15),
            Child = FooterBrandLogo,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var logoStage = new Grid
        {
            Width = 62,
            Height = 62,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        logoStage.Children.Add(logoGlow);
        logoStage.Children.Add(logoFrame);

        var brandTitle = new TextBlock
        {
            Text = "Chroma",
            FontSize = 22,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = (Brush)Application.Current.Resources["TextPrimaryBrush"],
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };

        var titleAccent = new Border
        {
            Width = 72,
            Height = 1,
            Background = (Brush)Application.Current.Resources["PrimaryActionGradientBrush"],
            CornerRadius = new CornerRadius(1),
            Opacity = 0.65,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };

        var creatorText = new TextBlock
        {
            Text = "created by acchtt",
            FontSize = 11,
            Foreground = (Brush)Application.Current.Resources["TextMutedBrush"],
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };

        var sloganChip = new Border
        {
            Height = 22,
            Padding = new Thickness(8, 0, 8, 0),
            Background = (Brush)Application.Current.Resources["PanelBrush"],
            BorderBrush = (Brush)Application.Current.Resources["StrokeBrush"],
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(11),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                Text = "Precision color, per game",
                FontSize = 10.5,
                FontWeight = Microsoft.UI.Text.FontWeights.Normal,
                Foreground = (Brush)Application.Current.Resources["TextMutedBrush"],
                TextWrapping = TextWrapping.NoWrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };

        var brandText = new StackPanel
        {
            Spacing = 1,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        brandText.Children.Add(brandTitle);
        brandText.Children.Add(titleAccent);
        brandText.Children.Add(creatorText);
        brandText.Children.Add(sloganChip);

        var brandBlock = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 14,
            MinWidth = 282,
            Height = 66,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        brandBlock.Children.Add(logoStage);
        brandBlock.Children.Add(brandText);

        var divider = new Border
        {
            Width = 1,
            Height = 44,
            Margin = new Thickness(16, 0, 16, 0),
            Background = (Brush)Application.Current.Resources["StrokeBrush"],
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var safeIcon = new Border
        {
            Width = 30,
            Height = 30,
            Background = (Brush)Application.Current.Resources["PanelBrush"],
            BorderBrush = (Brush)Application.Current.Resources["StrokeBrush"],
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(15),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Child = new FontIcon
            {
                Glyph = "\uE83D",
                FontSize = 16,
                Foreground = (Brush)Application.Current.Resources["PositiveBrush"],
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };

        var safetyCopy = new StackPanel
        {
            Spacing = 1,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        safetyCopy.Children.Add(new TextBlock
        {
            Text = "Anti-cheat safe",
            FontSize = 13.5,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = (Brush)Application.Current.Resources["TextPrimaryBrush"],
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        });
        safetyCopy.Children.Add(new TextBlock
        {
            Text = "External GPU control",
            FontSize = 10.5,
            Foreground = (Brush)Application.Current.Resources["TextMutedBrush"],
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        });

        var safeContent = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        safeContent.Children.Add(safeIcon);
        safeContent.Children.Add(safetyCopy);

        var safeBadge = new Border
        {
            Width = 180,
            Height = 52,
            Padding = new Thickness(14, 0, 14, 0),
            Background = (Brush)Application.Current.Resources["PanelRaisedBrush"],
            BorderBrush = (Brush)Application.Current.Resources["StrokeBrush"],
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(14),
            Child = safeContent,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        ToolTipService.SetToolTip(
            safeBadge,
            "External GPU-control architecture. No game injection or game-memory access. Not an official anti-cheat certification.");

        var actionGroup = new Grid
        {
            ColumnSpacing = 10,
            Height = 44,
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
            Height = 66,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        commandBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        commandBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        commandBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        commandBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        Grid.SetColumn(brandBlock, 0);
        Grid.SetColumn(divider, 1);
        Grid.SetColumn(safeBadge, 2);
        Grid.SetColumn(actionGroup, 3);
        commandBar.Children.Add(brandBlock);
        commandBar.Children.Add(divider);
        commandBar.Children.Add(safeBadge);
        commandBar.Children.Add(actionGroup);

        if (ProfilesPage.RowDefinitions.Count >= 2)
        {
            ProfilesPage.RowDefinitions[1].Height = new GridLength(104);
        }

        footer.Margin = new Thickness(0, 12, 0, 0);
        footer.Padding = new Thickness(18, 10, 18, 10);
        footer.MinHeight = 94;
        footer.CornerRadius = new CornerRadius(18);
        footer.BorderThickness = new Thickness(1);
        footer.BorderBrush = (Brush)Application.Current.Resources["FooterStrokeBrush"];
        footer.Background = (Brush)Application.Current.Resources["FooterBrush"];
        footer.VerticalAlignment = VerticalAlignment.Center;
        footer.Child = commandBar;
        _footerUtilityBarApplied = true;
    }

    private void ConfigureFooterActionButton(Button button, double width)
    {
        button.Width = width;
        button.Height = 44;
        button.MinHeight = 44;
        button.Margin = new Thickness(0);
        button.Padding = new Thickness(12, 0, 12, 0);
        button.Background = (Brush)Application.Current.Resources["PanelRaisedBrush"];
        button.BorderBrush = (Brush)Application.Current.Resources["StrokeBrush"];
        button.BorderThickness = new Thickness(1);
        button.HorizontalAlignment = HorizontalAlignment.Center;
        button.VerticalAlignment = VerticalAlignment.Center;
        button.HorizontalContentAlignment = HorizontalAlignment.Center;
        button.VerticalContentAlignment = VerticalAlignment.Center;
        button.CornerRadius = new CornerRadius(12);

        foreach (FontIcon icon in EnumerateLayoutDescendants<FontIcon>(button))
        {
            icon.FontSize = 17;
            icon.VerticalAlignment = VerticalAlignment.Center;
        }

        foreach (TextBlock text in EnumerateLayoutDescendants<TextBlock>(button))
        {
            text.FontSize = 13.5;
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
