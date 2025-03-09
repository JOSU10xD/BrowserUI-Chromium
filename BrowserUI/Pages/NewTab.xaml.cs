using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Microsoft.UI.Dispatching;
using System.Diagnostics;
using BrowserUIMultiCore;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BrowserUI.Pages
{
    public sealed partial class NewTab : Page
    {
        private bool IsHomeScreenVisible = true;
        private DispatcherTimer timer;
        private string homePageUrl = "https://www.google.com"; // Set your homepage URL
        private HashSet<string> visitedUrls = new HashSet<string>(); // To store already recorded URLs

        public NewTab()
        {
            this.InitializeComponent();
            InitializeTime();
            InitializeWebView();
        }

        private void InitializeTime()
        {
            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, e) =>
            {
                DateTime now = DateTime.Now;
                NtpTime.Text = now.ToString("HH:mm:ss");
                NtpDate.Text = now.ToString("dddd, MMM dd yyyy");
            };
            timer.Start();
        }

        private async void InitializeWebView()
        {
            await BrowserView.EnsureCoreWebView2Async();

            if (BrowserView.CoreWebView2 == null)
            {
                Debug.WriteLine("Error: WebView2 failed to initialize.");
                return;
            }

            BrowserView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            BrowserView.NavigationCompleted += BrowserView_NavigationCompleted;
            BrowserView.CoreWebView2.SourceChanged += CoreWebView2_SourceChanged;
            BrowserView.CoreWebView2.HistoryChanged += CoreWebView2_HistoryChanged;
        }

        private async void CoreWebView2_SourceChanged(CoreWebView2 sender, CoreWebView2SourceChangedEventArgs args)
        {
            await StorePageDetails();
        }

        private async void CoreWebView2_HistoryChanged(CoreWebView2 sender, object args)
        {
            await StorePageDetails();
        }

        private async Task StorePageDetails()
        {
            if (BrowserView.Source == null || BrowserView.CoreWebView2 == null)
                return;

            try
            {
                await Task.Delay(500); // Ensure page title is available
                string currentUrl = BrowserView.Source.ToString();

                if (visitedUrls.Contains(currentUrl))
                    return; // Prevent duplicate storage

                visitedUrls.Add(currentUrl);

                string title = BrowserView.CoreWebView2.DocumentTitle;
                if (string.IsNullOrWhiteSpace(title) || title == "Untitled")
                {
                    title = ExtractTitleFromUrl(currentUrl);
                }

                if (AuthService.CurrentUser != null)
                {
                    DataAccess.AddHistoryEntry(AuthService.CurrentUser.Username, currentUrl, title, DateTime.Now);
                }
                else
                {
                    Debug.WriteLine("Error: No authenticated user found when saving visited URL.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error storing page details: {ex.Message}");
            }
        }

        private void CoreWebView2_NewWindowRequested(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
        {
            args.Handled = true;
            BrowserView.Source = new Uri(args.Uri);
        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string query = args.QueryText?.Trim();
            if (string.IsNullOrEmpty(query))
                return;

            string url = ParseSearchOrUrl(query);
            NavigateToBrowser(url);

            if (AuthService.CurrentUser != null)
            {
                DataAccess.AddHistoryEntry(AuthService.CurrentUser.Username, url, query, DateTime.Now);
            }
            else
            {
                Debug.WriteLine("Error: No authenticated user found when saving search term.");
            }
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

        private string ParseSearchOrUrl(string query)
        {
            if (Uri.TryCreate(query, UriKind.Absolute, out Uri validUri) &&
                (validUri.Scheme == Uri.UriSchemeHttp || validUri.Scheme == Uri.UriSchemeHttps))
            {
                return query;
            }
            else if (query.Contains(".") && !query.Contains(" "))
            {
                return query.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? query : $"https://{query}";
            }
            else
            {
                return $"https://www.bing.com/search?q={Uri.EscapeDataString(query)}"; // Change to Google if needed
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
            BrowserView?.CoreWebView2?.Stop();
            BrowserView?.Close();
            BrowserView = null;
        }

        private async void BrowserView_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            if (BrowserView.Source == null || BrowserView.CoreWebView2 == null)
                return;

            try
            {
                await Task.Delay(500); // Ensure page title is loaded
                string currentUrl = BrowserView.Source.ToString();

                if (visitedUrls.Contains(currentUrl))
                    return;

                visitedUrls.Add(currentUrl);

                string title = BrowserView.CoreWebView2.DocumentTitle;
                if (string.IsNullOrWhiteSpace(title) || title == "Untitled")
                {
                    title = ExtractTitleFromUrl(currentUrl);
                }

                if (AuthService.CurrentUser != null)
                {
                    DataAccess.AddHistoryEntry(AuthService.CurrentUser.Username, currentUrl, title, DateTime.Now);
                }
                else
                {
                    Debug.WriteLine("Error: No authenticated user found when saving visited URL.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error storing page details after navigation: {ex.Message}");
            }
        }

        private string ExtractTitleFromUrl(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                string host = uri.Host.Replace("www.", "");
                string path = uri.AbsolutePath.Trim('/');
                string title = host.ToUpper();

                if (!string.IsNullOrEmpty(path))
                {
                    title += " - " + path.Replace("-", " ").Replace("/", " ");
                }

                return title;
            }
            catch
            {
                return "No Title";
            }
        }

        private string GetShortName(string url)
        {
            Uri uri = new Uri(url);
            return uri.Host.Replace("www.", "").Split('.')[0].ToUpper();
        }
    }
}
