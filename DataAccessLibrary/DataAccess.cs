using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Data.Sqlite;

namespace BrowserUIMultiCore
{
    public static class DataAccess
    {
        private static string GetDatabasePath(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty when accessing the database.");
            }

            string dbDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WinUi3UserCore", "Users", username, "Database");
            string dbPath = Path.Combine(dbDirectory, "History.db");

            // Ensure the directory exists before returning the path
            if (!Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
            }

            return dbPath;
        }

        public static void InitialiseDatabase(string username)
        {
            string dbPath = GetDatabasePath(username);

            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));

            using (SqliteConnection conn = new SqliteConnection($"Filename={dbPath}"))
            {
                conn.Open();

                // Ensure history table exists (without shortName)
                string createHistoryTable = @"
        CREATE TABLE IF NOT EXISTS history (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            fullUrl TEXT NOT NULL,
            pageTitle TEXT NOT NULL,
            visitTime TEXT NOT NULL
        )";

                using (SqliteCommand command = new SqliteCommand(createHistoryTable, conn))
                {
                    command.ExecuteNonQuery();
                }
            }
        }


        public static void AddHistoryEntry(string username, string fullUrl, string pageTitle, DateTime visitTime)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                Debug.WriteLine("Error: Username cannot be null or empty when adding a history entry.");
                return;
            }

            string dbPath = GetDatabasePath(username);

            using (SqliteConnection conn = new SqliteConnection($"Filename={dbPath}"))
            {
                conn.Open();

                string createTableCommand = @"
        CREATE TABLE IF NOT EXISTS history (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            fullUrl TEXT NOT NULL,
            pageTitle TEXT,
            visitTime TEXT NOT NULL
        )";

                using (SqliteCommand command = new SqliteCommand(createTableCommand, conn))
                {
                    command.ExecuteNonQuery();
                }

                using (SqliteCommand insertCommand = conn.CreateCommand())
                {
                    insertCommand.CommandText = @"
            INSERT INTO history (fullUrl, pageTitle, visitTime) 
            VALUES (@fullUrl, @pageTitle, @visitTime)";

                    insertCommand.Parameters.AddWithValue("@fullUrl", fullUrl);
                    insertCommand.Parameters.AddWithValue("@pageTitle", pageTitle ?? "No Title");
                    insertCommand.Parameters.AddWithValue("@visitTime", visitTime.ToString("o"));

                    insertCommand.ExecuteNonQuery();
                }
            }
        }


        public static void AddSearchTermToHistory(string username, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                Debug.WriteLine("Error: Username cannot be null or empty when adding a search term.");
                return;
            }

            string dbPath = GetDatabasePath(username);

            using (SqliteConnection conn = new SqliteConnection($"Filename={dbPath}"))
            {
                conn.Open();

                // Ensure the searchterms table exists before inserting data
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
                    VALUES (@searchTerm, CURRENT_TIMESTAMP)";

                    insertCommand.Parameters.AddWithValue("@searchTerm", searchTerm);
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

                using (SqliteCommand clearCommand = new SqliteCommand("DELETE FROM history", conn))
                {
                    clearCommand.ExecuteNonQuery();
                }
            }
        }

        public static List<(int id, string fullUrl, string pageTitle, DateTime visitTime)> GetHistory(string username)
        {
            string dbPath = GetDatabasePath(username);
            List<(int id, string fullUrl, string pageTitle, DateTime visitTime)> historyList = new();

            using (SqliteConnection conn = new SqliteConnection($"Filename={dbPath}"))
            {
                conn.Open();

                // Ensure table exists before querying
                string checkTableCommand = "SELECT name FROM sqlite_master WHERE type='table' AND name='history'";
                using (SqliteCommand checkCommand = new SqliteCommand(checkTableCommand, conn))
                {
                    var result = checkCommand.ExecuteScalar();
                    if (result == null)
                    {
                        Debug.WriteLine("Error: 'history' table does not exist.");
                        return historyList; // Return empty list
                    }
                }

                using (SqliteCommand command = new SqliteCommand("SELECT id, fullUrl, pageTitle, visitTime FROM history ORDER BY visitTime DESC", conn))
                {
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string fullUrl = reader.GetString(1);
                            string pageTitle = reader.IsDBNull(2) ? "No Title" : reader.GetString(2);
                            DateTime visitTime = DateTime.Parse(reader.GetString(3));

                            historyList.Add((id, fullUrl, pageTitle, visitTime));
                        }
                    }
                }
            }

            return historyList;
        }

    }
}
