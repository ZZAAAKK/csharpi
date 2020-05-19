using System;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using csharpi.Common;
using csharpi.Extensions;
using MySql.Data.MySqlClient;

namespace csharpi.Database
{
    public class DatabaseUser
    {
        public int UserID { get; private set; }
        public string UserName { get; private set; }
        public string FriendlyName { get; private set; }

        public DatabaseUser(string[] properties)
        {
            UserID = int.Parse(properties[0]);
            UserName = properties[1];
            FriendlyName = properties[2];
        }
    }
}
