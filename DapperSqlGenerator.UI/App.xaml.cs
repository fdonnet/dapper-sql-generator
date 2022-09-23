using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace DapperSqlGenerator.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider _serviceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            //Services init
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();

            var mainWindows = _serviceProvider.GetRequiredService<MainWindow>();

            mainWindows.Show();

        }

        private void ConfigureServices(IServiceCollection services)
        {
            //Only one main windows
            services.AddSingleton<MainWindow>();
        }
    }
}
