using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
            
            var http = ServiceProvider.GetRequiredService<HTTP_Client>();
            await http.Connect();
            await http.Get_ID();

            // Загружаем данные и показываем оболочку
            var mainViewModel = ServiceProvider.GetRequiredService<MainViewModel>();
            

            var EventViewModel = ServiceProvider.GetRequiredService<EventsViewModel>();

            var shellWindow = ServiceProvider.GetRequiredService<ShellWindow>();
            shellWindow.DataContext = ServiceProvider.GetRequiredService<ShellViewModel>();
            shellWindow.Show();
        }

        

        private void ConfigureServices(IServiceCollection services)
        {
            // Регистрация ViewModels
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<EventsViewModel>();
            services.AddSingleton<ShellViewModel>();
            services.AddSingleton<AppStateViewModel>();

            // Регистрация сервисов (если есть)
            services.AddSingleton<HTTP_Client>();

            // Регистрация окон
            services.AddSingleton<ShellWindow>();
        }

        
    }
}
