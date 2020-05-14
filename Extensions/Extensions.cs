using Discord;
using System.Threading.Tasks;

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
    }
}