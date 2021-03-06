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
        public static string GetConnectionString()
        {
            var _builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "config.json");  
         
            IConfiguration _config = _builder.Build();

            return _config["ConnectionString"];
        }
    }
}