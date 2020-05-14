using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using csharpi.Common;

namespace csharpi.Extensions
{
    public static class Methods
    {
        public static async void PrintConsoleSplitLine()
        {
            await new LogMessage(LogSeverity.Info, "",
                "----------------------------------------------------------------------").PrintToConsole();
        }
    }
}