using System;
using System.IO;

namespace GTA.NPCTest.src.utils
{
    public static class Logger
    {
        /// <summary>
        /// logs the message to a file... 
        /// but only if the log level defined in the mod options is greater or equal to the message's level
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logLevel">The smaller the level, the more relevant, with 1 being very important failure messages and 5 being max debug spam</param>
        public static void INFO(object message)
        {
                File.AppendAllText(".\\data\\NPCTest.log", DateTime.Now + " :[INFO] " + message + Environment.NewLine, encoding: System.Text.Encoding.UTF8);
        }

        public static void ERROR(Exception ex)
        {
            File.AppendAllText(".\\data\\NPCTest.log", DateTime.Now + 
                " :[ERROR][" + ex.Source + "] " + ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine, encoding: System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// overwrites the log file's content with a "cleared log" message
        /// </summary>
        public static void ClearLog()
        {
            File.WriteAllText(".\\data\\NPCTest.log", DateTime.Now + " : " + "Cleared log! (This happens when the mod is initialized)" + Environment.NewLine);
        }
    }
}
