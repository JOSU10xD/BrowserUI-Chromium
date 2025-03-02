using Microsoft.UI.Windowing;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT.Interop;
using BrowserUICore.Helpers;
using Windows.UI.WindowManagement;
using BrowserUI.Controls;
using BrowserUI.Pages;
using Windows.UI.WebUI;
using Microsoft.Web.WebView2.Core;
using Windows.Graphics.Imaging;
using System.Net.Http;
using System.Text.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.System;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Pickers;
using BrowserUIMultiCore;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BrowserUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    
    public sealed partial class MainWindow : Window
    {
        private Microsoft.UI.Windowing.AppWindow appWindow;
        private Microsoft.UI.Windowing.AppWindowTitleBar titleBar;
        private List<string> snippets = new();
        private string editingSnippet = null;
        private readonly string snippetsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ClipboardSnippets.json");
        public MainWindow()
        {
            this.InitializeComponent();
            LoadSnippets();
            NewSnippetBox.IsEnabled = true;  // Ensure the textbox is enabled
            NewSnippetBox.Focus(FocusState.Programmatic);
            LoadUserInfo();     
            Tabs.TabItems.Add(CreateNewTab(typeof(NewTab)));
            TitleTop();  // Load saved snippets when app starts
        }

        private void ClipBoardButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSnippets();
            ClipboardFlyout.ShowAt((FrameworkElement)sender);
            NewSnippetBox.IsEnabled = true;  // Ensure the textbox is enabled
            NewSnippetBox.Focus(FocusState.Programmatic); // Ensure TextBox is focused
        }

        // Load saved snippets from local storage
        private void LoadSnippets()
        {
            if (File.Exists(snippetsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(snippetsFilePath);
                    snippets = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
                }
                catch
                {
                    snippets = new List<string>();
                }
            }
            else
            {
                snippets = new List<string>();
            }
            RefreshSnippetsList();
        }

        // Refresh ListView Data
        private void RefreshSnippetsList()
        {
            SnippetsList.ItemsSource = null; // Reset binding
            SnippetsList.ItemsSource = snippets;
        }

        // Add or Edit a Snippet
        private void AddSnippet_Click(object sender, RoutedEventArgs e)
        {
            string newSnippet = NewSnippetBox.Text.Trim();
            if (!string.IsNullOrEmpty(newSnippet))
            {
                if (editingSnippet != null)
                {
                    // If editing, replace the old snippet
                    int index = snippets.IndexOf(editingSnippet);
                    if (index >= 0)
                    {
                        snippets[index] = newSnippet;
                    }
                    editingSnippet = null; // Reset edit mode
                }
                else
                {
                    // Otherwise, add new snippet
                    snippets.Add(newSnippet);
                }

                NewSnippetBox.Text = ""; // Clear input field
                RefreshSnippetsList();
            }
        }

        // Save snippets to local storage
        private void SaveSnippets_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string json = JsonSerializer.Serialize(snippets, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(snippetsFilePath, json);
            }
            catch (Exception ex)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Failed to save snippets: " + ex.Message,
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                _ = dialog.ShowAsync();
            }
        }

        // Edit a snippet
        private void EditSnippet_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is string snippet)
            {
                NewSnippetBox.Text = snippet;
                editingSnippet = snippet;
                NewSnippetBox.Focus(FocusState.Programmatic);
            }
        }

        // Delete a snippet
        private void DeleteSnippet_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is string snippet)
            {
                snippets.Remove(snippet);
                RefreshSnippetsList();
            }
        }
        private void LoadUserInfo()
        {
            try
            {
                // Load user data from JSON
                var userDataResult = UserDataManager.LoadUsers(); // Get the result object
                List<BrowserUIMultiCore.User> users = userDataResult.Users; // Extract the list

                if (users != null && users.Count > 0)
                {
                    AuthService.SetCurrentUser(users[0]); // Set the current user
                }

                // Display the username
                if (AuthService.CurrentUser != null)
                {
                    UsernameTextBlock.Text = AuthService.CurrentUser.Username;
                }
                else
                {
                    UsernameTextBlock.Text = "Guest";
                }
            }
            catch (Exception ex)
            {
                UsernameTextBlock.Text = "Error";
                Console.WriteLine($"Failed to load user info: {ex.Message}");
            }
        }

        private IBrowserTab GetCurrentTab()
        {
            if (Tabs.SelectedItem is FireBrowserTabViewItem selectedTab &&
                selectedTab.Content is Frame frame &&
                frame.Content is IBrowserTab browserTab)
            {
                return browserTab;
            }

            return null;
        }

        public void BackButton_Click(object sender, RoutedEventArgs e)
        {
            GetCurrentTab()?.BackButton_Click(sender, e);
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            GetCurrentTab()?.ForwardButton_Click(sender, e);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            GetCurrentTab()?.RefreshButton_Click(sender, e);
        }

        public void ScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
            GetCurrentTab()?.CaptureScreenshot(sender, e);
        }
        private void UrlBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (UrlBox.FindName("PART_EditableTextBox") is TextBox textBox)
            {
                textBox.TextChanged += UrlBox_TextChanged;
            }
        }
        private async void UrlBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = UrlBox.Text;

            if (!string.IsNullOrWhiteSpace(query))
            {
                try
                {
                    using HttpClient client = new();
                    string url = $"https://suggestqueries.google.com/complete/search?client=firefox&q={Uri.EscapeDataString(query)}";
                    var response = await client.GetStringAsync(url);

                    // Parse JSON response
                    var suggestions = JsonSerializer.Deserialize<List<object>>(response);
                    if (suggestions != null && suggestions.Count > 1 && suggestions[1] is JsonElement suggestionArray)
                    {
                        var searchSuggestions = suggestionArray.EnumerateArray().Select(x => x.GetString()).Where(x => x != null).ToList();
                        UrlBox.ItemsSource = new ObservableCollection<string>(searchSuggestions);
                    }
                }
                catch (Exception ex)
                {
                    // Handle errors (e.g., network issues)
                    Console.WriteLine($"Error fetching suggestions: {ex.Message}");
                }
            }
        }
        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            string input = UrlBox.Text;
            GetCurrentTab()?.GoButton_Click(sender, e, input);
        }
        private void Tabs_AddTabButtonClick(TabView sender, object args)
        {
            Tabs.TabItems.Add(CreateNewTab(typeof(NewTab)));
        }

        private void Tabs_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            if (args.Item is FireBrowserTabViewItem tab && tab.Content is Frame frame && frame.Content is NewTab1 newTabPage)
            {
                newTabPage.Dispose(); // Dispose of WebView2 resources
            }
            Tabs.TabItems.Remove(args.Item);
        }

        private async void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            EditBookmarkDialog.XamlRoot = this.Content.XamlRoot; // Required for WinUI 3
            await EditBookmarkDialog.ShowAsync();
        }

        #region TitleBar

        public void TitleTop()
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            Microsoft.UI.WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.SetIcon("gridlogo.ico");


            if (!Microsoft.UI.Windowing.AppWindowTitleBar.IsCustomizationSupported())
            {
                throw new Exception("Unsupported OS version.");
            }

            titleBar = appWindow.TitleBar;
            titleBar.ExtendsContentIntoTitleBar = true;
            var btnColor = Colors.Transparent;
            titleBar.BackgroundColor = btnColor;
            titleBar.ButtonBackgroundColor = btnColor;
            titleBar.InactiveBackgroundColor = btnColor;
            titleBar.ButtonInactiveBackgroundColor = btnColor;
            titleBar.ButtonHoverBackgroundColor = btnColor;
        }
        private double GetScaleAdjustment()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            Microsoft.UI.WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            DisplayArea displayArea = DisplayArea.GetFromWindowId(wndId, DisplayAreaFallback.Primary);
            IntPtr hMonitor = Win32Interop.GetMonitorFromDisplayId(displayArea.DisplayId);

            int result = Windowing.GetDpiForMonitor(hMonitor, Windowing.Monitor_DPI_Type.MDT_Default_DPI, out uint dpiX, out _);

            if (result != 0)
            {
                throw new Exception("Could not get DPI");
            }

            return dpiX / 96.0; // Simplified calculation
        }

        private void Apptitlebar_LayoutUpdated(object sender, object e)
        {
            double scaleAdjustment = GetScaleAdjustment();
            Apptitlebar.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var customDragRegionPosition = Apptitlebar.TransformToVisual(null).TransformPoint(new Point(0, 0));

            var dragRectsList = new List<Windows.Graphics.RectInt32>();

            for (int i = 0; i < 2; i++)
            {
                var dragRect = new Windows.Graphics.RectInt32
                {
                    X = (int)((customDragRegionPosition.X + (i * Apptitlebar.ActualWidth / 2)) * scaleAdjustment),
                    Y = (int)(customDragRegionPosition.Y * scaleAdjustment),
                    Height = (int)((Apptitlebar.ActualHeight - customDragRegionPosition.Y) * scaleAdjustment),
                    Width = (int)((Apptitlebar.ActualWidth / 2) * scaleAdjustment)
                };

                dragRectsList.Add(dragRect);
            }

            var dragRects = dragRectsList.ToArray();

            if (appWindow.TitleBar != null)
            {
                appWindow.TitleBar.SetDragRectangles(dragRects);
            }
        }

        private void Tabs_Loaded(object sender, RoutedEventArgs e)
        {
            Apptitlebar.SizeChanged += Apptitlebar_SizeChanged;
        }

        private void Apptitlebar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double scaleAdjustment = GetScaleAdjustment();
            Apptitlebar.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var customDragRegionPosition = Apptitlebar.TransformToVisual(null).TransformPoint(new Point(0, 0));

            var dragRects = new Windows.Graphics.RectInt32[2];

            for (int i = 0; i < 2; i++)
            {
                dragRects[i] = new Windows.Graphics.RectInt32
                {
                    X = (int)((customDragRegionPosition.X + (i * Apptitlebar.ActualWidth / 2)) * scaleAdjustment),
                    Y = (int)(customDragRegionPosition.Y * scaleAdjustment),
                    Height = (int)((Apptitlebar.ActualHeight - customDragRegionPosition.Y) * scaleAdjustment),
                    Width = (int)((Apptitlebar.ActualWidth / 2) * scaleAdjustment)
                };
            }

            appWindow.TitleBar?.SetDragRectangles(dragRects);
        }

        #endregion

        #region Tabs

        public class Passer
        {
            public FireBrowserTabViewItem Tab { get; set; }
            public FireBrowserTabViewContainer TabView { get; set; }
            public object Param { get; set; }

            //public ToolbarViewModel ViewModel { get; set; }
            public string UserName { get; set; }
        }

        public FireBrowserTabViewItem CreateNewTab(Type page = null, object param = null, int index = -1)
        {
            if (index == -1) index = Tabs.TabItems.Count;

            var newItem = new FireBrowserTabViewItem
            {
                Header = "GRID - HomePage",
                IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource { Symbol = Symbol.Home },
                Style = (Style)Microsoft.UI.Xaml.Application.Current.Resources["FloatingTabViewItemStyle"],
                Height = 40
            };


            Passer passer = new()
            {
                Tab = newItem,
                TabView = Tabs,
                Param = param,
            };

            double margin = ClassicToolBar.Height;
            var frame = new Frame
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(0, margin, 0, 0)
            };

            if (page != null)
            {
                frame.Navigate(page, passer);
            }

            var toolTip = new ToolTip();
            toolTip.Content = new Grid
            {
                Children =
        {
            new Microsoft.UI.Xaml.Controls.Image(),
            new TextBlock()
        }
            };
            ToolTipService.SetToolTip(newItem, toolTip);

            newItem.Content = frame;
            return newItem;
        }

        #endregion
    }
}
