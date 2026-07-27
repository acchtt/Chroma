using Chroma.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Chroma;

public sealed partial class MainWindow
{
    private readonly ResolutionOverrideStore _resolutionOverrideStore = new();
    private readonly Dictionary<string, ResolutionOverride> _resolutionOverrides =
        new(StringComparer.OrdinalIgnoreCase);

    private ToggleSwitch? _customResolutionToggle;
    private TextBox? _resolutionWidthTextBox;
    private TextBox? _resolutionHeightTextBox;
    private FrameworkElement? _resolutionFields;
    private TextBlock? _resolutionStatusText;
    private bool _syncingResolutionEditor;

    public void EnableCustomResolutionEditor()
    {
        LoadResolutionOverrides();

        // Match only the original placeholder panel. The outer profile-editor card
        // also contains these labels recursively, so requiring a direct StackPanel
        // child prevents replacing the entire editor surface.
        Border? reservedPanel = FindResolutionDescendant<Border>(ProfilesPage, border =>
            border.Child is StackPanel &&
            ResolutionPanelContainsText(border.Child, "Custom resolution") &&
            ResolutionPanelContainsText(border.Child, "Coming soon"));
        if (reservedPanel is null)
        {
            return;
        }

        BuildCustomResolutionPanel(reservedPanel);
        RewireResolutionEditorButtons();
        ProfilesList.SelectionChanged += (_, _) => SyncCustomResolutionEditor();
        _viewModel.Profiles.CollectionChanged += Profiles_CollectionChangedForResolution;
        SyncCustomResolutionEditor();
    }

    private void BuildCustomResolutionPanel(Border panel)
    {
        panel.Background = (Brush)Application.Current.Resources["PanelRaisedBrush"];
        panel.BorderBrush = (Brush)Application.Current.Resources["StrokeBrush"];
        panel.BorderThickness = new Thickness(1);
        panel.CornerRadius = new CornerRadius(12);
        panel.Padding = new Thickness(14, 12, 14, 12);
        panel.IsHitTestVisible = true;
        panel.Opacity = 1;

        var header = new Grid { ColumnSpacing = 12 };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var heading = new StackPanel { Spacing = 3 };
        heading.Children.Add(new TextBlock
        {
            Text = "Custom resolution",
            Foreground = (Brush)Application.Current.Resources["CyanBrush"],
            FontSize = 15,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
        });
        heading.Children.Add(new TextBlock
        {
            Text = "Switch the game display and restore the desktop mode automatically.",
            Foreground = (Brush)Application.Current.Resources["TextMutedBrush"],
            FontSize = 11.5,
            TextWrapping = TextWrapping.Wrap
        });

        _customResolutionToggle = new ToggleSwitch
        {
            OffContent = string.Empty,
            OnContent = string.Empty,
            MinWidth = 46,
            Width = 46,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        _customResolutionToggle.Toggled += CustomResolutionToggle_Toggled;
        Grid.SetColumn(_customResolutionToggle, 1);
        header.Children.Add(heading);
        header.Children.Add(_customResolutionToggle);

        var widthStack = CreateResolutionField("Width", out _resolutionWidthTextBox, "1680");
        var heightStack = CreateResolutionField("Height", out _resolutionHeightTextBox, "1050");

        var fieldsGrid = new Grid { ColumnSpacing = 10 };
        fieldsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        fieldsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        fieldsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        fieldsGrid.Children.Add(widthStack);

        var separator = new TextBlock
        {
            Text = "×",
            FontSize = 20,
            Foreground = (Brush)Application.Current.Resources["TextMutedBrush"],
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 0, 9)
        };
        Grid.SetColumn(separator, 1);
        fieldsGrid.Children.Add(separator);
        Grid.SetColumn(heightStack, 2);
        fieldsGrid.Children.Add(heightStack);
        _resolutionFields = fieldsGrid;

        var refreshNote = new Grid { ColumnSpacing = 8 };
        refreshNote.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        refreshNote.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        refreshNote.Children.Add(new FontIcon
        {
            Glyph = "\uE946",
            FontSize = 13,
            Foreground = (Brush)Application.Current.Resources["TextMutedBrush"],
            VerticalAlignment = VerticalAlignment.Center
        });
        var refreshText = new TextBlock
        {
            Text = "Only driver-supported modes at the current refresh rate are applied.",
            Foreground = (Brush)Application.Current.Resources["TextMutedBrush"],
            FontSize = 11.5,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(refreshText, 1);
        refreshNote.Children.Add(refreshText);

        _resolutionStatusText = new TextBlock
        {
            Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
            FontSize = 11.5,
            TextWrapping = TextWrapping.Wrap
        };

        var content = new StackPanel { Spacing = 11 };
        content.Children.Add(header);
        content.Children.Add(fieldsGrid);
        content.Children.Add(refreshNote);
        content.Children.Add(_resolutionStatusText);
        panel.Child = content;
    }

    private StackPanel CreateResolutionField(string label, out TextBox textBox, string placeholder)
    {
        textBox = new TextBox
        {
            Height = 42,
            MaxLength = 5,
            PlaceholderText = placeholder,
            TextAlignment = TextAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            FontSize = 15,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
        };
        textBox.BeforeTextChanging += ResolutionTextBox_BeforeTextChanging;
        textBox.TextChanged += ResolutionTextBox_TextChanged;
        textBox.LostFocus += (_, _) => UpdateResolutionEditorState();

        var stack = new StackPanel { Spacing = 5 };
        stack.Children.Add(new TextBlock
        {
            Text = label,
            Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
            FontSize = 11.5
        });
        stack.Children.Add(textBox);
        return stack;
    }

    private void RewireResolutionEditorButtons()
    {
        Button? saveButton = FindResolutionDescendant<Button>(ProfilesPage, button =>
            button.Content is string text && text == "Save Changes");
        if (saveButton is not null)
        {
            saveButton.Click -= SaveEditor_Click;
            saveButton.Click += SaveEditorWithResolution_Click;
        }

        Button? cancelButton = FindResolutionDescendant<Button>(ProfilesPage, button =>
            button.Content is string text && text == "Cancel");
        if (cancelButton is not null)
        {
            cancelButton.Click -= CancelEditor_Click;
            cancelButton.Click += CancelEditorWithResolution_Click;
        }
    }

    private static bool ResolutionPanelContainsText(DependencyObject root, string text) =>
        FindResolutionDescendant<TextBlock>(root,
            block => string.Equals(block.Text, text, StringComparison.Ordinal)) is not null;

    private static T? FindResolutionDescendant<T>(DependencyObject root, Func<T, bool> predicate)
        where T : DependencyObject
    {
        if (root is T typedRoot && predicate(typedRoot))
        {
            return typedRoot;
        }

        int count = VisualTreeHelper.GetChildrenCount(root);
        for (int index = 0; index < count; index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(root, index);
            T? match = FindResolutionDescendant(child, predicate);
            if (match is not null)
            {
                return match;
            }
        }

        return null;
    }
}
