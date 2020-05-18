using Discord;
using Discord.Net;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using csharpi.Extensions;
using csharpi.Database;
using csharpi.Common;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace csharpi.Modules
{
    // for commands to be available, and have the Context passed to them, we must inherit ModuleBase
    public class Commands : ModuleBase
    {
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
                "hello",
                "ask"
            };

            sb.AppendLine($"Here is a list of all commands available through Swift Bot: ");
            sb.AppendLine();

            foreach (string s in commandsList)
            {
                sb.AppendLine($"\"{s}\"");
            }

            sb.AppendLine();
            sb.AppendLine($"Just remember to use \'$\' at the beginning of each command.");
            sb.AppendLine();
            sb.AppendLine($"Happy commanding...!");

            embed.WithDescription(sb.ToString());

            await ReplyAsync(null, false, embed.Build());
        }

        [Command("hello")]
        public async Task HelloCommand()
        {
            var sb = new StringBuilder();
            var user = Context.User;
            
            sb.AppendLine($"You are -> {user.Username}");
            sb.AppendLine("I must now say, World!");

            await ReplyAsync(sb.ToString());
        }

        [Command("8ball")]
        [Alias("ask")]
        //[RequireUserPermission(GuildPermission.KickMembers)]
        public async Task AskEightBall([Remainder]string args = null)
        {
            var sb = new StringBuilder();
            var embed = new EmbedBuilder();

            var user = Context.User;
            string question = Context.Message.ToString().Replace("$8ball ", string.Empty).Replace("$ask ", string.Empty);

            var replies = new List<string>();

            replies.Add("yes");
            replies.Add("no");
            replies.Add("maybe");
            replies.Add("hazzzzy....");
            replies.Add("You tell me!");

            embed.WithColor(new Color(0, 255,0));
            embed.Title = "Welcome to the 8-ball!";
            
            sb.AppendLine($"Thanks for the question {user.Username}!");
            sb.AppendLine();

            if (args == null)
            {
                sb.AppendLine("Sorry, can't answer a question you didn't ask!");
            }
            else 
            {
                var answer = replies[new Random().Next(replies.Count - 1)];
                
                sb.AppendLine($"You asked: {question}...");
                sb.AppendLine();
                sb.AppendLine($"...your answer is {answer.ToString()}");

                switch (answer) 
                {
                    case "yes":
                    {
                        embed.WithColor(new Color(0, 255, 0));
                        break;
                    }
                    case "no":
                    {
                        embed.WithColor(new Color(255, 0, 0));
                        break;
                    }
                    case "maybe":
                    {
                        embed.WithColor(new Color(255,255,0));
                        break;
                    }
                    case "hazzzzy....":
                    {
                        embed.WithColor(new Color(255,0,255));
                        break;
                    }
                    case "You tell me!":
                    {
                        embed.WithColor(new Color(50,50,50));
                        break;
                    }
                }
            }

            embed.Description = sb.ToString();
            await ReplyAsync(null, false, embed.Build());
        }

        [Command("weekdays")]
        public async Task GetWeekdays()
        {
            MySqlConnection connection = new MySqlConnection(Database.DatabaseActivity.ConnectionString);
            connection.Open();

            List<string[]> rowStrings = new List<string[]>();

            using (MySqlDataAdapter adapter = new MySqlDataAdapter(new MySqlStoredProcedure("usp_Get_Weekdays", connection)))
            {
                DataSet data = new DataSet();
                adapter.Fill(data);

                foreach (DataRow r in data.Tables[0].Rows)
                {
                    rowStrings.Add(r.RowStrings());
                }
            }

            connection.Close();
            
            var sb = new StringBuilder();
            var embed = new EmbedBuilder();

            embed.WithColor(new Color(0, 255,0));
            embed.Title = "Weekdays, yo!";

            foreach (string[] array in rowStrings)
            {
                string a = "";

                foreach (string s in array)
                {
                    a = a == "" ? s : a + $";{s}";
                }

                sb.AppendLine(a);
            }

            embed.Description = sb.ToString();
            await ReplyAsync(null, false, embed.Build());
        }
    }
}