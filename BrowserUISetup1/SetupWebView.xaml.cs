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
using BrowserUIMultiCore;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BrowserUISetup
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SetupWebView : Page
    {
        public SetupWebView()
        {
            this.InitializeComponent();
        }

        private BrowserUIMultiCore.User GetUser()
        {
            // Check if the user is authenticated.
            if (AuthService.IsUserAuthenticated)
            {
                // Return the authenticated user.
                return AuthService.CurrentUser;
            }

            // If no user is authenticated, return null or handle as needed.
            return null;
        }
        private void StatusTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                // Assuming 'url' and 'selection' have been defined earlier
                string autoSettingValue = toggleSwitch.IsOn ? "1" : "0";

                // Load the user's settings
                Settings userSettings = UserFolderManager.LoadUserSettings(GetUser());


                // Set the 'Auto' setting
                userSettings.StatusBar = autoSettingValue;

                // Save the modified settings back to the user's settings file
                UserFolderManager.SaveUserSettings(GetUser(), userSettings);
            }
        }

        private void BrowserKeys_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch toggleSwitch)
            {
                // Assuming 'url' and 'selection' have been defined earlier
                string autoSettingValue = toggleSwitch.IsOn ? "1" : "0";

                // Load the user's settings
                Settings userSettings = UserFolderManager.LoadUserSettings(GetUser());


                // Set the 'Auto' setting
                userSettings.BrowserKeys = autoSettingValue;

                // Save the modified settings back to the user's settings file
                UserFolderManager.SaveUserSettings(GetUser(), userSettings);
            }
        }

        private void BrowserScripts_Toggled(object sender, RoutedEventArgs e)
        {

            if (sender is ToggleSwitch toggleSwitch)
            {
                // Assuming 'url' and 'selection' have been defined earlier
                string autoSettingValue = toggleSwitch.IsOn ? "1" : "0";

                // Load the user's settings
                Settings userSettings = UserFolderManager.LoadUserSettings(GetUser());


                // Set the 'Auto' setting
                userSettings.BrowserScripts = autoSettingValue;

                // Save the modified settings back to the user's settings file
                UserFolderManager.SaveUserSettings(GetUser(), userSettings);
            }
        }

        private void userag_TextChanged(object sender, TextChangedEventArgs e)
        {
            string blob = userag.Text.ToString();
            if (!string.IsNullOrEmpty(blob))
            {
                // Load the user's settings
                Settings userSettings = UserFolderManager.LoadUserSettings(GetUser());

                userSettings.Useragent = blob;

                // Save the modified settings back to the user's settings file
                UserFolderManager.SaveUserSettings(GetUser(), userSettings);
            }
        }

        private void SetupWebViewBtn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SetupFinish));
        }
    }
}
