using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Windows.Storage;
using System.IO;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DataAccessLibrary
{
    public class DataAccess
    {
        static string dbPath = "DataBase.db";
        public static async void InitialiseDatabase()
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync(dbPath, CreationCollisionOption.OpenIfExists);
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, dbPath);

            using (SqliteConnection conn =
                new SqliteConnection($"Filename={dbpath}"))
            {
                conn.Open();

                String searchTermsTableCommand = "CREATE TABLE IF NOT " +
                    "EXISTS searchterms (searchtermID INTEGER PRIMARY KEY," +
                    "SearchTerm VARCHAR(2048) NOT NULL," +
                    "DateSearched DATE, " +
                    "TermSearchedAmount INTEGER)";

                SqliteCommand createtable = new SqliteCommand(searchTermsTableCommand, conn);

                createtable.ExecuteReader();
            }
        }
    }
}

