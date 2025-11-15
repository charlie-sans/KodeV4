using System;
using System.Threading;
using System.Threading.Tasks;

namespace KodeRunner
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var httpService = new Services.HTTPService();
            var cts = new CancellationTokenSource();

            var serverTask = Task.Run(async () => await httpService.StartServer(8081, cts.Token), cts.Token);
            // Show routes discovered
            httpService.DumpRoutes();
            // Console.Clear();
            Console.WriteLine("Welcome to KodeRunner!");
            Console.WriteLine("Type 'help' for a list of commands, or 'exit' to quit.");


            // while (!cts.Token.IsCancellationRequested)
            // {
            //     // i aint doing all this in the main func.
            //     var StopServer = Shell.Run(cts);
            //     if (StopServer)
            //     {
            //         cts.Cancel();
            //     }
            // }
            
            await serverTask;
            Console.WriteLine("Server stopped.");
        }
    }
}