using FirefoxExtension.Commands;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirefoxExtension.Data;

namespace FirefoxExtension.Pages
{
    internal sealed partial class SearchHistoryPage : ListPage
    {
        private readonly List<ListItem> _items = new();
        private readonly DbService? _dbService;

        public SearchHistoryPage(DbService? dbService) 
        {
            _dbService = dbService;
            Title = "Search history";
            Name = "Open";

            LoadHistoryItems();
        }

        public override IListItem[] GetItems()
        {
            return _items.ToArray();
        }

        private void LoadHistoryItems()
        {
            _items.Clear();

            if (_dbService is null)
            {
                _items.Add(new ListItem(new AnonymousCommand(action: () => { })) { Title = "Unable to load Firefox history", Subtitle = "Firefox profile could not be found." });
                return;
            }
                

            try
            {

                foreach (var entry in _dbService.GetHistoryEntries())
                {
                    _items.Add(new HistoryListItem(entry.Title, entry.Url, IconHelpers.FromRelativePath("Assets\\FirefoxLogo.png")));
                }
            }
            catch
            {
                _items.Add(new ListItem(new AnonymousCommand(action: () => { })) { Title = "Unable to load Firefox history", Subtitle = "Check Firefox profile path or permissions." });
            }
        }
    }
}
