using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Neon.Common;
using Neon.Diagnostics;
using Neon.K8s;
using Neon.K8s.PortForward;
using Neon.Operator;

using Prometheus;

using QdrantOperator.Util;

using Quartz;

namespace QdrantOperator
{
    /// <summary>
    /// The Startup class.
    /// </summary>
    public class OperatorStartup
    {
        /// <summary>
        /// The Configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="configuration"></param>
        public OperatorStartup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Service configuration.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            var loggerFactory = LoggerFactory.Create(options =>
            {
                if (NeonHelper.IsDevWorkstation)
                {
                    options.SetMinimumLevel(LogLevel.Debug);
                }

                options.ClearProviders();
                options.AddJsonConsole();
            });

            var logger = loggerFactory.CreateLogger<OperatorStartup>();

            services.AddSingleton<ILoggerFactory>(loggerFactory);
            services.AddSingleton<ClusterHelper>();
            services.AddLogging();
            services.AddKubernetesOperator();
            services.AddQuartz(q =>
            {
                q.SchedulerId = "qdrant-operator";
                q.UseSimpleTypeLoader();
                q.UseInMemoryStore();
                q.UseDefaultThreadPool(tp =>
                {
                    tp.MaxConcurrency = 10;
                });
            });

            if (NeonHelper.IsDevWorkstation)
            {
                services.AddSingleton<PortForwardManager>((ctx) =>
                {
                    var loggerFactory = ctx.GetService<ILoggerFactory>();
                    var k8s = KubeHelper.GetKubernetesClient();
                    return new PortForwardManager(k8s, loggerFactory);
                });
            }

            var metricsPort = 9762;

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("METRICS_PORT")))
            {
                int.TryParse(Environment.GetEnvironmentVariable("METRICS_PORT"), out metricsPort);
            }

            if (!NeonHelper.IsDevWorkstation)
            {
                logger?.LogInformationEx(() => $"Configuring metrics port: {metricsPort}");

                services.AddMetricServer(options =>
                {
                    options.Port = (ushort)metricsPort;
                });
            }
        }

        /// <summary>
        /// App configuration.
        /// </summary>
        /// <param name="app"></param>
        public void Configure(IApplicationBuilder app)
        {
            app.UseKubernetesOperator();
        }
    }
}