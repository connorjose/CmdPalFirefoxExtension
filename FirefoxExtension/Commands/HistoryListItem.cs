using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using Windows.System;

namespace FirefoxExtension.Commands
{
    internal sealed partial class HistoryListItem : ListItem
    {
        public HistoryListItem(string title, string url, IconInfo icon) : base(new NoOpCommand())
        {
            Title = string.IsNullOrWhiteSpace(title) ? url : title;
            Subtitle = url;
            Icon = icon;
            Command = new AnonymousCommand(action: () => OpenUrl(url));
        }

        private static async void OpenUrl(string url)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(url) && Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    await Launcher.LaunchUriAsync(uri);
                }
            }
            catch
            {
                // ignore launch failures
            }
        }
    }
}
