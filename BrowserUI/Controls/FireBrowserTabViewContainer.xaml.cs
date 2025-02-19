using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BrowserUICore.Models;
using BrowserUI.Pages;


namespace BrowserUI.Controls
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FireBrowserTabViewContainer : TabView
    {
        public FireBrowserTabViewContainer()
        {
            this.InitializeComponent();
            ViewModel = new FireBrowserTabViewViewModel()
            {
                Style = (Style)Application.Current.Resources["DefaultTabViewStyle"]
            };
        }

        public FireBrowserTabViewViewModel ViewModel { get; set; } = new FireBrowserTabViewViewModel();

        public partial class FireBrowserTabViewViewModel : ObservableObject
        {
            [ObservableProperty] private Style style;
        }

        public Settings.UILayout Mode
        {
            get => (Settings.UILayout)GetValue(ModeProperty);
            set
            {
                ViewModel.Style = value switch
                {
                    Settings.UILayout.Modern => (Style)Application.Current.Resources["DefaultTabViewStyle"],
                    Settings.UILayout.Vertical => (Style)Application.Current.Resources["VerticalTabViewStyle"],
                    _ => (Style)Application.Current.Resources["DefaultTabViewStyle"]
                };

                SetValue(ModeProperty, value);
            }
        }


        public static DependencyProperty ModeProperty = DependencyProperty.Register(
        nameof(Mode),
        typeof(Settings.UILayout),
        typeof(FireBrowserTabViewContainer),
        null);

        private void Tabs_AddTabButtonClick(TabView sender, object args)
        {
            var newTab = new FireBrowserTabViewItem
            {
                Header = "New Tab",
                Content = new Frame
                {
                    Margin = new Thickness(0, 35, 0, 0) // Ensures new tabs respect the top margin
                }
            };

            ((Frame)newTab.Content).Navigate(typeof(NewTab));

            TabItems.Add(newTab);
            sender.SelectedItem = newTab;
        }


        // Event handler for closing a tab

    }
}

