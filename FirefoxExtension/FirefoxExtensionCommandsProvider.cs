// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using FirefoxExtension.Data;
using FirefoxExtension.Helpers;
using System;


namespace FirefoxExtension;

public partial class FirefoxExtensionCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;
    private DbService? _dbService;

    public FirefoxExtensionCommandsProvider()
    {
        InitDbService();
        DisplayName = "Firefox Extension";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        _commands = [
            new CommandItem(new FirefoxExtensionPage(_dbService)) { Title = DisplayName },
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
