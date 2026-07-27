using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Chroma;

public sealed partial class MainWindow
{
    private Border? _antiCheatSafetyCard;

    public void EnableAntiCheatSafety()
    {
        if (_antiCheatSafetyCard is not null || AboutPage.Content is not StackPanel aboutStack)
        {
            return;
        }

        var headerGrid = new Grid
        {
            ColumnSpacing = 14
        };
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(176) });

        var shieldTile = new Border
        {
            Width = 48,
            Height = 48,
            Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["ProfileIconBrush"],
            BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["ProfileIconStrokeBrush"],
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(13),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new FontIcon
            {
                Glyph = "\uE83D",
                FontSize = 23,
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["CyanBrush"]
            }
        };

        var headingStack = new StackPanel
        {
            Spacing = 3,
            VerticalAlignment = VerticalAlignment.Center
        };
        headingStack.Children.Add(new TextBlock
        {
            Text = "Anti-cheat safety",
            FontSize = 19,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
        });
        headingStack.Children.Add(new TextBlock
        {
            Text = "External GPU control • no game-process access",
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextMutedBrush"],
            FontSize = 12.5,
            TextWrapping = TextWrapping.Wrap
        });
        Grid.SetColumn(headingStack, 1);

        var statusRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 7
        };
        statusRow.Children.Add(new Microsoft.UI.Xaml.Shapes.Ellipse
        {
            Width = 8,
            Height = 8,
            Fill = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["PositiveBrush"],
            VerticalAlignment = VerticalAlignment.Center
        });
        statusRow.Children.Add(new TextBlock
        {
            Text = "SAFE",
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["PositiveBrush"],
            FontSize = 11,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            CharacterSpacing = 70
        });

        var statusContent = new StackPanel
        {
            Spacing = 2
        };
        statusContent.Children.Add(statusRow);
        statusContent.Children.Add(new TextBlock
        {
            Text = "Architecture review",
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextMutedBrush"],
            FontSize = 11.5
        });

        var statusPanel = new Border
        {
            Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["StatusCardBrush"],
            BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["StatusCardStrokeBrush"],
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(11),
            Padding = new Thickness(12, 8, 12, 8),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Child = statusContent
        };
        Grid.SetColumn(statusPanel, 2);

        headerGrid.Children.Add(shieldTile);
        headerGrid.Children.Add(headingStack);
        headerGrid.Children.Add(statusPanel);

        var headerPanel = new Border
        {
            Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["StatusCardBrush"],
            BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["StatusCardStrokeBrush"],
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(16, 14, 16, 14),
            Child = headerGrid
        };

        var summary = new TextBlock
        {
            Text = "Chroma stays outside game processes and changes display saturation through GPU-vendor control APIs. Its current architecture avoids the techniques commonly associated with cheats.",
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextSecondaryBrush"],
            FontSize = 14,
            TextWrapping = TextWrapping.Wrap
        };

        var details = new StackPanel { Spacing = 7, Margin = new Thickness(0, 3, 0, 0) };
        details.Children.Add(CreateSafetyLine("No DLL injection, game-code hooks, or internal overlay"));
        details.Children.Add(CreateSafetyLine("No reading or writing of game memory"));
        details.Children.Add(CreateSafetyLine("No game-file modification, input automation, or kernel driver"));
        details.Children.Add(CreateSafetyLine("Foreground detection uses a limited Windows process-name query only"));
        details.Children.Add(CreateSafetyLine("Color control uses Intel IGCL, NVIDIA NVAPI, or AMD ADLX"));

        var assessment = new TextBlock
        {
            Text = "Based on the current source code, Chroma is considered low risk for VAC, BattlEye, Easy Anti-Cheat, and Riot Vanguard. This is an architectural assessment, not an official certification or allowlisting.",
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextSecondaryBrush"],
            FontSize = 13.5,
            TextWrapping = TextWrapping.Wrap
        };

        var disclaimer = new Border
        {
            Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["StatusCardBrush"],
            BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["StatusCardStrokeBrush"],
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(14, 11, 14, 11),
            Margin = new Thickness(0, 3, 0, 0),
            Child = new TextBlock
            {
                Text = "Important: No third-party utility can guarantee approval by every game or anti-cheat provider. Publisher policies and detection rules can change. Stop using Chroma with a game if its publisher explicitly prohibits external color-adjustment utilities.",
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextMutedBrush"],
                FontSize = 12.5,
                TextWrapping = TextWrapping.Wrap
            }
        };

        var content = new StackPanel { Spacing = 14 };
        content.Children.Add(headerPanel);
        content.Children.Add(summary);
        content.Children.Add(details);
        content.Children.Add(assessment);
        content.Children.Add(disclaimer);

        _antiCheatSafetyCard = new Border
        {
            Style = (Style)Application.Current.Resources["CardBorderStyle"],
            Padding = new Thickness(24),
            Child = content
        };

        aboutStack.Children.Add(_antiCheatSafetyCard);
    }

    private static TextBlock CreateSafetyLine(string text)
    {
        return new TextBlock
        {
            Text = $"• {text}",
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextSecondaryBrush"],
            FontSize = 13.5,
            TextWrapping = TextWrapping.Wrap
        };
    }
}
