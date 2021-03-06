using Discord;
using Discord.Net;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using csharpi;
using csharpi.Extensions;
using csharpi.Dimensions;
using csharpi.Common;
using System.Data;

namespace csharpi.Modules
{
    public class Commands : ModuleBase
    {
        public static string ConnectionString { get => Configuration.GetConnectionString(); }

        [Command("commands")]
        public async Task CommandsCommand()
        {
            var sb = new StringBuilder();
            var embed = new EmbedBuilder();

            embed.WithColor(new Color(255, 0, 0));
            embed.WithTitle("List of available commands from Swift Bot");

            List<string> commandsList = new List<string>() 
            {
                "commands",
                "scheduling *",
                "myschedule",
                "content *",
                "mycontent"
            };

            sb.AppendLine($"Here is a list of all commands available through Swift Bot: ");
            sb.AppendLine();

            foreach (string s in commandsList)
            {
                sb.AppendLine($"\"{s}\"");
            }

            sb.AppendLine();
            sb.AppendLine($"Just remember to use '$' at the beginning of each command.");
            sb.AppendLine();
            sb.AppendLine($"* _For more info on these just type the command followed by 'help'_");
            sb.AppendLine();
            sb.AppendLine($"Happy commanding...!");

            embed.WithDescription(sb.ToString());

            await ReplyAsync(null, false, embed.Build());
        }

        [Command("fuckyou")]
        public async Task FuckYouCommand()
        {
            await ReplyAsync($"No, fuck _you_ <@!{Context.User.Id}>!");
        }

        [Command("weekdays")]
        //[RequireUserPermission(GuildPermission.KickMembers)]
        public async Task GetWeekdays()
        {
            SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();

            List<Weekday> weekdays = new List<Weekday>();

            using (SqlDataAdapter adapter = new SqlDataAdapter(new SqlStoredProcedure("usp_Get_Weekdays", connection)))
            {
                DataSet data = new DataSet();
                adapter.Fill(data);

                foreach (DataRow r in data.Tables[0].Rows)
                {
                    weekdays.Add(new Weekday(r.RowStrings()));
                }
            }

            connection.Close();
            
            var sb = new StringBuilder();
            var embed = new EmbedBuilder();

            embed.WithColor(new Color(0, 255,0));
            embed.Title = "Weekdays:";
            sb.AppendLine($"Day   ShortName   LongName");

            foreach (Weekday w in weekdays)
            {
                sb.AppendLine($"{w.DayID}   {w.ShortName}   {w.LongName}");
            }

            embed.Description = sb.ToString();
            await ReplyAsync(null, false, embed.Build());
        }

        [Command("segments")]
        public async Task GetSegments()
        {
            SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();

            List<Segment> segments = new List<Segment>();

            using (SqlDataAdapter adapter = new SqlDataAdapter(new SqlStoredProcedure("usp_Get_Segment", connection)))
            {
                DataSet data = new DataSet();
                adapter.Fill(data);

                foreach (DataRow r in data.Tables[0].Rows)
                {
                    segments.Add(new Segment(r.RowStrings()));
                }
            }

            connection.Close();
            
            var sb = new StringBuilder();
            var embed = new EmbedBuilder();

            embed.WithColor(new Color(0, 255,0));
            embed.Title = "Segments:";

            foreach (Segment segment in segments)
            {
                sb.AppendLine($"{segment.Name}");
            }

            embed.Description = sb.ToString();
            await ReplyAsync(null, false, embed.Build());
        }

        [Command("myschedule")]
        public async Task MyScheduleCommand()
        {
            var sb = new StringBuilder();
            var embed = new EmbedBuilder();
            var user = Context.User;

            List<Schedule> schedules = new List<Schedule>();

            SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();

            SqlCommand command = new SqlStoredProcedure("usp_Get_Schedule", 
                new SqlParameter[] 
                {
                    new SqlParameter("@action", 's'),
                    new SqlParameter("@name", $"<@!{user.Id}>")
                }, 
                connection);

            SqlDataAdapter adapter = new SqlDataAdapter(command);
            command.ExecuteNonQuery();
            DataSet data = new DataSet();
            adapter.Fill(data);

            foreach (DataRow r in data.Tables[0].Rows)
            {
                schedules.Add(new Schedule(r.RowStrings()));
            }

            embed.WithColor(new Color(0, 255,0));
            embed.Title = $"{user.Username}'s Schedule";

            foreach (Schedule schedule in schedules)
            {
                sb.AppendLine($"{schedule.Weekday} -> {schedule.Segment} ({schedule.StartTime} - {schedule.EndTime})");
            }

            embed.Description = sb.ToString();
            await ReplyAsync(null, false, embed.Build());
        }

