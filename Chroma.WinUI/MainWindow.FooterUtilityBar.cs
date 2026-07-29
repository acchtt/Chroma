using Chroma.Services;
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

        ConfigureFooterActionButton(websiteButton, 130);
        ConfigureFooterActionButton(githubButton, 130);
        ConfigureFooterActionButton(UpdatesButton, 170);
        UpdatesButtonText.Text = "Check updates";

        FooterBrandLogo.Width = 56;
        FooterBrandLogo.Height = 56;
        FooterBrandLogo.Margin = new Thickness(0);
        FooterBrandLogo.VerticalAlignment = VerticalAlignment.Center;

        FooterVersionText.Text = $"{UpdateService.CurrentVersionTag}  •  WinUI 3  •  .NET 8";
        FooterVersionText.FontSize = 12;
        FooterVersionText.Foreground = (Brush)Application.Current.Resources["TextMutedBrush"];
        FooterVersionText.VerticalAlignment = VerticalAlignment.Center;

        var brandText = new StackPanel
        {
            Spacing = 4,
            VerticalAlignment = VerticalAlignment.Center
        };
        brandText.Children.Add(new TextBlock
        {
            Text = "Chroma",
            FontSize = 24,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = (Brush)Application.Current.Resources["CyanBrush"]
        });
        brandText.Children.Add(FooterVersionText);

        var brandBlock = new Grid
        {
            MinWidth = 300,
            ColumnSpacing = 18,
            VerticalAlignment = VerticalAlignment.Center
        };
        brandBlock.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        brandBlock.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        Grid.SetColumn(FooterBrandLogo, 0);
        Grid.SetColumn(brandText, 1);
        brandBlock.Children.Add(FooterBrandLogo);
        brandBlock.Children.Add(brandText);

        var divider = new Border
        {
            Width = 1,
            Height = 52,
            Margin = new Thickness(12, 0, 12, 0),
            Background = (Brush)Application.Current.Resources["StrokeBrush"],
            VerticalAlignment = VerticalAlignment.Center
        };

        var safeContent = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        safeContent.Children.Add(new FontIcon
        {
            Glyph = "\uE83D",
            FontSize = 19,
            Foreground = (Brush)Application.Current.Resources["PositiveBrush"],
            VerticalAlignment = VerticalAlignment.Center
        });
        safeContent.Children.Add(new TextBlock
        {
            Text = "Anti-cheat safe",
            FontSize = 14,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = (Brush)Application.Current.Resources["TextPrimaryBrush"],
            VerticalAlignment = VerticalAlignment.Center
        });

        var safeBadge = new Border
        {
            Width = 190,
            Height = 44,
            Padding = new Thickness(16, 0, 16, 0),
            Background = (Brush)Application.Current.Resources["PanelRaisedBrush"],
            BorderBrush = (Brush)Application.Current.Resources["StrokeBrush"],
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Child = safeContent,
            VerticalAlignment = VerticalAlignment.Center
        };
        ToolTipService.SetToolTip(
            safeBadge,
            "External GPU-control architecture. No game injection or game-memory access. Not an official anti-cheat certification.");

        var actionGroup = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        actionGroup.Children.Add(safeBadge);
        actionGroup.Children.Add(websiteButton);
        actionGroup.Children.Add(githubButton);
        actionGroup.Children.Add(UpdatesButton);

        var commandBar = new Grid
        {
            VerticalAlignment = VerticalAlignment.Stretch
        };
        commandBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        commandBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        commandBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        commandBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        Grid.SetColumn(brandBlock, 0);
        Grid.SetColumn(divider, 1);
        Grid.SetColumn(actionGroup, 3);
        commandBar.Children.Add(brandBlock);
        commandBar.Children.Add(divider);
        commandBar.Children.Add(actionGroup);

        if (ProfilesPage.RowDefinitions.Count >= 2)
        {
            ProfilesPage.RowDefinitions[1].Height = new GridLength(122);
        }

        footer.Margin = new Thickness(0, 12, 0, 0);
        footer.Padding = new Thickness(24, 16, 24, 16);
        footer.MinHeight = 110;
        footer.CornerRadius = new CornerRadius(18);
        footer.BorderThickness = new Thickness(1);
        footer.BorderBrush = (Brush)Application.Current.Resources["StrokeBrush"];
        footer.Background = (Brush)Application.Current.Resources["PanelBrush"];
        footer.Child = commandBar;
        _footerUtilityBarApplied = true;
    }

    private void ConfigureFooterActionButton(Button button, double width)
    {
        button.Width = width;
        button.Height = 44;
        button.MinHeight = 44;
        button.Padding = new Thickness(16, 0, 16, 0);
        button.HorizontalAlignment = HorizontalAlignment.Right;
        button.VerticalAlignment = VerticalAlignment.Center;
        button.CornerRadius = new CornerRadius(12);

        foreach (FontIcon icon in EnumerateLayoutDescendants<FontIcon>(button))
        {
            icon.FontSize = 18;
        }

        foreach (TextBlock text in EnumerateLayoutDescendants<TextBlock>(button))
        {
            text.FontSize = 14;
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
