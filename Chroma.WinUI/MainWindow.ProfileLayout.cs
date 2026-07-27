using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Chroma;

public sealed partial class MainWindow
{
    private bool _profileLayoutRefreshEnabled;

    public void EnableProfileLayoutRefresh()
    {
        if (_profileLayoutRefreshEnabled)
        {
            return;
        }

        _profileLayoutRefreshEnabled = true;

        // Give the reorganized three-column workspace enough room while keeping
        // the window fully resizable for smaller and larger displays.
        _appWindow?.Resize(new Windows.Graphics.SizeInt32(1360, 860));

        ApplyShellLayout();
        ApplyProfilesWorkspaceLayout();

        ProfilesList.Loaded += (_, _) => ApplyRealizedProfileCardLayouts();
        ProfilesList.ContainerContentChanging += ProfilesList_ContainerContentChangingForLayout;
        DispatcherQueue.TryEnqueue(ApplyRealizedProfileCardLayouts);
    }

    private void ApplyShellLayout()
    {
        Grid? shell = Root.Children
            .OfType<Grid>()
            .FirstOrDefault(grid => Grid.GetRow(grid) == 1);
        if (shell is null || shell.ColumnDefinitions.Count < 2)
        {
            return;
        }

        shell.ColumnDefinitions[0].Width = new GridLength(240);

        Border? sidebar = shell.Children
            .OfType<Border>()
            .FirstOrDefault(border => Grid.GetColumn(border) == 0);
        if (sidebar?.Child is Grid sidebarGrid)
        {
            sidebarGrid.Margin = new Thickness(20, 16, 20, 18);

            Border? statusCard = sidebarGrid.Children
                .OfType<Border>()
                .FirstOrDefault(border => Grid.GetRow(border) == 3);
            if (statusCard is not null)
            {
                statusCard.Padding = new Thickness(16, 15, 16, 15);
                statusCard.MinHeight = 166;
                statusCard.CornerRadius = new CornerRadius(15);
            }
        }

        SidebarBrandLogo.Width = 132;
        SidebarBrandLogo.Height = 132;

        foreach (Border navChrome in new[] { ProfilesNavChrome, SettingsNavChrome, AboutNavChrome })
        {
            navChrome.Width = 190;
            navChrome.Height = 56;
            navChrome.CornerRadius = new CornerRadius(12);
        }
    }

    private void ApplyProfilesWorkspaceLayout()
    {
        ProfilesPage.Margin = new Thickness(22, 18, 22, 18);

        if (ProfilesPage.RowDefinitions.Count >= 2)
        {
            ProfilesPage.RowDefinitions[1].Height = new GridLength(150);
        }

        if (ProfilesPage.ColumnDefinitions.Count >= 2)
        {
            ProfilesPage.ColumnDefinitions[1].Width = new GridLength(420);
        }

        Grid? profilePane = ProfilesPage.Children
            .OfType<Grid>()
            .FirstOrDefault(grid => Grid.GetColumn(grid) == 0 && Grid.GetRow(grid) == 0);
        if (profilePane is not null)
        {
            profilePane.Margin = new Thickness(0, 0, 18, 0);

            Grid? header = profilePane.Children
                .OfType<Grid>()
                .FirstOrDefault(grid => Grid.GetRow(grid) == 0);
            if (header is not null)
            {
                header.Margin = new Thickness(4, 2, 0, 18);

                TextBlock? title = EnumerateLayoutDescendants<TextBlock>(header)
                    .FirstOrDefault(block => block.Text == "Game Profiles");
                TextBlock? subtitle = EnumerateLayoutDescendants<TextBlock>(header)
                    .FirstOrDefault(block => block.Text == "Manage per-game saturation profiles");

                if (title is not null)
                {
                    title.FontSize = 28;
                }

                if (subtitle is not null)
                {
                    subtitle.FontSize = 14;
                    subtitle.Margin = new Thickness(0, 5, 0, 0);
                }

                foreach (Button button in EnumerateLayoutDescendants<Button>(header))
                {
                    button.MinHeight = 46;
                }
            }
        }

        ProfilesList.Margin = new Thickness(0, 0, 0, 2);

        DropZone.Padding = new Thickness(24, 22, 24, 22);
        DropZone.Margin = new Thickness(0, 10, 0, 0);
        DropZone.MinHeight = 104;
        DropZone.CornerRadius = new CornerRadius(14);

        Border? editorCard = ProfilesPage.Children
            .OfType<Border>()
            .FirstOrDefault(border => Grid.GetColumn(border) == 1 && Grid.GetRow(border) == 0);
        if (editorCard is not null)
        {
            editorCard.Padding = new Thickness(24, 22, 24, 20);
            editorCard.CornerRadius = new CornerRadius(16);
            TuneEditorLayout(editorCard);
        }

        Border? footer = ProfilesPage.Children
            .OfType<Border>()
            .FirstOrDefault(border => Grid.GetRow(border) == 1 && Grid.GetColumnSpan(border) == 2);
        if (footer is not null)
        {
            footer.Margin = new Thickness(0, 18, 0, 0);
            footer.Padding = new Thickness(18, 14, 18, 14);
            footer.CornerRadius = new CornerRadius(16);

            Grid? footerGrid = footer.Child as Grid;
            if (footerGrid is not null && footerGrid.ColumnDefinitions.Count >= 2)
            {
                footerGrid.ColumnSpacing = 24;
                footerGrid.ColumnDefinitions[0].Width = new GridLength(0.9, GridUnitType.Star);
                footerGrid.ColumnDefinitions[1].Width = new GridLength(1.25, GridUnitType.Star);
            }

            foreach (Button button in EnumerateLayoutDescendants<Button>(footer))
            {
                button.MinHeight = 40;
            }
        }
    }

