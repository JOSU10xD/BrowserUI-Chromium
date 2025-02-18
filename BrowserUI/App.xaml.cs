using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using BrowserUIMultiCore;


namespace BrowserUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        public static string GetUsernameFromCoreFolderPath(string coreFolderPath)
        {
            try
            {
                string usrCoreFilePath = System.IO.Path.Combine(coreFolderPath, "UserCore.json");

                // Check if UsrCore.json exists
                if (File.Exists(usrCoreFilePath))
                {
                    // Read the JSON content from UsrCore.json
                    string jsonContent = File.ReadAllText(usrCoreFilePath);

                    // Deserialize the JSON content into a list of user objects
                    var users = JsonSerializer.Deserialize<List<BrowserUIMultiCore.User>>(jsonContent);

                    if (users != null && users.Count > 0 && !string.IsNullOrWhiteSpace(users[0].Username))
                    {
                        return users[0].Username; // Assuming you want the first user's username
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during file reading or deserialization
                Console.WriteLine("Error reading UsrCore.json: " + ex.Message);
            }

            // Return null or an empty string if the username couldn't be retrieved
            return null;
        }

        public void checknormal()
        {
            string coreFolderPath = UserDataManager.CoreFolderPath;
            string username = GetUsernameFromCoreFolderPath(coreFolderPath);

            if (username != null)
                AuthService.Authenticate(username);
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            if (!Directory.Exists(UserDataManager.CoreFolderPath))
            {
                m_window = new SetupWindow();
            }
            else
            {
                m_window = new MainWindow();
                checknormal();
            }

            m_window.Activate();
        }

        public Window m_window;
    }
}

