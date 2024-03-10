using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Neon.Common;
using Neon.Diagnostics;
using Neon.Operator;

using Prometheus;

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
            services.AddLogging();
            services.AddKubernetesOperator();

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