using FirefoxExtension.Commands;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FirefoxExtension.Data;
using System.Threading;

namespace FirefoxExtension.Pages
{
    internal sealed partial class SearchHistoryPage : DynamicListPage
    {
        // TODO: Consider making this configurable in the future, but for now, we will limit the number of history items to 100 to avoid performance issues.
        private static readonly int HistoryLimit = 100;
        private static readonly IconInfo BookmarkIcon = IconHelpers.FromRelativePath("Assets\\FirefoxLogo.png");
        private static readonly TimeSpan DebounceDelay = TimeSpan.FromMilliseconds(10);

        private readonly List<ListItem> _items = new();
        private readonly DbService? _dbService;
        private CancellationTokenSource? _debounce;

        public SearchHistoryPage(DbService? dbService) 
        {
            _dbService = dbService;
            Title = "Search history";
            Name = "Open";

            LoadHistoryItems(null, 50);
        }

        public override IListItem[] GetItems()
        {
            return _items.ToArray();
        }

        public override void UpdateSearchText(string oldSearch, string newSearch)
        {
            _debounce?.Cancel();
            _debounce?.Dispose();

            var cts = new CancellationTokenSource();
            _debounce = cts;

            IsLoading = true;

            _ = DebouncedSearchAsync(newSearch, cts.Token);
        }

        // TODO: Move to shared helper method
        private async Task DebouncedSearchAsync(string searchTerm, CancellationToken token) 
        {
            try
            {
                await Task.Delay(DebounceDelay, token);

                LoadHistoryItems(searchTerm, HistoryLimit);

                if (!token.IsCancellationRequested)
                {
                    RaiseItemsChanged(_items.Count);
                }
            }
            catch (TaskCanceledException)
            {
                // New keystroke - do nothing
            }
            finally 
            {
                if (!token.IsCancellationRequested)
                { 
                    IsLoading = false;
                }
            }
        }

        private void LoadHistoryItems(string? searchTerm, int limit)
        {
            _items.Clear();

            if (_dbService is null)
            {
                _items.Add(new ListItem(new AnonymousCommand(action: () => { })) { Title = "Unable to load Firefox history", Subtitle = "Firefox profile could not be found." });
                return;
            }

            try
            {
                foreach (var entry in _dbService.GetHistoryEntries(searchTerm, limit))
                {
                    _items.Add(new HistoryListItem(entry.Title, entry.Url, BookmarkIcon));
                }
            }
            catch
            {
                _items.Add(new ListItem(new AnonymousCommand(action: () => { })) { Title = "Unable to load Firefox history", Subtitle = "Check Firefox profile path or permissions." });
            }
        }
    }
}
