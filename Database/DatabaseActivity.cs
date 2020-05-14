using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using csharpi.Common;
using csharpi.Extensions;
using MySql.Data.MySqlClient;

namespace csharpi.Database
{
    public static class DatabaseActivity
    {        
        internal static void EnsureExists()
        {
            TestDatabaseSettings().GetAwaiter().GetResult();
            
            Methods.PrintConsoleSplitLine();
        }
        
        internal static async Task<(bool connectionValid, bool databaseExists)> TestDatabaseSettings()
        {
            MySqlConnection connection;
            
            await new LogMessage(LogSeverity.Info, "Database Validation", "Verifying database settings.").PrintToConsole();
            
            // test to see if connection is valid.
            try
            {
                connection = new MySqlConnection(ConnectionString);
                connection.Open();
                connection.Close();
                await new LogMessage(LogSeverity.Info, "Database Validation", "Connection to database established.").PrintToConsole();
            }
            catch (Exception e)
            {
                await new LogMessage(LogSeverity.Error, "Database Validation", e.Message).PrintToConsole();
                return (false, false);
            }
            
            // test to see if database exists - required to be after connection is valid.            
            await new LogMessage(LogSeverity.Info, "Database Validation", "Checking for database.").PrintToConsole();
            
            try
            {
                connection = new MySqlConnection(ConnectionString);
                connection.Open();
                connection.Close();
                
                await new LogMessage(LogSeverity.Info, "Database Validation", "Database exists.").PrintToConsole();

                return (true, true);
            }
            catch (Exception)
            {
                await new LogMessage(LogSeverity.Info, "Database Validation", "Unable to find database.").PrintToConsole();
                return (true, false);
            }
        }

        private static MySqlConnection GetDatabaseConnection()
        {
            MySqlConnection connection = new MySqlConnection(ConnectionString);
            
            try
            {
                return connection;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        internal static int ExecuteNonQueryCommand(string query, List<(string name, string value)> queryParams = null)
        {
            using (var conn = GetDatabaseConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(query, conn);
                cmd.Prepare();
            
                if (queryParams != null)
                {
                    foreach(var (name, value) in queryParams)
                    {
                        var param = new MySqlParameter
                        {
                            ParameterName = name, 
                            Value = value
                        };

                        cmd.Parameters.Add(param);
                    }
                }

                try
                {
                    int rows = cmd.ExecuteNonQuery();
                    conn.CloseAsync();

                    new LogMessage(LogSeverity.Debug, "Database Command",cmd.CommandText).PrintToConsole().GetAwaiter();
                    new LogMessage(LogSeverity.Debug, "Database Response", "Rows Updated: " + rows).PrintToConsole().GetAwaiter();

                    return rows;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            }
        }

        internal static (MySqlDataReader,MySqlConnection) ExecuteReader(string query)
        {
            MySqlConnection conn = GetDatabaseConnection();
            MySqlCommand cmd = new MySqlCommand(query, conn);
            
            conn.Open();
            
            try
            {
                return (cmd.ExecuteReader(), conn);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
            
        }

        static string ConnectionString { get => $"Server=127.0.0.1; Database=SlipperyWhiskers; Uid=swiftbot; Pwd={Configuration.GetDBPassword()}"; }
    }
}