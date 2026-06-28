using FirefoxExtension.Commands;
using FirefoxExtension.Data;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirefoxExtension.Pages
{
    internal sealed partial class SearchBookmarksPage : ListPage
    {
        private readonly List<ListItem> _items = new();
        private readonly DbService? _dbService;
        private static readonly IconInfo BookmarkIcon = IconHelpers.FromRelativePath("Assets\\FirefoxLogo.png");

        public SearchBookmarksPage(DbService? dbService)
        {
            _dbService = dbService;
            Title = "Search bookmarks";
            Name = "Open";

            LoadBookmarkItems();
        }

        public override IListItem[] GetItems()
        {
            return _items.ToArray();
        }

        private void LoadBookmarkItems()
        {
            _items.Clear();

            if (_dbService is null)
            {
                _items.Add(new ListItem(new AnonymousCommand(action: () => { })) { Title = "Unable to load Firefox bookmarks", Subtitle = "Firefox profile could not be found." });
                return;
            }

            try
            {
                foreach (var entry in _dbService.GetBookmarkEntries())
                {
                    _items.Add(new Commands.BookmarkListItem(entry.Title, entry.Url, BookmarkIcon));
                }
            }
            catch
            {
                _items.Add(new ListItem(new AnonymousCommand(action: () => { })) { Title = "Unable to load Firefox bookmarks", Subtitle = "Check Firefox profile path or permissions." });
            }
        }
    }
}
