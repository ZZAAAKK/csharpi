using System;
using System.IO;
using Discord;
using csharpi.Extensions;
using Newtonsoft.Json;

namespace csharpi.Common
{
    public class StringConfiguration
    {
        [JsonIgnore]
        private static string FileName { get; } = "MythicalCuddles/DiscordBot/config/strings.json";
        [JsonIgnore]
        private static readonly string File = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), FileName);

        // VARIABLES
        public string DefaultWebsiteName = "Personal Website";

        // END

        public static void EnsureExists()
        {
            if (!System.IO.File.Exists(File))
            {
                string path = Path.GetDirectoryName(File);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var stringConfig = new StringConfiguration();
                stringConfig.SaveJson();

                new LogMessage(LogSeverity.Info, "String Configuration", FileName + " created.").PrintToConsole().GetAwaiter();
            }

            new LogMessage(LogSeverity.Info, "String Configuration", FileName + " loaded.").PrintToConsole().GetAwaiter();
        }

        public void SaveJson()
        {
            System.IO.File.WriteAllText(File, ToJson());
        }

        public static StringConfiguration Load()
        {
            return JsonConvert.DeserializeObject<StringConfiguration>(System.IO.File.ReadAllText(File));
        }

        public string ToJson()
            => JsonConvert.SerializeObject(this, Formatting.Indented);

        public static void UpdateConfiguration(string websiteName = null)
        {
            var stringConfig = new StringConfiguration()
            {
                DefaultWebsiteName = websiteName ?? Load().DefaultWebsiteName,
            };
            stringConfig.SaveJson();
        }
    }
}