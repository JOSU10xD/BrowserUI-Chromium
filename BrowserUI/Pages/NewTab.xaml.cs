using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using Windows.UI.Core;
using Microsoft.UI.Dispatching;
using Windows.UI.WebUI;

namespace BrowserUI.Pages
{
    public sealed partial class NewTab : Page
    {
        private bool IsHomeScreenVisible = true;
        private DispatcherTimer timer;

        public NewTab()
        {
            this.InitializeComponent();
            InitializeTime();
        }

        private void InitializeTime()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                var now = DateTime.Now;
                NtpTime.Text = now.ToString("HH:mm:ss");
                NtpDate.Text = now.ToString("dddd, MMM dd yyyy");
            };
            timer.Start();
        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string query = args.QueryText;

            if (!string.IsNullOrWhiteSpace(query))
            {
                string url;

                if (!query.Contains("."))
                {
                    url = $"https://www.bing.com/search?q={Uri.EscapeDataString(query)}";
                }
                else if (!query.StartsWith("http"))
                {
                    url = $"https://{query}";
                }
                else
                {
                    url = query;
                }

                NavigateToBrowser(url);
            }
        }


        private void NavigateToBrowser(string url)
        {
            BrowserView.Source = new Uri(url);
            ShowBrowserView();
        }

        private void ShowBrowserView()
        {
            IsHomeScreenVisible = false;

            HomeScreenGrid.Visibility = Visibility.Collapsed;

            BrowserView.Visibility = Visibility.Visible;

            // Listen for navigation events
            BrowserView.NavigationCompleted += BrowserView_NavigationCompleted;
        }

        private void ShowHomeScreen()
        {
            IsHomeScreenVisible = true;

            HomeScreenGrid.Visibility = Visibility.Visible;

            BrowserView.Visibility = Visibility.Collapsed;
        }

        public void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (BrowserView.CanGoBack)
            {
                BrowserView.GoBack();
            }
            else
            {
                ShowHomeScreen();
            }
        }

        public void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (BrowserView.CanGoForward)
            {
                BrowserView.GoForward();
            }
        }

        public void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            BrowserView.Reload();
        }
        public void Dispose()
        {
           BrowserView?.Close();
            BrowserView?.CoreWebView2?.Stop();
            BrowserView = null;
        }
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Settings",
                Content = new TextBlock { Text = "Settings dialog placeholder." },
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            _ = dialog.ShowAsync();
        }

        private void ToggleDateButton_Click(object sender, RoutedEventArgs e)
        {
            if (NtpTime.Visibility == Visibility.Visible)
            {
                NtpTime.Visibility = Visibility.Collapsed;
                NtpDate.Visibility = Visibility.Collapsed;
            }
            else
            {
                NtpTime.Visibility = Visibility.Visible;
                NtpDate.Visibility = Visibility.Visible;
            }
        }

        private void BrowserView_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            // Optional: Detect if you are back to home/startup URL, e.g., "about:blank" or any default URL you set
        }
    }
}
