using Autofac;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Windows;
using WellAnalyzing.Application;
using WellAnalyzing.Extnesions.DependencyInjections;
using WellAnalyzing.Views;

using WpfApplication = System.Windows.Application;

namespace WellAnalyzing
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : WpfApplication
    {
        private IContainer? _container;
        private ILogger<App>? _logger;

        /// <inheritdoc />
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);

                var containerBuilder = new ContainerBuilder();

                containerBuilder
                    .AddLogging()
                    .AddInternalServices()
                    .AddViewsWithDataContexts()
                    .RegisterModule<ApplicationModule>();

                _container = containerBuilder.Build();
                _logger = _container.Resolve<ILogger<App>>();

                var shell = _container.Resolve<ShellView>();

                shell.Show();

                _logger?.LogInformation("Приложение запущено");
            }
            catch (Exception ex)
            {
                var errMessage = "Приложение завршилось с критической ошибкой";
                _logger?.LogError(ex, "{ErrMessage}", errMessage);

                MessageBox.Show(errMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        /// <inheritdoc />
        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush();

            _logger?.LogInformation("Завершение работы");
            _container?.Dispose();

            base.OnExit(e);
        }
    }
}
