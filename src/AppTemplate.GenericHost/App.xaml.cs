using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AppTemplate.GenericHost
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost _host;
        private ILogger _logger;

        public App()
        {
            //https://laurentkempe.com/2019/09/03/WPF-and-dotnet-Generic-Host-with-dotnet-Core-3-0/
            //https://github.com/Valks/DotnetCore.GenericHost.WPF.Demo/blob/master/Demo.DI.WPF/Startup.cs

            var startup = new Startup();
            var dir = Directory.GetCurrentDirectory();
            _host = new HostBuilder()
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddJsonFile("hostsettings.json", optional: true);
                })
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.AddEnvironmentVariables();
                    configApp.AddJsonFile("appsettings.json",
                        optional: true, reloadOnChange: true);
                    configApp.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                        optional: true, reloadOnChange: true);
                })
                .ConfigureServices((hostContext, services) => startup.ConfigureServices(hostContext, services))
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                })
                .Build();
        }

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            await _host.StartAsync();

            //get the IOC container from the bootstrapper

            //resolve the logger
            _logger = null;
            //_logger.Log(LogLevel.Information, "Application Started.");

            //resolve main windows
            var mainWindow = _host.Services.GetService<MainWindow>();
            mainWindow.Show();
        }

        private async void Application_Exit(object sender, ExitEventArgs e)
        {
            //_logger.Info("Application Closed.");
            await _host.StopAsync();
            _host.Dispose();
            _host = null;
        }

        #region Unhandled Exceptions
        /// <summary>
        /// Setup unhandled exception handlers
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                Current.DispatcherUnhandledException += GlobalExceptionHandler;
                AppDomain.CurrentDomain.UnhandledException += AppDomainExceptionHandler;

                base.OnStartup(e);
            }
            catch (Exception exception)
            {
                //_logger.Log(LogLevel.Critical, $"Application initialization failed. {exception.Message}");
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// For exceptions thrown by any thread other than UI thread (i.e. threads spawned by Tasks).
        /// This exceptions cannot be handled.
        /// </summary>
        /// <remarks>
        /// https://www.engineerspock.com/2016/03/15/global-exceptions-handling-in-wpf/
        /// </remarks>
        private void AppDomainExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            HandleUnhandledException(e.ExceptionObject as Exception);
        }

        /// <summary>
        /// For exceptions thrown in the UI thread.
        /// </summary>
        private void GlobalExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            HandleUnhandledException(e.Exception);
        }

        private void HandleUnhandledException(Exception exception)
        {
            //_logger.Log(exception.GetBaseException(), "Application Unhandled exception.");
            //show message
        }
        #endregion
    }
}
