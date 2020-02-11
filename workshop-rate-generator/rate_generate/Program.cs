using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;

namespace rate_generate
{
    class Program
    {
        private static readonly AutoResetEvent waitHandle = new AutoResetEvent(false);
        static void Main(string[] args)
        {
            NLog.LogManager.GetCurrentClassLogger().Info("~~~~ Starting RateGenerator ~~~~");

            var servicesProvider = Startup.Init();


            Manager manager = servicesProvider.GetRequiredService<Manager>();
            Console.CancelKeyPress += (o, e) =>
            {
                Console.WriteLine("Exit");

                // Allow the main thread to continue and exit...
                waitHandle.Set();
            };
            //wait
            waitHandle.WaitOne();
        }

    }
}
