using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Microsoft.UI.Dispatching;
using System.Diagnostics;
using DataAccessLibrary;
using BrowserUIMultiCore;

namespace BrowserUI.Pages
{
    public sealed partial class NewTab : Page
    {
        private bool IsHomeScreenVisible = true;
        private DispatcherTimer timer;
        private string homePageUrl = "https://www.google.com"; // Set your actual homepage URL

        public NewTab()
        {
            this.InitializeComponent();
            InitializeTime();
            InitializeWebView();
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

        private async void InitializeWebView()
        {
            await BrowserView.EnsureCoreWebView2Async();
            BrowserView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            BrowserView.NavigationCompleted += BrowserView_NavigationCompleted;
            BrowserView.Source = new Uri(homePageUrl);
        }

        private void CoreWebView2_NewWindowRequested(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
        {
            // Prevent opening in a new window and redirect to the same WebView
            args.Handled = true;
            BrowserView.Source = new Uri(args.Uri);
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

            if (AuthService.CurrentUser != null)
            {
                DataAccess.AddSearchTermToHistory(AuthService.CurrentUser.Username, sender.Text, DateTime.Now);
            }
            else
            {
                Debug.WriteLine("Error: No authenticated user found when saving search term.");
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
        }

        private void ShowHomeScreen()
        {
            IsHomeScreenVisible = true;
            HomeScreenGrid.Visibility = Visibility.Visible;
            BrowserView.Visibility = Visibility.Visible;
            BrowserView.Source = new Uri(homePageUrl);
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
            if (BrowserView.Source != null)
            {
                string currentUrl = BrowserView.Source.ToString();
                if (AuthService.CurrentUser != null)
                {
                    DataAccess.AddSearchTermToHistory(AuthService.CurrentUser.Username, currentUrl, DateTime.Now);
                }
                else
                {
                    Debug.WriteLine("Error: No authenticated user found when saving visited URL.");
                }
            }
        }
    }
}
