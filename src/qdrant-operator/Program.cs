using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Neon.Operator;

using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace QdrantOperator
{
    /// <summary>
    /// The Program.
    /// </summary>
    public static partial class Program
    {
        /// <summary>
        /// The service name.
        /// </summary>
        public const string ServiceName = "qdrant-operator";

        /// <summary>
        /// The program entry point.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task Main(string[] args)
        {
            var listenPort = 5000;

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LISTEN_PORT")))
            {
                int.TryParse(Environment.GetEnvironmentVariable("LISTEN_PORT"), out listenPort);
            }

            var host = KubernetesOperatorHost
               .CreateDefaultBuilder()
               .ConfigureOperator(settings =>
               {
                   settings.AssemblyScanningEnabled = false;
                   settings.Name                    = ServiceName;
                   settings.Port                    = listenPort;
               })
               .UseStartup<OperatorStartup>();

            var tracingOtlpEndpoint = Environment.GetEnvironmentVariable("TRACING_OTLP_ENDPOINT");

            if (!string.IsNullOrEmpty(tracingOtlpEndpoint))
            {
                host.Services.AddOpenTelemetry()
                    .ConfigureResource(resource => resource
                    .AddService(serviceName: TraceContext.ActivitySourceName))
                    .WithTracing(tracing =>
                    {
                        tracing.AddAspNetCoreInstrumentation();
                        tracing.AddKubernetesOperatorInstrumentation();
                        tracing.AddSource(TraceContext.ActivitySourceName);
                        tracing.AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(tracingOtlpEndpoint);
                        });
                    });
            }

            await host.Build().RunAsync();
        }
    }
}