    private void TuneEditorLayout(Border editorCard)
    {
        TextBlock? editorTitle = EnumerateLayoutDescendants<TextBlock>(editorCard)
            .FirstOrDefault(block => block.Text == "Edit Profile");
        if (editorTitle is not null)
        {
            editorTitle.FontSize = 22;
        }

        StackPanel? editorBody = EnumerateLayoutDescendants<StackPanel>(editorCard)
            .FirstOrDefault(panel =>
                Grid.GetRow(panel) == 1 &&
                panel.Children.Count >= 5 &&
                ContainsLayoutText(panel, "Saturation"));
        if (editorBody is not null)
        {
            editorBody.Margin = new Thickness(0, 16, 0, 10);
            editorBody.Spacing = 14;
        }

        foreach (Border iconBorder in EnumerateLayoutDescendants<Border>(editorCard)
                     .Where(border => Math.Abs(border.Width - 68) < 0.1 && Math.Abs(border.Height - 68) < 0.1))
        {
            iconBorder.Width = 74;
            iconBorder.Height = 74;
            iconBorder.CornerRadius = new CornerRadius(12);

            Image? icon = FindLayoutDescendant<Image>(iconBorder, _ => true);
            if (icon is not null)
            {
                icon.Width = 64;
                icon.Height = 64;
            }
        }

        TextBlock? saturationHeading = EnumerateLayoutDescendants<TextBlock>(editorCard)
            .FirstOrDefault(block => block.Text == "Saturation");
        if (saturationHeading is not null)
        {
            saturationHeading.FontSize = 16.5;
        }

        Grid? saturationEditor = EnumerateLayoutDescendants<Grid>(editorCard)
            .FirstOrDefault(grid => Math.Abs(grid.Width - 212) < 0.1 && Math.Abs(grid.Height - 54) < 0.1);
        if (saturationEditor is not null)
        {
            saturationEditor.Width = 232;
            saturationEditor.Height = 58;
            saturationEditor.Margin = new Thickness(0, 5, 0, 0);
        }

        Border? resolutionCard = FindDeepestLayoutDescendant<Border>(editorCard, border =>
            border.Child is StackPanel && ContainsLayoutText(border.Child, "Custom resolution"));
        if (resolutionCard is not null)
        {
            resolutionCard.Padding = new Thickness(16, 14, 16, 14);
            resolutionCard.Margin = new Thickness(0, 5, 0, 0);
            resolutionCard.CornerRadius = new CornerRadius(14);
        }

        foreach (Button button in EnumerateLayoutDescendants<Button>(editorCard))
        {
            if (button.Content is string text && (text == "Cancel" || text == "Save Changes"))
            {
                button.Height = 48;
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
            actionRow.Margin = new Thickness(0, 16, 0, 0);
        }
    }

    private void ProfilesList_ContainerContentChangingForLayout(
        ListViewBase sender,
        ContainerContentChangingEventArgs args)
    {
        if (args.InRecycleQueue)
        {
            return;
        }

        DependencyObject container = args.ItemContainer;
        DispatcherQueue.TryEnqueue(() => ApplyProfileCardLayout(container));
    }

    private void ApplyRealizedProfileCardLayouts()
    {
        for (int index = 0; index < ProfilesList.Items.Count; index++)
        {
            if (ProfilesList.ContainerFromIndex(index) is DependencyObject container)
            {
                ApplyProfileCardLayout(container);
            }
        }
    }

    private static void ApplyProfileCardLayout(DependencyObject container)
    {
        Border? card = FindDeepestLayoutDescendant<Border>(container, border =>
            border.Child is Grid grid && grid.ColumnDefinitions.Count == 5);
        if (card?.Child is not Grid contentGrid)
        {
            return;
        }

        card.MinHeight = 92;
        card.Padding = new Thickness(16, 12, 16, 12);
        card.CornerRadius = new CornerRadius(14);
        contentGrid.ColumnSpacing = 14;

        if (contentGrid.ColumnDefinitions.Count >= 5)
        {
            contentGrid.ColumnDefinitions[0].Width = new GridLength(64);
            contentGrid.ColumnDefinitions[2].Width = new GridLength(76);
            contentGrid.ColumnDefinitions[3].Width = new GridLength(52);
            contentGrid.ColumnDefinitions[4].Width = new GridLength(28);
        }

        Border? iconBorder = contentGrid.Children
            .OfType<Border>()
            .FirstOrDefault(border => Grid.GetColumn(border) == 0);
        if (iconBorder is not null)
        {
            iconBorder.Width = 64;
            iconBorder.Height = 64;
            iconBorder.CornerRadius = new CornerRadius(11);

            Image? image = FindLayoutDescendant<Image>(iconBorder, _ => true);
            if (image is not null)
            {
                image.Width = 56;
                image.Height = 56;
            }
        }

        StackPanel? details = contentGrid.Children
            .OfType<StackPanel>()
            .FirstOrDefault(panel => Grid.GetColumn(panel) == 1);
        if (details is not null)
        {
            details.Spacing = 5;
            if (details.Children.OfType<TextBlock>().FirstOrDefault() is TextBlock name)
            {
                name.FontSize = 16.5;
            }

            if (details.Children.OfType<TextBlock>().Skip(1).FirstOrDefault() is TextBlock path)
            {
                path.FontSize = 12;
            }
        }

        TextBlock? saturation = contentGrid.Children
            .OfType<TextBlock>()
            .FirstOrDefault(block => Grid.GetColumn(block) == 2);
        if (saturation is not null)
        {
            saturation.FontSize = 18;
        }

        ToggleSwitch? toggle = contentGrid.Children
            .OfType<ToggleSwitch>()
            .FirstOrDefault(control => Grid.GetColumn(control) == 3);
        if (toggle is not null)
        {
            toggle.Width = 48;
            toggle.MinWidth = 48;
        }
    }

    private static bool ContainsLayoutText(DependencyObject root, string text) =>
        FindLayoutDescendant<TextBlock>(root,
            block => string.Equals(block.Text, text, StringComparison.Ordinal)) is not null;

    private static T? FindLayoutDescendant<T>(DependencyObject root, Func<T, bool> predicate)
        where T : DependencyObject
    {
        if (root is T typedRoot && predicate(typedRoot))
        {
            return typedRoot;
        }

        int childCount = VisualTreeHelper.GetChildrenCount(root);
        for (int index = 0; index < childCount; index++)
        {
            T? match = FindLayoutDescendant(VisualTreeHelper.GetChild(root, index), predicate);
            if (match is not null)
            {
                return match;
            }
        }

        return null;
    }

    private static T? FindDeepestLayoutDescendant<T>(DependencyObject root, Func<T, bool> predicate)
        where T : DependencyObject
    {
        int childCount = VisualTreeHelper.GetChildrenCount(root);
        for (int index = 0; index < childCount; index++)
        {
            T? match = FindDeepestLayoutDescendant(VisualTreeHelper.GetChild(root, index), predicate);
            if (match is not null)
            {
                return match;
            }
        }

        return root is T typedRoot && predicate(typedRoot) ? typedRoot : null;
    }

    private static IEnumerable<T> EnumerateLayoutDescendants<T>(DependencyObject root)
        where T : DependencyObject
    {
        int childCount = VisualTreeHelper.GetChildrenCount(root);
        for (int index = 0; index < childCount; index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(root, index);
            if (child is T typedChild)
            {
                yield return typedChild;
            }

            foreach (T descendant in EnumerateLayoutDescendants<T>(child))
            {
                yield return descendant;
            }
        }
    }
}
