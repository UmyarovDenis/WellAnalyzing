using Autofac;
using System.Windows;
using WellAnalyzing.Views;

namespace WellAnalyzing.Extnesions.DependencyInjections;

public static partial class ServiceProviderExtensions
{
    /// <summary>
    /// Связыват View и ViewModel главной оболочки интерфейса приложения.
    /// </summary>
    /// <param name="builder"><see cref="ContainerBuilder"/>.</param>
    public static ContainerBuilder AddViewsWithDataContexts(this ContainerBuilder builder) =>
        builder
            .BindViewToViewModel<ShellView, ShellViewModel>();

    private static ContainerBuilder BindViewToViewModel<TView, TViewModel>(
        this ContainerBuilder builder)
        where TView : FrameworkElement
        where TViewModel : class
    {
        builder
            .RegisterType<TViewModel>()
            .AsSelf()
            .AsImplementedInterfaces()
            .InstancePerDependency();

        builder
            .RegisterType<TView>()
            .AsSelf()
            .InstancePerDependency()
            .OnActivated(a => a.Instance.DataContext = a.Context.Resolve<TViewModel>());

        return builder;
    }
}
