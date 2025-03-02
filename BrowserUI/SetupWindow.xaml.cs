using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading;

namespace BrowserUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SetupWindow : Window
    {
        private AppWindow appWindow;
        private AppWindowTitleBar titleBar;
        public SetupWindow()
        {
            this.InitializeComponent();

            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);

            appWindow = AppWindow.GetFromWindowId(windowId);

            //copy from mainwindow.xaml


            if (!AppWindowTitleBar.IsCustomizationSupported())
            {
                // Why? Because I don't care
                throw new Exception("Unsupported OS version.");
            }
            else
            {
                titleBar = appWindow.TitleBar;
                titleBar.ExtendsContentIntoTitleBar = true;
                var btnColor = Colors.Transparent;
                titleBar.BackgroundColor = btnColor;
                titleBar.ButtonBackgroundColor = btnColor;
                titleBar.InactiveBackgroundColor = btnColor;
                titleBar.ButtonInactiveBackgroundColor = btnColor;
            }
        }

        private void setup_Loaded(object sender, RoutedEventArgs e)
        {
            setup.Navigate(typeof(BrowserUISetup.SetupInit));
        }
    }
}
