using FirefoxExtension.Pages;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirefoxExtension.Commands
{
    internal sealed partial class IncrementingListItem : ListItem
    {
        public IncrementingListItem(SearchHistoryPage page) : base(new NoOpCommand())
        {
            _page = page;
            Command = new AnonymousCommand(action: _page.Increment) { Result = CommandResult.KeepOpen() };
        }

        private SearchHistoryPage _page;
    }
}
