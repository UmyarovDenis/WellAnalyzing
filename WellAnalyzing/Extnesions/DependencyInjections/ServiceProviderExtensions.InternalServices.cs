using Autofac;
using Autofac.Extensions.DependencyInjection;
using WellAnalyzing.Services;
using WellAnalyzing.Services.Interfaces;

namespace WellAnalyzing.Extnesions.DependencyInjections;

public static partial class ServiceProviderExtensions
{
    /// <summary>
    /// Рагистрирует внутренние сервисы.
    /// </summary>
    /// <param name="builder"><see cref="ContainerBuilder"/>.</param>
    public static ContainerBuilder AddInternalServices(this ContainerBuilder builder)
    {
        builder.Populate([]);

        builder
            .RegisterType<FileService>()
            .As<IFileService>()
            .SingleInstance();

        builder
            .RegisterType<NotificationService>()
            .As<INotificationService>()
            .SingleInstance();

        return builder;
    }
}
