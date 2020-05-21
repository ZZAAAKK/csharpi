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

    public class Schedule
    {
        public string UserName { get; private set; }
        public string Weekday { get; private set; }
        public string Segment { get; private set; }
        public string StartTime { get; private set; }
        public string EndTime { get; private set; }

        public Schedule (string[] properties) 
        {
            UserName = properties[0];
            Weekday = properties[1];
            Segment = properties[2];
            StartTime = properties[3];
            EndTime = properties[4];
        }
    }

    public class HeatmapRow
    {
        public string Segment { get; private set; }
        public int Monday { get; private set; }
        public int Tuesday { get; private set; }
        public int Wednesday { get; private set; }
        public int Thursday { get; private set; }
        public int Friday { get; private set; }
        public int Saturday { get; private set; }
        public int Sunday { get; private set; }

        public HeatmapRow(string[] properties)
        {
            Segment = properties[0];
            Monday = int.Parse(properties[1]);
            Tuesday = int.Parse(properties[2]);
            Wednesday = int.Parse(properties[3]);
            Thursday = int.Parse(properties[4]);
            Friday = int.Parse(properties[5]);
            Saturday = int.Parse(properties[6]);
            Sunday = int.Parse(properties[7]);
        }
    }

    public class ContentType
    {
        public int ID { get; private set; }
        public string Value { get; private set; }

        public ContentType(string [] properties)
        {
            ID = int.Parse(properties[0]);
            Value = properties[1];
        }
    }

    public class ContentVersion
    {
        public int ID { get; private set; }
        public string Value { get; private set; }

        public ContentVersion(string [] properties)
        {
            ID = int.Parse(properties[0]);
            Value = properties[1];
        }
    }

    public class ScheduledContent
    {
        public int ID { get; private set; }
        public string Title { get; private set; }
        public string UserName { get; private set; }
        public int RequiredLevel { get; private set; }
        public int AverageItemLevel { get; private set; }
        public string Type { get; private set; }
        public string Version { get; private set; }
        public bool Complete { get; private set; }
        public DateTime CompleteDateTime { get; private set; }

        public ScheduledContent(string[] properties) 
        {
            ID = int.Parse(properties[0]);
            Title = properties[1];
            UserName = properties[2];
            RequiredLevel = int.Parse(properties[3]);
            AverageItemLevel = int.Parse(properties[4]);
            Type = properties[5];
            Version = properties[6];
            Complete = Convert.ToBoolean(properties[7]);
            CompleteDateTime = DateTime.Parse(properties[8]);
        }
    }
}
