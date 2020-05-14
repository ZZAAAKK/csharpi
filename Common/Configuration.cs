using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Discord;
using csharpi.Extensions;

namespace csharpi.Common
{
    public class Configuration
    {
        public static string GetDBPassword()
        {
            var _builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "config.json");  

            // build the configuration and assign to _config          
            IConfiguration _config = _builder.Build();

            return _config["DatabasePassword"];
        }
    }
}