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

        public int BookmarkCount
        {
            get
            {
                using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT COUNT(1) FROM moz_bookmarks";
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public IEnumerable<BookmarkEntry> GetBookmarkEntries(string? searchTerm = null, int limit = 200)
        {
            var bookmarks = new List<BookmarkEntry>();

            using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
            {
                connection.Open();
                var command = connection.CreateCommand();

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    command.CommandText =
                    @"
                        SELECT COALESCE(b.title, p.url) AS title, p.url
                        FROM moz_bookmarks b
                        JOIN moz_places p ON b.fk = p.id
                        WHERE b.type = 1
                        ORDER BY b.dateAdded DESC
                        LIMIT @limit
                    ";
                    command.Parameters.AddWithValue("@limit", limit);
                }
                else
                {
                    command.CommandText =
                    @"
                        SELECT COALESCE(b.title, p.url) AS title, p.url
                        FROM moz_bookmarks b
                        JOIN moz_places p ON b.fk = p.id
                        WHERE b.type = 1
                            AND (b.title LIKE @contains ESCAPE '\' OR p.url LIKE @contains ESCAPE '\')
                        ORDER BY b.dateAdded DESC
                        LIMIT @limit
                    ";
                    var escaped = searchTerm.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
                    command.Parameters.AddWithValue("@contains", "%" + escaped + "%");
                    command.Parameters.AddWithValue("@limit", limit);
                }

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

        public IEnumerable<HistoryEntry> GetHistoryEntries(string? searchTerm = null, int limit = 100)
        {
            var historyItems = new List<HistoryEntry>();

            using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
            {
                connection.Open();
                var command = connection.CreateCommand();

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    command.CommandText =
                    @"
                        SELECT COALESCE(title, url) AS title, url
                        FROM moz_places
                        WHERE visit_count > 0
                        ORDER BY frecency DESC
                        LIMIT @limit
                    ";
                    command.Parameters.AddWithValue("@limit", 50);
                }
                else
                {
                    // Cheap SQL-side prefilter: keep anything that contains the
                    // characters in order somewhere (loose net), let the host's
                    // fuzzy matcher do the real scoring/highlighting on the result.
                    command.CommandText =
                    @"
                        SELECT COALESCE(title, url) AS title, url
                        FROM moz_places
                        WHERE visit_count > 0
                            AND (title LIKE @contains ESCAPE '\' OR url LIKE @contains ESCAPE '\')
                        ORDER BY frecency DESC
                        LIMIT @limit
                    ";
                    var escaped = searchTerm.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
                    command.Parameters.AddWithValue("@contains", "%" + escaped + "%");
                    command.Parameters.AddWithValue("@limit", limit);
                }

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
