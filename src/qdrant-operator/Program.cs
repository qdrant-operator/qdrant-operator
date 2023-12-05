using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Threading.Tasks;

namespace QdrantOperator
{
    public static partial class Program
    {
        public static async Task Main(string[] args)
        {
            var host =
                Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseStartup<OperatorStartup>();
               });

            host.ConfigureLogging(
                logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                });

            await host.Build().RunAsync();
        }
    }
}