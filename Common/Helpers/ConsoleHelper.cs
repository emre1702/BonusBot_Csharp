using System;
using Color = System.Drawing.Color;
using static Colorful.Console;
using Discord;

namespace BonusBot.Common.Helpers
{
    public static class ConsoleHelper
    {
        private static readonly object _lockObj = new object();

        public static void Log(LogSeverity severity, string source, string message, Exception exception = null)
        {
            lock (_lockObj)
            {
                HandleLog(severity, source, message, exception);
            }
        }

        public static void PrintHeader()
        {
            var logo =
                @"
     _________  _________  _________  _________  _________  _________ ____   ____ _________  __________
    |    _o___)|    _o___)/    O    \/__     __\/    O    \/__     __\\___\_/___/|    _o___)/   /_____/
    |___|%%%%%'|___|\____\\_________/`%%|___|%%'\_________/`%%|___|%%' %%%/_\%%% |___|%%%%%'\___\%%%%%'
     `B'        `BB' `BBB' `BBBBBBB'     `B'     `BBBBBBB'     `B'        `B'     `B'        `BBBBBBBB'
";

            Append(logo, Color.Orchid);
            Append($"{Environment.NewLine}   {new String('=', 100)}", Color.AliceBlue);
            Write(Environment.NewLine);
        }

        private static void HandleLog(LogSeverity severity, string source, string message, Exception exception)
        {
            var (color, simplified) = ProcessLogSeverity(severity);
            Append($"    {simplified}", color);

            (color, simplified) = ProcessSource(source);
            Append($" -> {simplified} -> ", color);

            if (!string.IsNullOrWhiteSpace(message))
                Append(message, Color.White);

            if (exception != null)
                Append(exception.Message, Color.IndianRed);

            Write(Environment.NewLine);
        }

        private static void Append(string message, Color color)
        {
            ForegroundColor = color;
            Write(message);
        }

        private static (Color Color, string Simplified) ProcessSource(string source)
        => source switch
        {
            "Discord" 
                => (Color.RoyalBlue, "DSCD"),
            "Gateway" 
                => (Color.RoyalBlue, "DSCD"),
            "Victoria" 
                => (Color.Pink, "VCRA"),
            "Node#0" 
                => (Color.Pink, "VCRA"),
            "Node#0Socket" 
                => (Color.Pink, "VCRA"),
            "Job" 
                => (Color.DarkSalmon, "CORE"),
            "Core" 
                => (Color.DarkSalmon, "CORE"),
            "WebHook"
                => (Color.DeepSkyBlue, "WEBH"),
            _ => (Color.Gray, "UKWN")
        };

        private static (Color Color, string Simplified) ProcessLogSeverity(LogSeverity logSeverity)
            => logSeverity switch
        {
            LogSeverity.Info
                => (Color.Green, "INFO"),
            LogSeverity.Debug
                => (Color.SandyBrown, "DBUG"),
            LogSeverity.Error
                => (Color.Maroon, "EROR"),
            LogSeverity.Verbose
                => (Color.SandyBrown, "VROS"),
            LogSeverity.Warning
                => (Color.Yellow, "WARN"),
            LogSeverity.Critical
                => (Color.Maroon, "CRIT"),
            _ => (Color.Gray, "UKWN")
        };
    }
}
