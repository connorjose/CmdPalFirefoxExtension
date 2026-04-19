using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Profile;

namespace FirefoxExtension.Helpers
{
    public static class FileHelper
    {
        private static readonly string[] FireFoxReleases = [ ".default-release", ".default-nightly" ];
        private static readonly string UserDataDirectoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "Mozilla", 
            "Firefox", 
            "Profiles");

        private static string GetProfileDir() 
        {
            if (!Directory.Exists(UserDataDirectoryPath))
            {
                throw new DirectoryNotFoundException("Firefox profiles directory not found.");
            }

            var profileDir = Directory.GetDirectories(UserDataDirectoryPath)
                .FirstOrDefault(dir => FireFoxReleases
                .Any(suffix => Path.GetFileName(dir)
                .EndsWith(suffix, StringComparison.OrdinalIgnoreCase)));

            return profileDir ?? throw new InvalidOperationException("No matching Firefox profile found");
        }

        public static string GetFirefoxDbPath()
        {
            var profileDir = GetProfileDir();
            return Path.Combine(profileDir, "places.sqlite");
        }
    }
}
