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
        public static async Task Main(string[] args)
        {
            var host = KubernetesOperatorHost
               .CreateDefaultBuilder()
               .ConfigureOperator(settings =>
               {
                   settings.AssemblyScanningEnabled = false;
                   settings.Name                    = "qdrant-operator";
               })
               .UseStartup<OperatorStartup>();

            var tracingOtlpEndpoint = Environment.GetEnvironmentVariable("TRACING_OTLP_ENDPOINT");

            host.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                .AddService(serviceName: TraceContext.ActivitySourceName))
                .WithTracing(tracing =>
                {
                    tracing.AddAspNetCoreInstrumentation();
                    tracing.AddHttpClientInstrumentation();
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