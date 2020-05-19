using System;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using csharpi.Common;
using csharpi.Extensions;
using MySql.Data.MySqlClient;

namespace csharpi.Dimensions
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

    public class Weekday
    {
        public int DayID { get; private set; }
        public string ShortName { get; private set; }
        public string LongName { get; private set; }

        public Weekday (string[] properties) 
        {
            DayID = int.Parse(properties[0]);
            ShortName = properties[1];
            LongName = properties[2];
        }
    }

    public class Segment
    {
        public int SegmentID { get; private set; }
        public string Name { get; private set; }

        public Segment (string[] properties) 
        {
            SegmentID = int.Parse(properties[0]);
            Name = properties[1];
        }
    }
}
