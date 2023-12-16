using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Neon.Operator;

namespace QdrantOperator
{
    public static partial class Program
    {
        public static async Task Main(string[] args)
        {
            var host = KubernetesOperatorHost
               .CreateDefaultBuilder()
               .ConfigureOperator(settings =>
               {
                   settings.AssemblyScanningEnabled = false;
                   settings.Name                    = "qdrant-operator";
               })
               .UseStartup<OperatorStartup>()
               .Build();

            await host.RunAsync();
        }
    }
}