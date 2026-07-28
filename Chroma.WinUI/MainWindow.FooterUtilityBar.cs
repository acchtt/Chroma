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

        ConfigureFooterActionButton(websiteButton, 90);
        ConfigureFooterActionButton(githubButton, 86);
        ConfigureFooterActionButton(UpdatesButton, 126);
        UpdatesButtonText.Text = "Check updates";

        FooterVersionText.Text = $"{UpdateService.CurrentVersionTag}  •  WinUI 3  •  .NET 8";
        FooterVersionText.FontSize = 11;
        FooterVersionText.Foreground = (Brush)Application.Current.Resources["TextMutedBrush"];
        FooterVersionText.VerticalAlignment = VerticalAlignment.Center;

        FooterBrandLogo.Width = 40;
        FooterBrandLogo.Height = 40;
        FooterBrandLogo.Margin = new Thickness(4, 0, 0, 0);
        FooterBrandLogo.VerticalAlignment = VerticalAlignment.Center;

        var appDetails = new StackPanel
        {
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center
        };
        appDetails.Children.Add(new TextBlock
        {
            Text = "Chroma",
            FontSize = 14,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = (Brush)Application.Current.Resources["CyanBrush"]
        });
        appDetails.Children.Add(FooterVersionText);

        var utilityGrid = new Grid
        {
            ColumnSpacing = 10,
            VerticalAlignment = VerticalAlignment.Center
        };
        utilityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        utilityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        utilityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        utilityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        utilityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        utilityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        Grid.SetColumn(appDetails, 0);
        Grid.SetColumn(websiteButton, 2);
        Grid.SetColumn(githubButton, 3);
        Grid.SetColumn(UpdatesButton, 4);
        Grid.SetColumn(FooterBrandLogo, 5);

        utilityGrid.Children.Add(appDetails);
        utilityGrid.Children.Add(websiteButton);
        utilityGrid.Children.Add(githubButton);
        utilityGrid.Children.Add(UpdatesButton);
        utilityGrid.Children.Add(FooterBrandLogo);

        if (ProfilesPage.RowDefinitions.Count >= 2)
        {
            ProfilesPage.RowDefinitions[1].Height = new GridLength(82);
        }

        footer.Margin = new Thickness(0, 10, 0, 0);
        footer.Padding = new Thickness(16, 8, 16, 8);
        footer.MinHeight = 72;
        footer.CornerRadius = new CornerRadius(14);
        footer.Child = utilityGrid;
        _footerUtilityBarApplied = true;
    }

    private static void ConfigureFooterActionButton(Button button, double width)
    {
        button.Width = width;
        button.Height = 36;
        button.MinHeight = 36;
        button.Padding = new Thickness(12, 0, 12, 0);
        button.HorizontalAlignment = HorizontalAlignment.Right;
        button.VerticalAlignment = VerticalAlignment.Center;
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
