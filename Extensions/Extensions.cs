using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Discord;
using System.Threading.Tasks;
using System.Data.Sql;
using System.Data.SqlClient;

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

    public struct SqlStoredProcedure
    {
        public string Name {get; private set;}
        public List<SqlParameter> Parameters {get;private set;}
        public SqlConnection Connection {get;private set;}

        public SqlStoredProcedure(string name, SqlConnection connection)
        {
            Name = name;
            Parameters = new List<SqlParameter>();
            Connection = connection;
        }

        public SqlStoredProcedure(string name, SqlParameter[] parameters, SqlConnection connection)
        {
            Name = name;
            Parameters = new List<SqlParameter>();
            Parameters.AddRange(parameters.ToList());
            Connection = connection;
        }

        public static implicit operator SqlCommand(SqlStoredProcedure procedure)
        {
            SqlCommand command = new SqlCommand(procedure.Name, procedure.Connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddRange(procedure.Parameters.ToArray());

            return command;
        }
    }
}