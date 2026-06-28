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
            return GetBookmarkEntries().Select(entry => entry.Title ?? entry.Url);
        }

        public IEnumerable<BookmarkEntry> GetBookmarkEntries()
        {
            var bookmarks = new List<BookmarkEntry>();

            using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    SELECT COALESCE(b.title, p.url) AS title, p.url
                    FROM moz_bookmarks b
                    JOIN moz_places p ON b.fk = p.id
                    WHERE b.type = 1
                    ORDER BY b.dateAdded DESC
                ";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var title = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                        var url = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        bookmarks.Add(new BookmarkEntry(title, url));
                    }
                }
            }

            return bookmarks;
        }

        public IEnumerable<string> GetHistory()
        {
            return GetHistoryEntries().Select(entry => entry.Title ?? entry.Url);
        }

        public IEnumerable<HistoryEntry> GetHistoryEntries()
        {
            var historyItems = new List<HistoryEntry>();

            using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    SELECT COALESCE(title, url) AS title, url
                    FROM moz_places
                    WHERE visit_count > 0
                    ORDER BY last_visit_date DESC
                    LIMIT 50
                ";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var title = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                        var url = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        historyItems.Add(new HistoryEntry(title, url));
                    }
                }
            }

            return historyItems;
        }
    }
}
