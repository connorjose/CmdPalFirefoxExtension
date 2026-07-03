using FirefoxExtension.Commands;
using FirefoxExtension.Data;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FirefoxExtension.Pages
{
    internal sealed partial class SearchBookmarksPage : DynamicListPage
    {
        private static readonly TimeSpan DebounceDelay = TimeSpan.FromMilliseconds(10);
        private static readonly int BookmarkLoadThreshold = 200;
        private static readonly IconInfo BookmarkIcon = IconHelpers.FromRelativePath("Assets\\FirefoxLogo.png");

        private readonly List<ListItem> _items = new();
        private readonly List<BookmarkEntry> _allBookmarks = new();
        private readonly DbService? _dbService;
        private readonly bool _loadAllBookmarks;
        private CancellationTokenSource? _debounce;

        public SearchBookmarksPage(DbService? dbService)
        {
            _dbService = dbService;
            Title = "Search bookmarks";
            Name = "Open";
            _loadAllBookmarks = ShouldLoadAllBookmarks();

            if (_loadAllBookmarks && _dbService is not null)
            {
                _allBookmarks.AddRange(_dbService.GetBookmarkEntries(null, int.MaxValue));
            }

            LoadBookmarkItems(null, BookmarkLoadThreshold);
        }

        private bool ShouldLoadAllBookmarks()
        {
            if (_dbService is null)
            {
                return false;
            }
            return _dbService.BookmarkCount <= BookmarkLoadThreshold;
        }

        public override IListItem[] GetItems()
        {
            return _items.ToArray();
        }

        public override void UpdateSearchText(string oldSearch, string newSearch)
        {
            if (_loadAllBookmarks)
            {
                _items.Clear();
                var filteredList = FilterItems(_allBookmarks, newSearch);
                _items.AddRange(filteredList);
                RaiseItemsChanged(_items.Count);

                return;
            }

            _debounce?.Cancel();
            _debounce?.Dispose();

            var cts = new CancellationTokenSource();
            _debounce = cts;

            IsLoading = true;

            _ = DebouncedSearchAsync(newSearch, cts.Token);
        }

        private async Task DebouncedSearchAsync(string searchTerm, CancellationToken token)
        {
            try
            {
                await Task.Delay(DebounceDelay, token);

                LoadBookmarkItems(searchTerm, BookmarkLoadThreshold);

                if (!token.IsCancellationRequested)
                {
                    RaiseItemsChanged(_items.Count);
                }
            }
            catch (TaskCanceledException)
            {
                // Newer keystroke superseded this one.
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                    IsLoading = false;
                }
            }
        }

        private void LoadBookmarkItems(string? searchTerm, int limit)
        {
            _items.Clear();

            if (_dbService is null)
            {
                _items.Add(new ListItem(new AnonymousCommand(action: () => { })) { Title = "Unable to load Firefox bookmarks", Subtitle = "Firefox profile could not be found." });
                return;
            }

            try
            {
                foreach (var entry in _dbService.GetBookmarkEntries(searchTerm, limit))
                {
                    _items.Add(new BookmarkListItem(entry.Title, entry.Url, BookmarkIcon));
                }
            }
            catch
            {
                _items.Add(new ListItem(new AnonymousCommand(action: () => { })) { Title = "Unable to load Firefox bookmarks", Subtitle = "Check Firefox profile path or permissions." });
            }
        }

        private static IEnumerable<BookmarkListItem> FilterItems(IEnumerable<BookmarkEntry> items, string searchTerm)
        {
            var candidateItems = items
            .Select(entry => (Entry: entry, Item: new BookmarkListItem(entry.Title, entry.Url, BookmarkIcon)))
            .ToList();

            var filtered = ListHelpers.FilterList(
                candidateItems,
                searchTerm,
                (query, candidate) => ListHelpers.ScoreListItem(query, candidate.Item));

            return filtered.Select(x => x.Item);
        }
    }
}
