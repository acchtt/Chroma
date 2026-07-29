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

        ConfigureFooterActionButton(websiteButton, 128);
        ConfigureFooterActionButton(githubButton, 128);
        ConfigureFooterActionButton(UpdatesButton, 164);
        UpdatesButtonText.Text = "Check updates";

        FooterBrandLogo.Width = 56;
        FooterBrandLogo.Height = 56;
        FooterBrandLogo.Margin = new Thickness(0);
        FooterBrandLogo.HorizontalAlignment = HorizontalAlignment.Center;
        FooterBrandLogo.VerticalAlignment = VerticalAlignment.Center;

        var brandText = new StackPanel
        {
            Spacing = 4,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        brandText.Children.Add(new TextBlock
        {
            Text = "Chroma",
            FontSize = 24,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = (Brush)Application.Current.Resources["CyanBrush"],
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        });
        brandText.Children.Add(new TextBlock
        {
            Text = "created by acchtt",
            FontSize = 12,
            Foreground = (Brush)Application.Current.Resources["TextMutedBrush"],
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        });

        var brandBlock = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 18,
            MinWidth = 300,
            Height = 64,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        brandBlock.Children.Add(FooterBrandLogo);
        brandBlock.Children.Add(brandText);

        var divider = new Border
        {
            Width = 1,
            Height = 52,
            Margin = new Thickness(24, 0, 24, 0),
            Background = (Brush)Application.Current.Resources["StrokeBrush"],
            HorizontalAlignment = HorizontalAlignment.Center,
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
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        });
        safeContent.Children.Add(new TextBlock
        {
            Text = "Anti-cheat safe",
            FontSize = 14,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = (Brush)Application.Current.Resources["TextPrimaryBrush"],
            HorizontalAlignment = HorizontalAlignment.Center,
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
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var actionGroup = new Grid
        {
            ColumnSpacing = 16,
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
            Height = 64,
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
            ProfilesPage.RowDefinitions[1].Height = new GridLength(110);
        }

        footer.Margin = new Thickness(0, 12, 0, 0);
        footer.Padding = new Thickness(24, 14, 24, 14);
        footer.MinHeight = 110;
        footer.CornerRadius = new CornerRadius(18);
        footer.BorderThickness = new Thickness(1);
        footer.BorderBrush = (Brush)Application.Current.Resources["StrokeBrush"];
        footer.Background = (Brush)Application.Current.Resources["PanelBrush"];
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
        button.Padding = new Thickness(16, 0, 16, 0);
        button.HorizontalAlignment = HorizontalAlignment.Center;
        button.VerticalAlignment = VerticalAlignment.Center;
        button.HorizontalContentAlignment = HorizontalAlignment.Center;
        button.VerticalContentAlignment = VerticalAlignment.Center;
        button.CornerRadius = new CornerRadius(12);

        foreach (FontIcon icon in EnumerateLayoutDescendants<FontIcon>(button))
        {
            icon.FontSize = 18;
            icon.VerticalAlignment = VerticalAlignment.Center;
        }

        foreach (TextBlock text in EnumerateLayoutDescendants<TextBlock>(button))
        {
            text.FontSize = 14;
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
