using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.UI.Xaml;
using BrowserUIMultiCore;

namespace BrowserUI
{
    public partial class App : Application
    {
        public static MainWindow MainWindow { get; private set; }
        private Window m_window;

        public App()
        {
            this.InitializeComponent();
        }

        public static string GetUsernameFromCoreFolderPath(string coreFolderPath)
        {
            try
            {
                string usrCoreFilePath = System.IO.Path.Combine(coreFolderPath, "UserCore.json");

                if (File.Exists(usrCoreFilePath))
                {
                    string jsonContent = File.ReadAllText(usrCoreFilePath);
                    var users = JsonSerializer.Deserialize<List<BrowserUIMultiCore.User>>(jsonContent);

                    if (users != null && users.Count > 0 && !string.IsNullOrWhiteSpace(users[0].Username))
                    {
                        return users[0].Username;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading UserCore.json: " + ex.Message);
            }
            return null;
        }

        public void checknormal()
        {
            string coreFolderPath = UserDataManager.CoreFolderPath;
            string username = GetUsernameFromCoreFolderPath(coreFolderPath);

            if (username != null)
                AuthService.Authenticate(username);
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            if (!Directory.Exists(UserDataManager.CoreFolderPath))
            {
                m_window = new SetupWindow();
                m_window.Activate();
            }
            else
            {
                MainWindow = new MainWindow();
                checknormal();
                MainWindow.Activate();
            }
        }
    }
}
