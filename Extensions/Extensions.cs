using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Discord;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace csharpi.Extensions
{
	public static class Extensions
    {        
        public static async Task PrintToConsole(this LogMessage logMessage)
        {
            await Handlers.ConsoleHandler.Log(logMessage);
        }
        
        public static async void PrintLogMessage(this string source, string message, LogSeverity logSeverity = LogSeverity.Info)
        {
            var logMessage = new LogMessage(logSeverity, source, message);
            await logMessage.PrintToConsole();
        }

        public static string[] RowStrings(this DataRow row)
        {
            string[] strings = new string[row.ItemArray.Length];

            for (int i = 0; i < row.ItemArray.Length; i++)
            {
                strings[i] = row.ItemArray.GetValue(i).ToString();
            }

            return strings;
        }
    }

    public struct MySqlStoredProcedure
        {
            public string Name {get; private set;}
            public List<MySqlParameter> Parameters {get;private set;}
            public MySqlConnection Connection {get;private set;}

            public MySqlStoredProcedure(string name, MySqlConnection connection)
            {
                Name = name;
                Parameters = new List<MySqlParameter>();
                Connection = connection;
            }

            public MySqlStoredProcedure(string name, MySqlParameter[] parameters, MySqlConnection connection)
            {
                Name = name;
                Parameters = new List<MySqlParameter>();
                Parameters.AddRange(parameters.ToList());
                Connection = connection;
            }

            public static implicit operator MySqlCommand(MySqlStoredProcedure procedure)
            {
                MySqlCommand command = new MySqlCommand(procedure.Name, procedure.Connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddRange(procedure.Parameters.ToArray());

                return command;
            }
        }
}