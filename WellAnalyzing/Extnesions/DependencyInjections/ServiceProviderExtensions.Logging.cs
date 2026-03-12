using Autofac;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;

namespace WellAnalyzing.Extnesions.DependencyInjections;

public static partial class ServiceProviderExtensions
{
    /// <summary>
    /// Добавляет поддержку <see cref="Serilog.ILogger"/>.
    /// </summary>
    /// <param name="builder"><see cref="ContainerBuilder"/>.</param>
    public static ContainerBuilder AddLogging(this ContainerBuilder builder)
    {
        ConfigureLogging();

        builder
            .RegisterType<LoggerFactory>()
            .As<ILoggerFactory>()
            .SingleInstance()
            .OnActivated(args => args.Instance.AddSerilog(Log.Logger));

        builder
            .RegisterGeneric(typeof(Logger<>))
            .As(typeof(ILogger<>))
            .SingleInstance();

        builder.Register(c =>
        {
            var factory = c.Resolve<ILoggerFactory>();
            return factory.CreateLogger("Default");
        })
        .As<Microsoft.Extensions.Logging.ILogger>()
        .SingleInstance();

        return builder;
    }

    private static void ConfigureLogging()
    {
        Log.Logger =
            new LoggerConfiguration()
                .MinimumLevel
                .Debug()
                .WriteTo.File(
                    path: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
                )
                .CreateLogger();
    }
}
