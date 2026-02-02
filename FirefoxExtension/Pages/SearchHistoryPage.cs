using FirefoxExtension.Commands;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirefoxExtension.Pages
{
    internal sealed partial class SearchHistoryPage : ListPage
    {
        // TODO: Search history subset

        public SearchHistoryPage() 
        {
            Icon = new("\uF147");
            Title = "Search history";
            Name = "Open";

            _items = [new IncrementingListItem(this) { Subtitle = $"Item 0" }];
        }

        public override IListItem[] GetItems()
        {
            return _items.ToArray();
        }

        internal void Increment() 
        {
            _items.Add(new IncrementingListItem(this) { Subtitle = $"Item {_items.Count}" });
            RaiseItemsChanged();
        }

        private List<ListItem> _items;
    }
}
