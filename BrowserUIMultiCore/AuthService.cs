using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BrowserUIMultiCore;

public class AuthService
{
    private static List<User> users;
    private static readonly string UserDataFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WinUi3UserCore", "UserCore.json");

    static AuthService()
    {
        LoadUsersFromJson();
    }

    private static void LoadUsersFromJson()
    {
        try
        {
            string userDatajson = File.ReadAllText(UserDataFilePath);
            users = JsonSerializer.Deserialize<List<User>>(userDatajson);
        }
        catch (Exception ex) 
        {
            Console.WriteLine($"Error loading user data: {ex.Message}");
            users = new List<User>();
            CurrentUser = null;
        }
    }

    public static User CurrentUser { get; private set; }

    public static bool IsUserAuthenticated => CurrentUser != null;

    public static bool Authenticate(string username)
    {
        User authenticatedUser = users.FirstOrDefault(x => x.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (authenticatedUser != null)
        {
            CurrentUser = authenticatedUser;
            return true;
        }

        return false;
    }

    public static void Logout()
    {
        CurrentUser = null;
    }
}
