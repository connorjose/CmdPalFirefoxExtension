// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using FirefoxExtension.Data;
using FirefoxExtension.Pages;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace FirefoxExtension;

internal sealed partial class FirefoxExtensionPage : ListPage
{
    private readonly IconInfo FirefoxIcon = IconHelpers.FromRelativePath("Assets\\FirefoxLogo.png");
    private readonly DbService? _dbService;
    public FirefoxExtensionPage(DbService? dbService)
    {
        _dbService = dbService;
        Icon = FirefoxIcon;
        Title = "Firefox Extension";
        Name = "Open";
    }

    public override IListItem[] GetItems()
    {
        return [
            new ListItem(new SearchHistoryPage(_dbService)) { Title = "Search History", Subtitle = "Search Firefox History", Icon = FirefoxIcon },
            new ListItem(new SearchBookmarksPage(_dbService)) { Title = "Search Bookmarks", Subtitle = "Search Firefox Bookmarks", Icon = FirefoxIcon },
            new ListItem(new SearchTabsPage()) { Title = "Search Tabs", Subtitle = "Search Firefox Tabs", Icon = FirefoxIcon }
        ];
    }
}
