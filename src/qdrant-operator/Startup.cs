using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Neon.Operator;

namespace QdrantOperator
{
    public class OperatorStartup
    {
        public IConfiguration Configuration { get; }

        public OperatorStartup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILoggerFactory>(LoggerFactory.Create(options =>
                {
                    options.ClearProviders();
                    options.AddJsonConsole();
                }));
            services.AddLogging();
            services.AddKubernetesOperator();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseKubernetesOperator();
        }
    }
}
