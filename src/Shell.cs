using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KodeRunner
{
    
    internal class Shell
    {
        // TODO: replace this at somepoint with the Powershell SDK for better cross platform support
        public static string GetShell()
        {
            if (OperatingSystem.IsWindows())
            {
                return "powershell.exe";
            }
            else
            {
                return "/bin/bash";
            }
        }

        internal static bool Run(CancellationTokenSource cts)
        {
            Console.Write("> ");

            var i = Console.ReadLine();
            // explicitly check for null to avoid possible null reference exceptions and 
            // to see if the user wants to exit in a case insensitive manner
            if (i != null && i.ToLower() == "exit")
            {
                return true;
            }

            switch (i)
            {
                case "help":
                    Console.WriteLine("Available commands:");
                    Console.WriteLine("  help - Show this help message");
                    Console.WriteLine("  exit - Stop the server and exit the application");
                    break;
                default:
                    if (!string.IsNullOrWhiteSpace(i))
                    {
                        Console.WriteLine($"Unknown command: {i}. Type 'help' for a list of commands.");
                    }
                    break;
            }
            return false;
        }
    }
}
