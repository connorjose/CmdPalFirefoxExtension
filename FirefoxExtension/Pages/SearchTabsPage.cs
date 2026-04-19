using System;
using System.Collections.Generic;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace FirefoxExtension.Pages;

internal sealed partial class SearchTabsPage : ListPage
{

    public SearchTabsPage()
    {
        Icon = new("\uEAF3");
        Title = "Search Tabs";
        Name = "Open";
    }

}