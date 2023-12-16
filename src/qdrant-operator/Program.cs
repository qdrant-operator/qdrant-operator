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
    public static partial class Program
    {
        public const string ServiceName = "qdrant-operator";
        public static async Task Main(string[] args)
        {
            var listenPort = 80;
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

            host.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                .AddService(serviceName: TraceContext.ActivitySourceName))
                .WithTracing(tracing =>
                {
                    tracing.AddAspNetCoreInstrumentation();
                    tracing.AddKubernetesOperatorInstrumentation();
                    tracing.AddSource(TraceContext.ActivitySourceName);
                    if (tracingOtlpEndpoint != null)
                    {
                        tracing.AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(tracingOtlpEndpoint);
                        });
                    }
                    else
                    {
                        tracing.AddConsoleExporter();
                    }
                });

            await host.Build().RunAsync();
        }
    }
}