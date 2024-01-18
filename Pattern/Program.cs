using System;
using System.Threading.Tasks;

namespace ChatTest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Сервер запущен");
                return;
            }

            string name = args[0];
            int localPort = int.Parse(args[1]);

            CancellationTokenSource cts = new CancellationTokenSource();

        
            await Client.SendMsg(name, localPort, cts.Token);

       
            cts.Cancel();
        }
    }
}
