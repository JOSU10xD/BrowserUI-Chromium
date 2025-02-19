using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;

namespace BrowserUI.Pages
{
    public sealed partial class NewTab : Page
    {
        private bool IsOnInitialState = true;

        public NewTab()
        {
            this.InitializeComponent();

            UpdateTime();

            // Optional initial homepage (NTP view by default)
            BrowserView.Source = new Uri("https://www.bing.com");

            NewTabSearch.QuerySubmitted += NewTabSearch_QuerySubmitted;
            DataTime.Toggled += DataTimeSwitch_Toggled;

            // Listen to navigation events
            BrowserView.NavigationStarting += BrowserView_NavigationStarting;
            BrowserView.NavigationCompleted += BrowserView_NavigationCompleted;
        }

        // When a URL is searched from the search box
        private void NewTabSearch_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string query = args.QueryText;
            if (!string.IsNullOrWhiteSpace(query))
            {
                if (!query.Contains("."))
                {
                    query = "https://www.google.com/search?q=" + Uri.EscapeDataString(query);
                }
                else if (!query.StartsWith("http"))
                {
                    query = "https://" + query;
                }

                BrowserView.Source = new Uri(query);
            }
        }

        // Handles 'Back' button click
        public void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (BrowserView.CanGoBack)
            {
                BrowserView.GoBack();
            }
            else
            {
                // No history -> Go to initial state
                ResetToInitialState();
            }
        }

        // Handles 'Forward' button click
        public void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (BrowserView.CanGoForward)
            {
                BrowserView.GoForward();
            }
        }

        // Handles 'Refresh' button click
        public void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            BrowserView.Reload();
        }

        // Optional toggle date switch
        private void DataTimeSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (DataTime.IsOn)
            {
                NtpGrid.Visibility = Visibility.Visible;
                UpdateTime();
            }
            else
            {
                NtpGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateTime()
        {
            var now = DateTime.Now;
            NtpTime.Text = now.ToString("HH:mm:ss");
            NtpDate.Text = now.ToString("dddd, MMM dd yyyy");

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                var current = DateTime.Now;
                NtpTime.Text = current.ToString("HH:mm:ss");
                NtpDate.Text = current.ToString("dddd, MMM dd yyyy");
            };
            timer.Start();
        }

        // NAVIGATION EVENTS (CORE LOGIC FOR BACKGROUND HANDLING)

        // When navigation starts, assume we're leaving the initial state
        private void BrowserView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (IsOnInitialState)
            {
                IsOnInitialState = false;
            }
        }

        // When navigation is complete, optionally check if we are on our initial state URL
        private void BrowserView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // Optional: Detect if we are back to the homepage
            // This is useful if you want to show the initial state when you revisit the homepage manually
            var currentUrl = BrowserView.Source?.ToString();

            if (string.IsNullOrEmpty(currentUrl) || currentUrl == "https://www.bing.com") // Change this URL to your NTP if needed
            {
                IsOnInitialState = true;
                ShowInitialStateUI();
            }
        }

        // Reset the entire tab to the initial state (like when closing/reopening a tab)
        private void ResetToInitialState()
        {
            IsOnInitialState = true;
            BrowserView.Source = new Uri("https://www.bing.com"); // Your home page

            ShowInitialStateUI();
        }

        // Show initial search and date UI
        private void ShowInitialStateUI()
        {
            BigGrid.Visibility = Visibility.Visible;
            NtpGrid.Visibility = DataTime.IsOn ? Visibility.Visible : Visibility.Collapsed;
            TabEditBtn.Visibility = Visibility.Visible;
        }

        // Hide initial UI; it will still run in the background
        private void HideInitialStateUI()
        {
            BigGrid.Visibility = Visibility.Collapsed;
            NtpGrid.Visibility = Visibility.Collapsed;
            TabEditBtn.Visibility = Visibility.Collapsed;
        }
    }
}
