using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace StrausRadio
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.CancelKeyPress += delegate
            {
                WorkerFlags.Stop = true;
            };
            
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                });
    }
}