        [Command("scheduling")]
        public async Task SchedulingCommand([Remainder]string args = null)
        {
            var sb = new StringBuilder();
            var embed = new EmbedBuilder();
            var user = Context.User;

            args = args.ToLower() == "help" ? "help " : 
                args.ToLower() == "?" ? "? " : 
                args.ToLower() == "users" ? "users " : 
                args.ToLower() == "schedule" ? "schedule " : 
                args;

            switch (args.Substring(0, args.IndexOf(' ')).ToLower())
            {
                case "users":
                    try 
                    {
                        SqlConnection connection = new SqlConnection(ConnectionString);
                        connection.Open();

                        SqlCommand command = new SqlStoredProcedure("usp_Get_User", 
                            new SqlParameter[] 
                            {
                                new SqlParameter("@action", 'a'),
                                new SqlParameter("@name", string.Empty)
                            }, 
                            connection);

                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        command.ExecuteNonQuery();

                        DataSet data = new DataSet();
                        adapter.Fill(data);

                        List<DatabaseUser> users = new List<DatabaseUser>();

                        foreach (DataRow r in data.Tables[0].Rows)
                        {
                            users.Add(new DatabaseUser(r.RowStrings()));
                        }
                            
                        sb.AppendLine($"The following users exist in the scheduling server:");

                        foreach (DatabaseUser u in users)
                        {
                            sb.AppendLine($"{u.UserName}");
                        }
                    }
                    catch (Exception e)
                    {
                        sb.AppendLine($"Sorry <@!{user.Id}>, I didn't get the content you asked for, but I got you this error:\n{e.Message}");
                    }
                    break;
                case "adduser":
                    try 
                    {
                        SqlConnection connection = new SqlConnection(ConnectionString);
                        connection.Open();

                        SqlCommand command = new SqlStoredProcedure("usp_Set_User", 
                            new SqlParameter[] 
                            {
                                new SqlParameter("@id", args.Replace("adduser ", string.Empty)),
                                new SqlParameter("@name", Program._client.GetUser(ulong.Parse(args.Replace("adduser ", string.Empty).Replace("<@!", string.Empty).Replace(">", string.Empty))).Username)
                            }, 
                            connection);

                        command.ExecuteNonQuery();

                        sb.AppendLine($"Successfully added user *{args.Replace("adduser ", string.Empty)}*");
                    }
                    catch (Exception e)
                    {
                        sb.AppendLine($"Sorry <@!{user.Id}>, I failed to create user *{args.Replace("adduser ", string.Empty)}* with error\n{e.Message}");
                    }
                    break;
                case "removeuser":
                    try 
                    {
                        SqlConnection connection = new SqlConnection(ConnectionString);
                        connection.Open();

                        SqlCommand command = new SqlStoredProcedure("usp_Delete_User", 
                            new SqlParameter[] 
                            {
                                new SqlParameter("@name", args.Replace("removeuser ", string.Empty))
                            }, 
                            connection);

                        command.ExecuteNonQuery();

                        sb.AppendLine($"Successfully removed user *{args.Replace("removeuser ", string.Empty)}*");
                    }
                    catch (Exception e)
                    {
                        embed.Color = new Color(255, 0, 0);
                        sb.AppendLine($"Sorry <@!{user.Id}>, I failed to remove user *{args.Replace("removeuser ", string.Empty)}* but I got you this error:\n{e.Message}");
                    }
                    break;
                case "addtime":
                    try 
                    {
                        DatabaseUser databaseUser;
                        List<Weekday> weekdays = new List<Weekday>();
                        List<Segment> segments = new List<Segment>();

                        SqlConnection connection = new SqlConnection(ConnectionString);
                        connection.Open();

                        SqlCommand command = new SqlStoredProcedure("usp_Get_User", 
                            new SqlParameter[] 
                            {
                                new SqlParameter("@action", 's'),
                                new SqlParameter("@name", $"<@!{user.Id}>")
                            }, 
                            connection);

                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        command.ExecuteNonQuery();
                        DataSet data = new DataSet();
                        adapter.Fill(data);

                        databaseUser = new DatabaseUser(data.Tables[0].Rows[0].RowStrings());

                        command = new SqlStoredProcedure("usp_Get_Segment", connection);

                        adapter = new SqlDataAdapter(command);
                        command.ExecuteNonQuery();
                        data = new DataSet();
                        adapter.Fill(data);

                        foreach (DataRow r in data.Tables[0].Rows)
                        {
                            segments.Add(new Segment(r.RowStrings()));
                        }

                        command = new SqlStoredProcedure("usp_Get_Weekdays", connection);

                        adapter = new SqlDataAdapter(command);
                        command.ExecuteNonQuery();
                        data = new DataSet();
                        adapter.Fill(data);

                        foreach (DataRow r in data.Tables[0].Rows)
                        {
                            weekdays.Add(new Weekday(r.RowStrings()));
                        }

                        string[] parameters = args.Replace("addtime ", string.Empty).ToUpper().Replace(" ", string.Empty).Split(';');

                        command = new SqlStoredProcedure("usp_Set_Schedule",
                            new SqlParameter[] 
                            {
                                new SqlParameter("@action", 'i'),
                                new SqlParameter("@user", databaseUser.UserID),
                                new SqlParameter("@day", weekdays.Find(x => x.LongName.ToUpper() == parameters[0] || x.ShortName.ToUpper() == parameters[0]).DayID),
                                new SqlParameter("@seg", segments.Find(x => x.Name.ToUpper() == parameters[1]).SegmentID)
                            },
                            connection);

                        command.ExecuteNonQuery();

                        sb.AppendLine($"Successfully created the following scheduled segment:");
                        sb.AppendLine($"User: {databaseUser.UserName}");
                        sb.AppendLine($"Weekday: {weekdays.Find(x => x.LongName.ToUpper() == parameters[0] || x.ShortName.ToUpper() == parameters[0]).LongName}");
                        sb.AppendLine($"Segment: {segments.Find(x => x.Name.ToUpper() == parameters[1]).Name}");
                    }
                    catch (Exception e)
                    {
                        embed.Color = new Color(255, 0, 0);
                        sb.AppendLine($"Sorry <@!{user.Id}>, I failed to create that scheduled segment but I got you this error:\n{e.Message}");
                    }
                    break;
                case "removetime":
                    try 
                    {
                        DatabaseUser databaseUser;
                        List<Weekday> weekdays = new List<Weekday>();
                        List<Segment> segments = new List<Segment>();

                        SqlConnection connection = new SqlConnection(ConnectionString);
                        connection.Open();

                        SqlCommand command = new SqlStoredProcedure("usp_Get_User", 
                            new SqlParameter[] 
                            {
                                new SqlParameter("@action", 's'),
                                new SqlParameter("@name", $"<@!{user.Id}>")
                            }, 
                            connection);

                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        command.ExecuteNonQuery();
                        DataSet data = new DataSet();
                        adapter.Fill(data);

                        databaseUser = new DatabaseUser(data.Tables[0].Rows[0].RowStrings());

                        command = new SqlStoredProcedure("usp_Get_Segment", connection);

                        adapter = new SqlDataAdapter(command);
                        command.ExecuteNonQuery();
                        data = new DataSet();
                        adapter.Fill(data);

                        foreach (DataRow r in data.Tables[0].Rows)
                        {
                            segments.Add(new Segment(r.RowStrings()));
                        }

                        command = new SqlStoredProcedure("usp_Get_Weekdays", connection);

                        adapter = new SqlDataAdapter(command);
                        command.ExecuteNonQuery();
                        data = new DataSet();
                        adapter.Fill(data);

                        foreach (DataRow r in data.Tables[0].Rows)
                        {
                            weekdays.Add(new Weekday(r.RowStrings()));
                        }

                        string[] parameters = args.Replace("removetime ", string.Empty).ToUpper().Replace(" ", string.Empty).Split(';');

                        command = new SqlStoredProcedure("usp_Set_Schedule",
                            new SqlParameter[] 
                            {
                                new SqlParameter("@action", 'd'),
                                new SqlParameter("@user", databaseUser.UserID),
                                new SqlParameter("@day", weekdays.Find(x => x.LongName.ToUpper() == parameters[0] || x.ShortName.ToUpper() == parameters[0]).DayID),
                                new SqlParameter("@seg", segments.Find(x => x.Name.ToUpper() == parameters[1]).SegmentID)
                            },
                            connection);

                        command.ExecuteNonQuery();

                        sb.AppendLine($"Successfully removed the following scheduled segment:");
                        sb.AppendLine($"User: {databaseUser.UserName}");
                        sb.AppendLine($"Weekday: {weekdays.Find(x => x.LongName.ToUpper() == parameters[0] || x.ShortName.ToUpper() == parameters[0]).LongName}");
                        sb.AppendLine($"Segment: {segments.Find(x => x.Name.ToUpper() == parameters[1]).Name}");
                    }
                    catch (Exception e)
                    {
                        embed.Color = new Color(255, 0, 0);
                        sb.AppendLine($"Sorry <@!{user.Id}>, I failed to remove scheduled segment but I got you this error:\n{e.Message}");
                    }
                    break;
                case "schedule":
                    try 
                    {
                        List<HeatmapRow> heatmapRows = new List<HeatmapRow>();

                        SqlConnection connection = new SqlConnection(ConnectionString);
                        connection.Open();

                        SqlCommand command = new SqlStoredProcedure("usp_Get_Schedule", 
                            new SqlParameter[] 
                            {
                                new SqlParameter("@action", 'h'),
                                new SqlParameter("@name", string.Empty)
                            }, 
                            connection);

                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        command.ExecuteNonQuery();
                        DataSet data = new DataSet();
                        adapter.Fill(data);

                        foreach (DataRow r in data.Tables[0].Rows)
                        {
                            heatmapRows.Add(new HeatmapRow(r.RowStrings()));
                        }

                        sb.AppendLine("```");
                        sb.AppendLine("╔═════╦════╦════╦════╦════╦════╦════╦════╗");
                        sb.AppendLine("║     ║ Mo ║ Tu ║ We ║ Th ║ Fr ║ Sa ║ Su ║");
                        sb.AppendLine("╠═════╬════╬════╬════╬════╣════╣════╣════╣");
                        if (heatmapRows.Find(x => x.Segment == "Morning") == null)
                        {
                            sb.AppendLine("║ Mrn ║  0 ║  0 ║  0 ║  0 ║  0 ║  0 ║  0 ║");
                        }
                        else 
                        {
                            HeatmapRow r = heatmapRows.Find(x => x.Segment == "Morning");
                            string[] days = 
                            {
                                r.Monday < 10 ? $" {r.Monday}" : $"{r.Monday}",
                                r.Tuesday < 10 ? $" {r.Tuesday}" : $"{r.Tuesday}",
                                r.Wednesday < 10 ? $" {r.Wednesday}" : $"{r.Wednesday}",
                                r.Thursday < 10 ? $" {r.Thursday}" : $"{r.Thursday}",
                                r.Friday < 10 ? $" {r.Friday}" : $"{r.Friday}",
                                r.Saturday < 10 ? $" {r.Saturday}" : $"{r.Saturday}",
                                r.Sunday < 10 ? $" {r.Sunday}" : $"{r.Sunday}"
                            };
                            sb.AppendLine($"║ Mrn ║ {days[0]} ║ {days[1]} ║ {days[2]} ║ {days[3]} ║ {days[4]} ║ {days[5]} ║ {days[6]} ║");
                        }
                        sb.AppendLine("╠═════╬════╬════╬════╬════╣════╣════╣════╣");
                        if (heatmapRows.Find(x => x.Segment == "Afternoon") == null)
                        {
                            sb.AppendLine("║ Aft ║  0 ║  0 ║  0 ║  0 ║  0 ║  0 ║  0 ║");
                        }
                        else 
                        {
                            HeatmapRow r = heatmapRows.Find(x => x.Segment == "Afternoon");
                            string[] days = 
                            {
                                r.Monday < 10 ? $" {r.Monday}" : $"{r.Monday}",
                                r.Tuesday < 10 ? $" {r.Tuesday}" : $"{r.Tuesday}",
                                r.Wednesday < 10 ? $" {r.Wednesday}" : $"{r.Wednesday}",
                                r.Thursday < 10 ? $" {r.Thursday}" : $"{r.Thursday}",
                                r.Friday < 10 ? $" {r.Friday}" : $"{r.Friday}",
                                r.Saturday < 10 ? $" {r.Saturday}" : $"{r.Saturday}",
                                r.Sunday < 10 ? $" {r.Sunday}" : $"{r.Sunday}"
                            };
                            sb.AppendLine($"║ Aft ║ {days[0]} ║ {days[1]} ║ {days[2]} ║ {days[3]} ║ {days[4]} ║ {days[5]} ║ {days[6]} ║");
                        }
                        sb.AppendLine("╠═════╬════╬════╬════╬════╣════╣════╣════╣");
                        if (heatmapRows.Find(x => x.Segment == "Evening") == null)
                        {
                            sb.AppendLine("║ Eve ║  0 ║  0 ║  0 ║  0 ║  0 ║  0 ║  0 ║");
                        }
                        else 
                        {
                            HeatmapRow r = heatmapRows.Find(x => x.Segment == "Evening");
                            string[] days = 
                            {
                                r.Monday < 10 ? $" {r.Monday}" : $"{r.Monday}",
                                r.Tuesday < 10 ? $" {r.Tuesday}" : $"{r.Tuesday}",
                                r.Wednesday < 10 ? $" {r.Wednesday}" : $"{r.Wednesday}",
                                r.Thursday < 10 ? $" {r.Thursday}" : $"{r.Thursday}",
                                r.Friday < 10 ? $" {r.Friday}" : $"{r.Friday}",
                                r.Saturday < 10 ? $" {r.Saturday}" : $"{r.Saturday}",
                                r.Sunday < 10 ? $" {r.Sunday}" : $"{r.Sunday}"
                            };
                            sb.AppendLine($"║ Eve ║ {days[0]} ║ {days[1]} ║ {days[2]} ║ {days[3]} ║ {days[4]} ║ {days[5]} ║ {days[6]} ║");
                        }
                        sb.AppendLine("╠═════╬════╬════╬════╬════╣════╣════╣════╣");
                        if (heatmapRows.Find(x => x.Segment == "Night") == null)
                        {
                            sb.AppendLine("║ Noc ║  0 ║  0 ║  0 ║  0 ║  0 ║  0 ║  0 ║");
                        }
                        else 
                        {
                            HeatmapRow r = heatmapRows.Find(x => x.Segment == "Night");
                            string[] days = 
                            {
                                r.Monday < 10 ? $" {r.Monday}" : $"{r.Monday}",
                                r.Tuesday < 10 ? $" {r.Tuesday}" : $"{r.Tuesday}",
                                r.Wednesday < 10 ? $" {r.Wednesday}" : $"{r.Wednesday}",
                                r.Thursday < 10 ? $" {r.Thursday}" : $"{r.Thursday}",
                                r.Friday < 10 ? $" {r.Friday}" : $"{r.Friday}",
                                r.Saturday < 10 ? $" {r.Saturday}" : $"{r.Saturday}",
                                r.Sunday < 10 ? $" {r.Sunday}" : $"{r.Sunday}"
                            };
                            sb.AppendLine($"║ Noc ║ {days[0]} ║ {days[1]} ║ {days[2]} ║ {days[3]} ║ {days[4]} ║ {days[5]} ║ {days[6]} ║");
                        }
                        sb.AppendLine("╚═════╩════╩════╩════╩════╩════╩════╩════╝");
                        sb.AppendLine("```");
                        await ReplyAsync(sb.ToString());
                        return;
                    }
                    catch (Exception e)
                    {
                        embed.Color = new Color(255, 0, 0);
                        sb.AppendLine($"Sorry <@!{user.Id}>, I failed to generate the heatmap but I got you this error:\n{e.Message}");
                    }
                    break;
                case "?":
                case "help":
                    embed.WithColor(new Color(0, 0, 0));
                    embed.Title = "Scheduling Help";

                    sb.AppendLine("You can use the following commands in the scheduler:");
                    sb.AppendLine();
                    sb.AppendLine("adduser [@User Name]");
                    sb.AppendLine("removeuser [@User Name]");
                    sb.AppendLine("addtime [day]; [period]");
                    sb.AppendLine("removetime [day]; [period]");
                    sb.AppendLine("users");
                    sb.AppendLine("schedule");
                    sb.AppendLine("help");
                    sb.AppendLine("?");
                    sb.AppendLine();
                    sb.AppendLine("Please note the following:");
                    sb.AppendLine("1) You must be a user in the scheduling server before attempting to schedule times.");
                    sb.AppendLine("2) This is a trust-based system so all members of the Discord server can add and remove users, however if this is abused then roles will be introduced.");
                    sb.AppendLine("3) You do not need to specify a user when scheduling times as it refers to your own times only.");
                    break;
                default:
                    sb.AppendLine($"Sorry <@!{user.Id}>, I didn't understand that.");
                    sb.AppendLine();
                    sb.AppendLine("Please type '$scheduling help' or '$scheduling ?' for a list of commands.");
                    break;
            }
            
            embed.Description = sb.ToString();
            await ReplyAsync(null, false, embed.Build());
        }

