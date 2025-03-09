using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;
using Windows.Storage;
using BrowserUIMultiCore;

namespace DataAccessLibrary
{
    public class DataAccess
    {
        public static void InitialiseDatabase(string username)
        {
            string dbPath = GetDatabasePath(username);

            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)); // Ensure the Database folder exists

            using (SqliteConnection conn = new SqliteConnection($"Filename={dbPath}"))
            {
                conn.Open();

                string createTableCommand = @"
                    CREATE TABLE IF NOT EXISTS history (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        url TEXT NOT NULL,
                        title TEXT,
                        visit_time DATETIME DEFAULT CURRENT_TIMESTAMP,
                        visit_count INTEGER NOT NULL DEFAULT 1
                    )";

                SqliteCommand createTable = new SqliteCommand(createTableCommand, conn);
                createTable.ExecuteNonQuery();
            }
        }

        public static void AddHistoryEntry(string username, string url, string title)
        {
            string dbPath = GetDatabasePath(username);

            using (SqliteConnection conn = new SqliteConnection($"Filename={dbPath}"))
            {
                conn.Open();

                SqliteCommand insertCommand = new SqliteCommand(@"
                    INSERT INTO history (url, title, visit_time, visit_count)
                    VALUES (@url, @title, CURRENT_TIMESTAMP, 1)", conn);

                insertCommand.Parameters.AddWithValue("@url", url);
                insertCommand.Parameters.AddWithValue("@title", title ?? "Untitled Page");

                insertCommand.ExecuteNonQuery();
            }
        }

        public static List<(string Url, string Title, DateTime VisitTime, int VisitCount)> GetHistory(string username)
        {
            string dbPath = GetDatabasePath(username);
            List<(string Url, string Title, DateTime VisitTime, int VisitCount)> history = new();

            using (SqliteConnection conn = new SqliteConnection($"Filename={dbPath}"))
            {
                conn.Open();

                SqliteCommand selectCommand = new SqliteCommand("SELECT url, title, visit_time, visit_count FROM history ORDER BY visit_time DESC", conn);
                using (SqliteDataReader reader = selectCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        history.Add((
                            reader.GetString(0),  // URL
                            reader.GetString(1),  // Title
                            reader.GetDateTime(2), // Visit Time
                            reader.GetInt32(3)    // Visit Count
                        ));
                    }
                }
            }

            return history;
        }

        public static void AddSearchTermToHistory(string username, string searchTerm, DateTime dateSearched)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                Debug.WriteLine("Error: Username cannot be null or empty when adding a search term.");
                return;
            }

            string dbPath = Path.Combine(UserDataManager.CoreFolderPath, "Users", username, "Database", "History.db");

            using (SqliteConnection conn = new SqliteConnection($"Filename={dbPath}"))
            {
                conn.Open();

                string createTableCommand = @"
            CREATE TABLE IF NOT EXISTS searchterms (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                searchTerm TEXT NOT NULL,
                dateSearched TEXT NOT NULL
            )";

                using (SqliteCommand command = new SqliteCommand(createTableCommand, conn))
                {
                    command.ExecuteNonQuery();
                }

                using (SqliteCommand insertCommand = conn.CreateCommand())
                {
                    insertCommand.CommandText = @"
                INSERT INTO searchterms (searchTerm, dateSearched) 
                VALUES (@searchTerm, @dateSearched)";

                    insertCommand.Parameters.AddWithValue("@searchTerm", searchTerm);
                    insertCommand.Parameters.AddWithValue("@dateSearched", dateSearched.ToString("o"));

                    insertCommand.ExecuteNonQuery();
                }
            }
        }

        public static void ClearHistory(string username)
        {
            string dbPath = GetDatabasePath(username);

            using (SqliteConnection conn = new SqliteConnection($"Filename={dbPath}"))
            {
                conn.Open();

                SqliteCommand clearCommand = new SqliteCommand("DELETE FROM history", conn);
                clearCommand.ExecuteNonQuery();
            }
        }

        private static string GetDatabasePath(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty when accessing the database.");
            }

            return Path.Combine(UserDataManager.CoreFolderPath, UserDataManager.UsersFolderPath, username, "Database", "History.db");
        }
    }
}
