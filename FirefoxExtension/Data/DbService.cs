using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;

namespace FirefoxExtension.Data
{
    public class DbService
    {
        private readonly string _dbPath;
        public IEnumerable<string> Bookmarks => GetBookmarks();
        public IEnumerable<string> History => GetHistory();

        public DbService(string dbPath)
        {
            if (string.IsNullOrEmpty(dbPath))
            {
                throw new ArgumentException("Database path cannot be null or empty.", nameof(dbPath));
            }

            _dbPath = dbPath;
        }

        public IEnumerable<string> GetBookmarks()
        {
            var bookmarks = new List<string>();

            using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    SELECT b.title
                    FROM moz_bookmarks b
                    JOIN moz_places p ON b.fk = p.id
                    WHERE b.type = 1
                ";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bookmarks.Add(reader.GetString(0));
                    }
                }
            }

            return bookmarks;
        }

        public IEnumerable<string> GetHistory()
        {
            var historyItems = new List<string>();

            using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    SELECT title
                    FROM moz_places
                    WHERE visit_count > 0
                    ORDER BY last_visit_date DESC
                    LIMIT 100
                ";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        historyItems.Add(reader.GetString(0));
                    }
                }
            }

            return historyItems;
        }
    }
}