        [Command("mycontent")]
        public async Task GetMyContent()
        {
            var sb = new StringBuilder();
            var embed = new EmbedBuilder();
            var user = Context.User;

            embed.Title = "Your Content:";
            embed.Color = new Color(0, 0, 0);

            try
            {
                SqlConnection connection = new SqlConnection(ConnectionString);
                connection.Open();

                List<ScheduledContent> scheduledContents = new List<ScheduledContent>();

                SqlCommand command = new SqlStoredProcedure("usp_Get_Scheduled_Content", connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                command.ExecuteNonQuery();

                DataSet data = new DataSet();
                adapter.Fill(data);

                
                foreach (DataRow r in data.Tables[0].Rows)
                {
                    scheduledContents.Add(new ScheduledContent(r.RowStrings()));
                }

                scheduledContents.RemoveAll(x => x.UserName != $"<@!{user.Id}>");

                if (scheduledContents.Count == 0)
                {
                    embed.Color = new Color(255, 0, 0);
                    sb.AppendLine($"Sorry <@!{user.Id}>, looks like you don't have any content yet.");
                }
                else 
                {
                    foreach (ScheduledContent s in scheduledContents)
                    {
                        string completeString = s.Complete == 1 ? $"Marked complete on {s.CompleteDateTime}" : "Not complete";
                        sb.AppendLine($"{s.Title} -> {completeString} (ID = {s.ID})");
                    }
                }
            }
            catch (Exception e)
            {
                embed.Color = new Color(255, 0, 0);
                sb.AppendLine($"Sorry <@!{user.Id}>, I couldn't get your content but I got you this error instead: \n{e.Message}");
            }

            embed.Description = sb.ToString();
            await ReplyAsync(null, false, embed.Build());
        }

        [Command("content")]
        public async Task ContentCommand([Remainder]string args = null)
        {
            var sb = new StringBuilder();
            var embed = new EmbedBuilder();
            var user = Context.User;

            args = args.ToLower() == "help" ? "help " : 
                args.ToLower() == "?" ? "? " : 
                args.ToLower() == "type" ? "type " : 
                args.ToLower() == "version" ? "version " : 
                args.ToLower() == "random" ? "random " : 
                args.ToLower() == "all" ? "all " : 
                args;

            switch (args.Substring(0, args.IndexOf(' ')).ToLower())
            {
                case "type":
                    try 
                    {
                        SqlConnection connection = new SqlConnection(ConnectionString);
                        connection.Open();

                        List<ContentType> types = new List<ContentType>();

                        SqlCommand command = new SqlStoredProcedure("usp_Get_Content_Type", connection);
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        command.ExecuteNonQuery();

                        DataSet data = new DataSet();
                        adapter.Fill(data);
                    
                        foreach (DataRow r in data.Tables[0].Rows)
                        {
                            types.Add(new ContentType(r.RowStrings()));
                        }

                        sb.AppendLine("```");
                        sb.AppendLine("╔════╦═════════════════╗");
                        sb.AppendLine("║ ID ║      Value      ║");

                        foreach (ContentType t in types)
                        {
                            sb.AppendLine("║════║═════════════════║");
                            sb.AppendLine($"║  {t.ID} ║ {t.Value.PadRight(16)}║");
                        }

                        sb.AppendLine("╚════╩═════════════════╝");
                        sb.AppendLine("```");
                    }
                    catch (Exception e)
                    {
                        embed.Color = new Color(255, 0, 0);
                        sb.AppendLine($"Sorry <@!{user.Id}>, I couldn't get your content but I got you this error instead: \n{e.Message}");
                    }
                    break;
                case "version":
                    try 
                    {
                        SqlConnection connection = new SqlConnection(ConnectionString);
                        connection.Open();

                        List<ContentVersion> versions = new List<ContentVersion>();

                        SqlCommand command = new SqlStoredProcedure("usp_Get_Content_Version", connection);
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        command.ExecuteNonQuery();

                        DataSet data = new DataSet();
                        adapter.Fill(data);
                    
                        foreach (DataRow r in data.Tables[0].Rows)
                        {
                            versions.Add(new ContentVersion(r.RowStrings()));
                        }

                        sb.AppendLine("```");
                        sb.AppendLine("╔════╦═════════════════╗");
                        sb.AppendLine("║ ID ║      Value      ║");

                        foreach (ContentVersion v in versions)
                        {
                            sb.AppendLine("║════║═════════════════║");
                            sb.AppendLine($"║  {v.ID} ║ {v.Value.PadRight(16)}║");
                        }

                        sb.AppendLine("╚════╩═════════════════╝");
                        sb.AppendLine("```");
                    }
                    catch (Exception e)
                    {
                        embed.Color = new Color(255, 0, 0);
                        sb.AppendLine($"Sorry <@!{user.Id}>, I couldn't get your content but I got you this error instead: \n{e.Message}");
                    }
                    break;
                case "filter":
                    try 
                    {
                        SqlConnection connection = new SqlConnection(ConnectionString);
                        connection.Open();

                        List<ContentType> types = new List<ContentType>();
                        List<ContentVersion> versions = new List<ContentVersion>();
                        List<Duty> duties = new List<Duty>();

                        SqlCommand command = new SqlStoredProcedure("usp_Get_Content_Type", connection);
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        command.ExecuteNonQuery();

                        DataSet data = new DataSet();
                        adapter.Fill(data);
                    
                        foreach (DataRow r in data.Tables[0].Rows)
                        {
                            types.Add(new ContentType(r.RowStrings()));
                        }

                        command = new SqlStoredProcedure("usp_Get_Content_Version", connection);
                        adapter = new SqlDataAdapter(command);
                        command.ExecuteNonQuery();

                        data = new DataSet();
                        adapter.Fill(data);
                    
                        foreach (DataRow r in data.Tables[0].Rows)
                        {
                            versions.Add(new ContentVersion(r.RowStrings()));
                        }

                        string[] parameters = args.Replace("filter ", string.Empty).ToLower().Replace(" ", string.Empty).Split(';');

                        int typeID;
                        int versionID;

                        try
                        {
                            typeID = int.Parse(parameters[0]);
                        }
                        catch(Exception e)
                        {
                            typeID = types.Find(x => x.Value.ToLower() == parameters[0]).ID;
                            e.ToString().PrintLogMessage("Error caught: ", LogSeverity.Error);
                        }

                        try
                        {
                            versionID = int.Parse(parameters[1]);
                        }
                        catch(Exception e)
                        {
                            versionID = versions.Find(x => x.Value.ToLower() == parameters[1]).ID;
                            e.ToString().PrintLogMessage("Error caught: ", LogSeverity.Error);
                        }

                        command = new SqlStoredProcedure("usp_Get_Duty", 
                            new SqlParameter[] 
                            {
                                new SqlParameter("@type", typeID),
                                new SqlParameter("@version", versionID),
                                new SqlParameter("@action", 'f')
                            }, connection);
                        adapter = new SqlDataAdapter(command);
                        command.ExecuteNonQuery();

                        data = new DataSet();
                        adapter.Fill(data);
                    
                        foreach (DataRow r in data.Tables[0].Rows)
                        {
                            duties.Add(new Duty(r.RowStrings()));
                        }

                        sb.AppendLine("```");
                        sb.AppendLine("╔══════╦════════════════════════════════════════════════╗");
                        sb.AppendLine("║  ID  ║                     Title                      ║");

                        foreach (Duty d in duties)
                        {
                            sb.AppendLine($"║  {d.ID.ToString().PadLeft(3)} ║ {d.Title.PadRight(46)} ║");
                        }

                        sb.AppendLine("╚══════╩════════════════════════════════════════════════╝");
                        sb.AppendLine("```");
                        await ReplyAsync(sb.ToString());
                        return;
                    }
                    catch (Exception e)
                    {
                        embed.Color = new Color(255, 0, 0);
                        sb.AppendLine($"Sorry <@!{user.Id}>, I couldn't get your content but I got you this error instead: \n{e.Message}");
                    }
                    break;
                case "add":
                    try
                    {
                        SqlConnection connection = new SqlConnection(ConnectionString);
                        connection.Open();
                        
                        List<Duty> duties = new List<Duty>();
                        DatabaseUser databaseUser;

                        SqlCommand command = new SqlStoredProcedure("usp_Get_Duty", 
                            new SqlParameter[] 
                            {
                                new SqlParameter("@action", 'a'),
                                new SqlParameter("@type", 0),
                                new SqlParameter("@version", 0)
                            }, connection);
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        command.ExecuteNonQuery();

                        DataSet data = new DataSet();
                        adapter.Fill(data);

                        foreach (DataRow r in data.Tables[0].Rows)
                        {
                            duties.Add(new Duty(r.RowStrings()));
                        }

                        command = new SqlStoredProcedure("usp_Get_User", 
                            new SqlParameter[] 
                            {
                                new SqlParameter("@action", 's'),
                                new SqlParameter("@name", $"<@!{user.Id}>")
                            }, 
                            connection);

                        adapter = new SqlDataAdapter(command);
                        command.ExecuteNonQuery();
                        data = new DataSet();
                        adapter.Fill(data);

                        databaseUser = new DatabaseUser(data.Tables[0].Rows[0].RowStrings());
                        Duty duty = duties.Find(x => x.ID == int.Parse(args.Replace("add ", string.Empty)));

                        command = new SqlStoredProcedure("usp_Set_Scheduled_Content", 
                            new SqlParameter[] 
                            {
                                new SqlParameter("@action", 'i'),
                                new SqlParameter("@user", databaseUser.UserID),
                                new SqlParameter("@dutyID", duty.ID),
                                new SqlParameter("@compTimeStamp", DateTime.Now),
                                new SqlParameter("@scheduledContentID", 0)
                            }, 
                            connection);
                        command.ExecuteNonQuery();

                        sb.AppendLine($"Successfully added {duty.Title} to your scheduled content!");
                    }
                    catch (Exception e)
                    {
                        embed.Color = new Color(255, 0, 0);
                        sb.AppendLine($"Sorry <@!{user.Id}>, I wasn't able to add that content but I got you this error instead: \n{e.Message}");
                    }
                    break;
                case "remove":
                    try
                    {
                        SqlConnection connection = new SqlConnection(ConnectionString);
                        connection.Open();

                        List<ScheduledContent> scheduledContents = new List<ScheduledContent>();

                        SqlCommand command = new SqlStoredProcedure("usp_Get_Scheduled_Content", connection);
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        command.ExecuteNonQuery();

                        DataSet data = new DataSet();
                        adapter.Fill(data);

                        
                        foreach (DataRow r in data.Tables[0].Rows)
                        {
                            scheduledContents.Add(new ScheduledContent(r.RowStrings()));
                        }

                        ScheduledContent content = scheduledContents.Find(x => x.ID == int.Parse(args.Replace("remove ", string.Empty)));

                        command = new SqlStoredProcedure("usp_Set_Scheduled_Content", 
                            new SqlParameter[] 
                            {
                                new SqlParameter("@action", 'd'),
                                new SqlParameter("@user", 0),
                                new SqlParameter("@dutyID", 0),
                                new SqlParameter("@compTimeStamp", DateTime.Now),
                                new SqlParameter("@scheduledContentID", content.ID)
                            }, 
                            connection);
                        command.ExecuteNonQuery();

                        sb.AppendLine($"Successfully removed {content.Title} from your scheduled content!");
                    }
                    catch (Exception e)
                    {
                        embed.Color = new Color(255, 0, 0);
                        sb.AppendLine($"Sorry <@!{user.Id}>, I couldn't remove that from your content but I got you this error instead: \n{e.Message}");
                    }
                    break;
                case "markcomplete":
                    try
                    {
                        SqlConnection connection = new SqlConnection(ConnectionString);
                        connection.Open();

                        List<ScheduledContent> scheduledContents = new List<ScheduledContent>();

                        SqlCommand command = new SqlStoredProcedure("usp_Get_Scheduled_Content", connection);
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        command.ExecuteNonQuery();

                        DataSet data = new DataSet();
                        adapter.Fill(data);

                        foreach (DataRow r in data.Tables[0].Rows)
                        {
                            scheduledContents.Add(new ScheduledContent(r.RowStrings()));
                        }

                        ScheduledContent content = scheduledContents.Find(x => x.ID == int.Parse(args.Replace("markcomplete ", string.Empty)));

                        command = new SqlStoredProcedure("usp_Set_Scheduled_Content", 
                            new SqlParameter[] 
                            {
                                new SqlParameter("@action", 'u'),
                                new SqlParameter("@user", 0),
                                new SqlParameter("@dutyID", 0),
                                new SqlParameter("@compTimeStamp", DateTime.Now),
                                new SqlParameter("@scheduledContentID", content.ID)
                            }, 
                            connection);
                        command.ExecuteNonQuery();

                        sb.AppendLine($"Successfully marked {content.Title} as complete!");
                    }
                    catch (Exception e)
                    {
                        embed.Color = new Color(255, 0, 0);
                        sb.AppendLine($"Sorry <@!{user.Id}>, I wasn't able to update that but I got you this error instead: \n{e.Message}");
                    }
                    break;
                case "all":
                    try
                    {
                        SqlConnection connection = new SqlConnection(ConnectionString);
                        connection.Open();

                        List<ScheduledContent> scheduledContents = new List<ScheduledContent>();

                        SqlCommand command = new SqlStoredProcedure("usp_Get_Scheduled_Content", connection);
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        command.ExecuteNonQuery();

                        DataSet data = new DataSet();
                        adapter.Fill(data);

                        
                        foreach (DataRow r in data.Tables[0].Rows)
                        {
                            scheduledContents.Add(new ScheduledContent(r.RowStrings()));
                        }

                        scheduledContents.RemoveAll(x => x.Complete != 0);

                        if (scheduledContents.Count == 0)
                        {
                            embed.Color = new Color(255, 0, 0);
                            sb.AppendLine($"Sorry <@!{user.Id}>, looks like there isn't any content yet.");
                        }
                        else 
                        {
                            foreach (ScheduledContent s in scheduledContents) 
                            {
                                if (s.Complete == 0) 
                                {
                                    sb.AppendLine($"{s.Title} -> Added by {s.UserName} (ID = {s.ID})");
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        embed.Color = new Color(255, 0, 0);
                        sb.AppendLine($"Sorry <@!{user.Id}>, I couldn't get the content you asked for but I got you this error instead: \n{e.Message}");
                    }
                    break;
                case "random":
                    embed.Title = "Random Content:";
                    Random random = new Random();
                    int red = random.Next(0, 255);
                    int green = random.Next(0, 255);
                    int blue = random.Next(0, 255);

                    embed.Color = new Color(red, green, blue);

                    try
                    {
                        SqlConnection connection = new SqlConnection(ConnectionString);
                        connection.Open();

                        List<ScheduledContent> scheduledContents = new List<ScheduledContent>();

                        SqlCommand command = new SqlStoredProcedure("usp_Get_Scheduled_Content", connection);
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        command.ExecuteNonQuery();

                        DataSet data = new DataSet();
                        adapter.Fill(data);

                        
                        foreach (DataRow r in data.Tables[0].Rows)
                        {
                            scheduledContents.Add(new ScheduledContent(r.RowStrings()));
                        }

                        scheduledContents.RemoveAll(x => x.Complete != 0);

                        if (scheduledContents.Count == 0)
                        {
                            embed.Color = new Color(255, 0, 0);
                            sb.AppendLine($"Sorry <@!{user.Id}>, looks like there isn't any content yet.");
                        }
                        else 
                        {
                            ScheduledContent content = scheduledContents[random.Next(0, scheduledContents.Count - 1)];
                            sb.AppendLine($"{content.Title} -> Added by {content.UserName} (ID = {content.ID})");
                        }
                    }
                    catch (Exception e)
                    {
                        embed.Color = new Color(255, 0, 0);
                        sb.AppendLine($"Sorry <@!{user.Id}>, I couldn't get the content you asked for but I got you this error instead: \n{e.Message}");
                    }
                    break;
                case "?":
                case "help":
                    embed.WithColor(new Color(0, 0, 0));
                    embed.Title = "Scheduling Help";

                    sb.AppendLine("You can use the following commands following $content:");
                    sb.AppendLine();
                    sb.AppendLine("type");
                    sb.AppendLine("version");
                    sb.AppendLine("filter [type];[version]");
                    sb.AppendLine("filter [typeID];[versionID]");
                    sb.AppendLine("add [DutyID]");
                    sb.AppendLine("remove [ScheduledContentID]");
                    sb.AppendLine("markcomplete [ScheduledContentID]");
                    sb.AppendLine("random");
                    sb.AppendLine("all");
                    sb.AppendLine("help");
                    sb.AppendLine("?");
                    sb.AppendLine();
                    sb.AppendLine("Please note the following:");
                    sb.AppendLine("1) For ease of use you can either type out the full type/version of content you want, or just use the IDs provided.");
                    sb.AppendLine("2) When marking content as complete, or removing it, be sure to use '$content all' to get the ID before attempting it.");
                    sb.AppendLine("3) You do not need to specify a user when adding content - it's always against your name.");
                    break;
                default:
                    sb.AppendLine($"Sorry <@!{user.Id}>, I didn't understand that.");
                    sb.AppendLine();
                    sb.AppendLine("Please type '$content help' or '$content ?' for a list of commands.");
                    break;
            }

            embed.Description = sb.ToString();
            await ReplyAsync(null, false, embed.Build());
        }

        [Command("swiftcraft")]
        public async Task MinecraftServerWork([Remainder]string args = null){
            var sb = new StringBuilder();
            var embed = new EmbedBuilder();
            var user = Context.User;

            var ipAddress = "192.168.0.41";

            switch (args.Trim().ToLower())
            {
                case "start":
                    embed.WithColor(new Color(0, 0, 0));
                    embed.Title = "Swiftcraft Start";

                    sb.AppendLine($"Huzzah! The server is now running. You can access it at {ipAddress}:25565.");
                    sb.AppendLine("Type '$swiftcraft status' at any time to check the status, and joining details (if it's running).");
                    break;
                case "status":
                    embed.WithColor(new Color(0, 0, 0));
                    embed.Title = "Swiftcraft Status";
                    var status = "running";

                    sb.AppendLine($"The server is now {status}. You can access it at {ipAddress}:25565.");
                    break;
                case "stop":
                    embed.WithColor(new Color(0, 0, 0));
                    embed.Title = "Swiftcraft Stop";

                    sb.AppendLine($"The server is now closed.");
                    break;
                case "help":
                case "?":
                    embed.WithColor(new Color(0, 0, 0));
                    embed.Title = "Swiftcraft Help";

                    sb.AppendLine("You can use the following commands with $swiftcraft:");
                    sb.AppendLine();
                    sb.AppendLine("start*");
                    sb.AppendLine("status");
                    sb.AppendLine("stop*");
                    sb.AppendLine("help");
                    sb.AppendLine("?");
                    sb.AppendLine();
                    sb.AppendLine("Please note the following:");
                    sb.AppendLine("1) You must be Swift in order to start/stop the server.");
                    sb.AppendLine("2) Anyone can query the status of the server at any time.");
                    sb.AppendLine("3) Just send Swift a DM at any time and he'll likely start the server straight away.");
                    break;
                default:
                    embed.WithColor(new Color(0, 0, 0));
                    embed.Title = "Swiftcraft Error";

                    sb.AppendLine($"Whatever you entered there (specifically {args}) didn't work. Type '$swiftcraft help' for help.");
                    break;
            }

            embed.Description = sb.ToString();
            await ReplyAsync(null, false, embed.Build());
        }
    }

}