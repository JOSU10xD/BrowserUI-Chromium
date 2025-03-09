using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BrowserUIMultiCore;


namespace BrowserUISetup
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SetupUser : Page
    {
        public SetupUser()
        {
            this.InitializeComponent();
        }

        private async void Create_Click(object sender, RoutedEventArgs e)
        {
            // Show loading animation
            LoadingAnimation.Visibility = Visibility.Visible;
            LoadingAnimation.IsActive = true;

            // Create user
            await CreateUserOnStartup();

            // Wait for 2 seconds before navigating
            await Task.Delay(2000);

            // Hide loading animation before navigating
            LoadingAnimation.IsActive = false;
            LoadingAnimation.Visibility = Visibility.Collapsed;

            // Navigate to next page
            Frame.Navigate(typeof(SetupUi));
        }

        private async Task CreateUserOnStartup()
        {
            // Create a new user object with a unique username.
            User newUser = new User
            {
                Username = UserName.Text, // Generate a unique username.                                                              // Add other user properties as needed.
            };

            // Create a list of users and add the new user to it.
            List<User> users = new List<User>();
            users.Add(newUser);

            // Create the user folders.
            UserFolderManager.CreateUserFolders(newUser);

            // Save the list of users to the JSON file.
            UserDataManager.SaveUsers(users);
            //AuthService.InitAuthService();
            // Authenticate the new user (if needed).
            AuthService.Authenticate(newUser.Username);
        }
    }
}
