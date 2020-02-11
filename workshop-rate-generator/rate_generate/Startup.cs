using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using System.Collections.Generic;
using System.Text;

namespace rate_generate
{
    public class Startup
    {
        public static IServiceProvider Init()
        {

            IConfiguration config = LoadConfiguration();

            var servicesProvider = BuildDependencyInjector(config);

            var logger = servicesProvider.GetRequiredService<ILogger<Program>>();
            logger.LogDebug("kubemq_msmq_rates_generator.Setup: Loaded configuration, logger and Dependency Injector ");

            return servicesProvider;
        }

        private static IConfiguration LoadConfiguration()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json", optional: true)
               .AddEnvironmentVariables()
               .Build();

            var Configuration = config;

            return config;

        }

        private static IServiceProvider BuildDependencyInjector(IConfiguration config)
        {
            var services = new ServiceCollection();
            services.AddSingleton<Manager>();
            services.AddSingleton(config);
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddLogging((builder) => builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace));

            var serviceProvider = services.BuildServiceProvider();

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
            loggerFactory.ConfigureNLog("nlog.config");
            return serviceProvider;
        }
    }
}
