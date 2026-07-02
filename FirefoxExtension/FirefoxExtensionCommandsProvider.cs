// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using FirefoxExtension.Data;
using FirefoxExtension.Helpers;
using System;
using FirefoxExtension.Pages;


namespace FirefoxExtension;

public partial class FirefoxExtensionCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;
    private DbService? _dbService;
    private readonly IconInfo ExtensionIcon = IconHelpers.FromRelativePath("Assets\\FirefoxLogo.png");

    public FirefoxExtensionCommandsProvider()
    {
        InitDbService();
        DisplayName = "Firefox Extension";
        _commands = [
            new CommandItem(new SearchHistoryPage(_dbService)) { Title = "Search History", Subtitle = "Search Firefox History", Icon = ExtensionIcon },
            new CommandItem(new SearchBookmarksPage(_dbService)) { Title = "Search Bookmarks", Subtitle = "Search Firefox Bookmarks", Icon = ExtensionIcon },
        ];
    }

    private void InitDbService()
    {
        try
        {
            _dbService = new DbService(FileHelper.GetFirefoxDbPath());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing DbService: {ex.Message}");
        }
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }

}
