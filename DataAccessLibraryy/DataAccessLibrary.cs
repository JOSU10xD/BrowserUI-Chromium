using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Windows.Storage;
using System.IO;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DataAccessLibrary
{
    public class DataAccess
    {
        static string dbPath = "dataBase.db";
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
        public static void AddSearchTermToTable(string SearchTerm, DateTime DateSearched, int TermSearchedAmount)
        {
            string db = Path.Combine(ApplicationData.Current.LocalFolder.Path, dbPath);

            //enter db not dbPath
            using (SqliteConnection conn = new SqliteConnection($"FileName={db}"))
            {
                conn.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = conn;

                //Search term is correct
                insertCommand.CommandText = "INSERT INTO searchterms VALUES(NULL, @SearchTerm, @DateSearched, @TermSearchedAmount)";
                insertCommand.Parameters.AddWithValue("@SearchTerm", SearchTerm);
                insertCommand.Parameters.AddWithValue("@DateSearched", DateSearched);
                insertCommand.Parameters.AddWithValue("@TermSearchedAmount", TermSearchedAmount);

                insertCommand.ExecuteReader();

                conn.Close();
            }
        }
    }
}

